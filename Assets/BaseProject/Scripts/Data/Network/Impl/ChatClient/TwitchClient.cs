using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Unity;
using UniRx;
using UnityEngine;
using TwitchChatMessage = TwitchLib.Client.Models.ChatMessage;

 namespace CreativeMode.Impl
{
    public class TwitchClient : IChatClient
    {
        private const string twitchEmotePlaceholder1x = "https://static-cdn.jtvnw.net/emoticons/v1/{0}/1.0";
        private const string twitchEmotePlaceholder2x = "https://static-cdn.jtvnw.net/emoticons/v1/{0}/2.0";
        private const string twitchEmotePlaceholder4x = "https://static-cdn.jtvnw.net/emoticons/v1/{0}/4.0";
        
        private const string bttvGlobalEmoteApi = "https://api.betterttv.net/2/emotes";
        private const string bttvChannelEmoteApi = "https://api.betterttv.net/2/channels/{0}";
        private const string ffzEmoteApi = "https://api.frankerfacez.com/v1/room/{0}";
        
        public IObservable<ChatMessageRemote> ChatMessages { get; }
        public IObservable<Unit> OnChatCleared { get; }

        private IObservable<Client> Client { get; }
        private Dictionary<string, Color> dynamicUserColors = new Dictionary<string, Color>();

        public TwitchClient(string oauth, string username, string joinChannel)
        {
            Client = CreateTwitchClientObservable(oauth, username, joinChannel);
            ChatMessages = CreateChatMessageObservable(joinChannel);
            OnChatCleared = CreateClearChatObservable();
        }

        private IObservable<Client> CreateTwitchClientObservable(string oauth, string username, string joinChannel)
        {
            if (string.IsNullOrWhiteSpace(oauth) || 
                string.IsNullOrWhiteSpace(username) || 
                string.IsNullOrWhiteSpace(joinChannel))
            {
                // TODO: move auth check to higher level
                Debug.LogWarning("Disabling twitch client, no credentials specified");
                return Observable.Empty<Client>();
            }
            
            return Observable.CreateSafe<Client>(s =>
            {
                var client = new Client();
                var credentials = new ConnectionCredentials(username, oauth);

                Debug.Log("Twitch client creation start");
                
                client.OnConnected += (sender, args) => Debug.Log("Twitch client connected");
                client.OnDisconnected += (sender, args) => Debug.Log("Twitch client disconnected");
                client.OnMessageReceived += (sender, args) => Debug.Log($"Twitch message: {args.ChatMessage.Message}");
                client.OnJoinedChannel += (sender, args) => Debug.Log($"Twitch join channel: {args.Channel}");

                client.OnConnectionError += (sender, args) =>
                {
                    Debug.Log($"Twitch connection error: {args.Error.Message}");
                    client.Connect();
                };
                
                client.Initialize(credentials, joinChannel);
                client.Connect();
                
                Debug.Log("Twitch client creation end");
                s.OnNext(client);

                return Disposable.Create((() => { client.Disconnect(); }));
            }).Replay(1).RefCount()
                .ObserveOn(Scheduler.MainThread)
                .SubscribeOn(Scheduler.MainThread);
        }

        private IObservable<ChatMessageRemote> CreateChatMessageObservable(string channelName)
        {
            var bttvEmotes = GetExternalEmotesForChannel(channelName)
                .Replay(1).RefCount();

            var chatMessages = Client.SelectMany(c =>
            {
                return Observable.FromEvent<EventHandler<OnMessageReceivedArgs>, TwitchChatMessage>(
                    h => (s, e) => h(e.ChatMessage),
                    h => c.OnMessageReceived += h,
                    h => c.OnMessageReceived -= h);
            });

            return chatMessages.CombineLatest(bttvEmotes, ConvertMessage)
                .SubscribeOn(Scheduler.ThreadPool)
                .ObserveOn(Scheduler.MainThread)
                .Share();
        }

