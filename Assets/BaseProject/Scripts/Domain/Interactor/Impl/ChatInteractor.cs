using System;
using System.Linq;
using System.Text;
using CreativeMode;
using UniRx;

public class ChatInteractor : IChatInteractor
{
    private IChatClient ChatClient => Instance<IChatClient>.Get();
    private IChatStorage ChatStorage => Instance<IChatStorage>.Get();
    
    private IconAtlas iconAtlas;
    private EmoteSize emoteSize;

    private Subject<Unit> onClearChatMessagesBehaviour = new Subject<Unit>();
    
    public IObservable<ChatMessage> ChatMessages => ChatClient
        .ChatMessages.Select(ConvertChatMessage);

    public IObservable<Unit> OnClearChatMessages => onClearChatMessagesBehaviour
        .Concat(ChatClient.OnChatCleared);

    public ChatInteractor(IconAtlas atlas, EmoteSize size)
    {
        iconAtlas = atlas;
        emoteSize = size;

        ChatClient.ChatMessages.Subscribe(m =>
        {
            ChatStorage.SaveChatMessage(new ChatMessageDB
            {
                AuthorId = m.authorId,
                AuthorName = m.author,
                Message = m.rawMessage,
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
            message = new TextWithIcons
            {
                text = m.message,
                icons = m.messageEmotes.Select(e => new TextIcon
                {
                    atlas = iconAtlas.Texture,
                    rect = iconAtlas.GetIcon(GetEmoteUrl(e)),
                    position = e.position,
                    isModifier = e.isModifier
                }).ToArray()
            },
            rawMessage = m.rawMessage,
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
}