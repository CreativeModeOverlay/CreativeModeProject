using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CreativeMode;
using UniRx;
using UnityEngine;

public class ChatInteractor : IChatInteractor
{
    private IChatClient ChatClient => Instance<IChatClient>.Get();
    private IChatStorage ChatStorage => Instance<IChatStorage>.Get();

    private EmoteSize emoteSize;

    private Subject<Unit> onClearChatMessagesBehaviour = new Subject<Unit>();
    
    public IObservable<ChatMessage> ChatMessages => ChatClient
        .ChatMessages.Select(ConvertChatMessage);

    public IObservable<Unit> OnClearChatMessages => onClearChatMessagesBehaviour
        .Concat(ChatClient.OnChatCleared);

    public ChatInteractor(EmoteSize size)
    {
        emoteSize = size;

        ChatClient.ChatMessages.Subscribe(m =>
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
            hasMention = m.hasMention,
            isBroadcaster = m.isBroadcaster,
            isModerator = m.isModerator,

            canDropOnDesktop = true
        };
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