        private IObservable<Unit> CreateClearChatObservable()
        {
            return Client.SelectMany(c =>
            {
                return Observable.FromEvent<EventHandler<OnChatClearedArgs>, Unit>(
                    h => (s, e) => h(Unit.Default),
                    h => c.OnChatCleared += h,
                    h => c.OnChatCleared -= h);
            }).Share();
        }
        
        private ChatMessageRemote ConvertMessage(TwitchChatMessage message, ExternalEmote[] externalEmotes)
        {
            var emotes = new List<ChatMessageRemote.Emote>();
            var emoteSpans = new List<EmoteSpan>();
            emoteSpans.AddRange(GetEmoteSpansFromTwitch(message.EmoteSet));
            emoteSpans.AddRange(GetExternalEmoteSpans(message.Message, externalEmotes));
            emoteSpans.Sort((l, r) => l.startIndex - r.startIndex);

            foreach (var e in emoteSpans)
            {
                emotes.Add(new ChatMessageRemote.Emote
                {
                    startIndex = e.startIndex,
                    endIndex = e.endIndex,
                    isModifier = e.modifierEmote,
                    url1x = e.url1x,
                    url2x = e.url2x,
                    url4x = e.url4x,
                });
            }

            if (!ColorUtility.TryParseHtmlString(message.ColorHex, out var authorColor) && 
                !dynamicUserColors.TryGetValue(message.Username, out authorColor))
            {
                authorColor = UnityEngine.Random.ColorHSV(0, 1, 
                    0.5f, 1f, 0.75f, 1f);
                dynamicUserColors[message.Username] = authorColor;
            }

            return new ChatMessageRemote
            {
                authorId = message.UserId,
                author = message.DisplayName,
                authorColor = authorColor,
                isBroadcaster = message.IsBroadcaster,
                isModerator = message.IsModerator,
                message = message.Message,
                messageEmotes = emotes.ToArray()
            };
        }

        private IEnumerable<EmoteSpan> GetEmoteSpansFromTwitch(EmoteSet set)
        {
            return set.Emotes.Select(e => new EmoteSpan
            {
                url1x = string.Format(twitchEmotePlaceholder1x, e.Id),
                url2x = string.Format(twitchEmotePlaceholder2x, e.Id),
                url4x = string.Format(twitchEmotePlaceholder4x, e.Id),
                startIndex = e.StartIndex,
                endIndex = e.EndIndex + 1
            });
        }

        private IEnumerable<EmoteSpan> GetExternalEmoteSpans(string text, ExternalEmote[] emotes)
        {
            var result = new List<EmoteSpan>();
            var scanIndex = 0;

            int AdvanceToToken(bool whitespace)
            {
                while (scanIndex < text.Length)
                {
                    if (char.IsWhiteSpace(text[scanIndex]) == whitespace)
                        return scanIndex;

                    scanIndex++;
                }

                return scanIndex;
            }

            bool TextContains(int offset, string value)
            {
                for (var i = 0; i < value.Length; i++)
                {
                    if (text[i + offset] != value[i])
                        return false;
                }

                return true;
            }

            while (scanIndex < text.Length)
            {
                var startIndex = AdvanceToToken(false);
                var endIndex = AdvanceToToken(true);
                var length = endIndex - startIndex;

                foreach (var e in emotes)
                {
                    if (e.name.Length == length && TextContains(startIndex, e.name))
                    {
                        result.Add(new EmoteSpan
                        {
                            startIndex = startIndex,
                            endIndex = endIndex,
                            modifierEmote = e.isModifier,
                            url1x = e.url1x,
                            url2x = e.url2x,
                            url4x = e.url4x
                        });
                        break;
                    }
                }
            }
            
            return result;
        }

        private IObservable<ExternalEmote[]> GetExternalEmotesForChannel(string channelName)
        {
            return GetBttvEmoteByUrl(bttvGlobalEmoteApi)
                .Merge(GetBttvEmoteByChannelName(channelName))
                .Merge(GetFfzEmotesByChannelName(channelName))
                .ToList()
                .Select(l => l.SelectMany(e => e).ToArray());
        }

