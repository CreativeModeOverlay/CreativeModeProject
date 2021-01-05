using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CreativeMode;
using UniRx;
using UnityEngine;

public class ChatInteractor : IChatInteractor
{
    private string addMusicToQueueEventName = "Добавить песню в очередь";
    
    private IChatProvider ChatProvider => Instance<IChatProvider>.Get();
    private IChatStorage ChatStorage => Instance<IChatStorage>.Get();
    
    private EmoteSize emoteSize;

    private Subject<Unit> onClearChatMessagesBehaviour = new Subject<Unit>();
    
    public IObservable<ChatMessage> ChatMessages => ChatProvider
        .ChatMessages.Select(ConvertChatMessage);

    public IObservable<ChatEvent> ChatEvents => ChatProvider
        .ChatEvents.Select(ConvertChatEvents).Where(e => e != null);

    public IObservable<Unit> OnClearChatMessages => onClearChatMessagesBehaviour
        .Concat(ChatProvider.OnChatCleared);

    public ChatInteractor(EmoteSize size)
    {
        emoteSize = size;

        ChatProvider.ChatMessages.Subscribe(m =>
        {
            ChatStorage.SaveChatMessage(new ChatMessageDB
            {
                AuthorId = m.authorId,
                AuthorName = m.author,
                Message = m.message,
                Date = DateTime.Now
            });
        });
    }

    public void ClearChatMessages()
    {
        onClearChatMessagesBehaviour.OnNext(Unit.Default);
    }

    private ChatMessage ConvertChatMessage(ChatMessageRemote m)
    {
        return new ChatMessage
        {
            authorId = m.authorId,
            author = m.author,
            authorColor = m.authorColor,
            message = ParseChatMessageSpans(m.message, m.authorId, m.messageEmotes),
            isBroadcaster = m.isBroadcaster,
            isModerator = m.isModerator,

            canDropOnDesktop = true
        };
    }

    private ChatEvent ConvertChatEvents(ChatEventRemote e)
    {
        ChatEvent result;
        
        if (e.eventId == addMusicToQueueEventName)
        {
            result = new AddMediaToQueueChatEvent
            {
                mediaUrl = e.message
            };
        }
        else
        {
            result = new UnknownChatEvent
            {
                eventId = e.eventId,
                payload = e.message
            };
        }

        result.authorId = e.authorId;
        result.author = e.author;
        return result;
    }
    
    private string GetEmoteUrl(ChatMessageRemote.Emote e)
    {
        switch (emoteSize)
        {
            case EmoteSize.Size1x: return e.url1x;
            case EmoteSize.Size2x: return e.url2x;
            case EmoteSize.Size4x: return e.url4x;
        }
        
        throw new ArgumentException($"Unknown emote size {emoteSize}");
    }
    
    private SpannedText ParseChatMessageSpans(string chatMessageText, 
        string authorId, ChatMessageRemote.Emote[] emotes)
    {
        var tags = SpannedTextUtils.ParseHtmlTags(chatMessageText, (tag, value) =>
        {
            switch (tag.ToLowerInvariant())
            {
                case "b": return BoldTag.Instance;
                case "i": return ItalicTag.Instance;
                case "br": return LineBreak.Instance;
                
                case "s":
                case "size": 
                    return SpannedTextUtils.ParseSizeScaleSpan(value);
                
                case "c": 
                case "color": 
                    return SpannedTextUtils.ParseColorSpan(value);
            }

            return null;
        });

        tags.AddRange(emotes.Select(CreateEmoteTag));
        tags.AddRange(TwemojiUtils.MatchEmojis(chatMessageText).Select(CreateEmojiTag));

        SanitizeTags(authorId, tags);
        return new SpannedText(chatMessageText, tags);
    }

    private TextTag CreateEmoteTag(ChatMessageRemote.Emote e)
    {
        return new TextTag
        {
            textStartIndex = e.startIndex,
            textEndIndex = e.endIndex,
            tag = new UrlIconTag
            {
                url = GetEmoteUrl(e),
                isModifier = e.isModifier
            }
        };
    }

    private TextTag CreateEmojiTag(Match match)
    {
        return new TextTag
        {
            textStartIndex = match.Index,
            textEndIndex = match.Index + match.Length,
            tag = new UrlIconTag
            {
                url = TwemojiUtils.GetEmojiUrl(match.Value)
            },
        };
    }

    private void SanitizeTags(string authorId, List<TextTag> tags)
    {
        for (var i = tags.Count - 1; i >= 0; i--)
        {
            var tag = tags[i];

            if (!CheckTagUsagePermission(authorId, tag.tag))
            {
                tags.RemoveAt(i);
                continue;
            }

            if(tag.isClosing)
                continue;
            
            switch (tag.tag)
            {
                case SizeScaleTag s:
                    s.scale = Mathf.Clamp(s.scale, 0.5f, 2f);
                    break;
                
                case ColorTag c:
                    c.color.a = (byte) Mathf.Clamp(c.color.a, 96, 255);
                    break;
                
                default:
                    continue;
            }

            tags[i] = tag;
        }
    }

    private bool CheckTagUsagePermission(string authorId, object tag)
    {
        return true; // TODO: permission system for tags?
    }
}