        private IObservable<ExternalEmote[]> GetBttvEmoteByChannelName(string channelName)
        {
            if (string.IsNullOrWhiteSpace(channelName))
                return Observable.Empty<ExternalEmote[]>();

            return GetBttvEmoteByUrl(string.Format(bttvChannelEmoteApi, channelName));
        }
        
        private IObservable<ExternalEmote[]> GetBttvEmoteByUrl(string url)
        {
            return Observable.Start(() =>
            {
                using (var stream = WebRequest.Create(url)
                    .GetResponse()
                    .GetResponseStream())
                {
                    var reader = new StreamReader(stream);
                    var jsonText = reader.ReadToEnd();
                    var response = JsonConvert.DeserializeObject<BttvResponse>(jsonText);
                    
                    return response.emotes.Select(e =>
                    {
                        string GetEmoteUrl(string size)
                        {
                            return "http:" + response.urlTemplate
                                .Replace("{{id}}", e.id)
                                .Replace("{{image}}", size);
                        }
                        
                        return new ExternalEmote
                        {
                            name = e.code,
                            isModifier = IsModifierEmote(e.code),
                            url1x = GetEmoteUrl("1x"),
                            url2x = GetEmoteUrl("2x"),
                            url4x = GetEmoteUrl("3x"),
                        };
                    }).ToArray();
                }
            });
        }

        private IObservable<ExternalEmote[]> GetFfzEmotesByChannelName(string channelName)
        {
            if (string.IsNullOrWhiteSpace(channelName))
                return Observable.Empty<ExternalEmote[]>();

            return Observable.Start(() =>
            {
                using (var stream = WebRequest.Create(string.Format(ffzEmoteApi, channelName))
                    .GetResponse()
                    .GetResponseStream())
                {
                    var reader = new StreamReader(stream);
                    var jsonText = reader.ReadToEnd();
                    var response = JsonConvert.DeserializeObject<FrankerFaceZResponse>(jsonText);
                    var icons = response.sets.FirstOrDefault().Value.emoticons;

                    if(icons == null)
                        return new ExternalEmote[0];

                    return icons.Select(e =>
                    {
                        string GetEmoteUrl(string size)
                        {
                            if (!e.urls.TryGetValue(size, out var emoteUrl))
                                emoteUrl = e.urls.LastOrDefault().Value;

                            return "http:" + emoteUrl;
                        }

                        return new ExternalEmote
                        {
                            name = e.name,
                            isModifier = e.modifier,
                            url1x = GetEmoteUrl("1"),
                            url2x = GetEmoteUrl("2"),
                            url4x = GetEmoteUrl("4"),
                        };
                    }).ToArray();
                }
            });
        }

        public bool IsModifierEmote(string name)
        {
            return name == "cvHazmat" ||
                   name == "cvMask";
        }

        private struct EmoteSpan
        {
            public string url1x;
            public string url2x;
            public string url4x;
            public int startIndex;
            public int endIndex;
            public bool modifierEmote;
        }
        
        private struct ExternalEmote
        {
            public string name;
            public bool isModifier;
            public string url1x;
            public string url2x;
            public string url4x;
        }
        
#pragma warning disable 649
        private struct BttvEmoteResponse
        {
            public string id;
            public string channel;
            public string code;
            public string imageType;
            public bool modifierEmote;
        }
        
        private struct BttvResponse
        {
            public string urlTemplate;
            public BttvEmoteResponse[] emotes;
        }
        
        private struct FrankerFaceZResponse
        {
            public Dictionary<string, FrankerFaceZEmoteSet> sets;
        }
        
        private struct FrankerFaceZEmoteSet
        {
            public FrankerFaceZEmote[] emoticons;
        }
        
        private struct FrankerFaceZEmote
        {
            public string name;
            public bool modifier;
            public Dictionary<string, string> urls;
        }
#pragma warning restore 649
    }
}