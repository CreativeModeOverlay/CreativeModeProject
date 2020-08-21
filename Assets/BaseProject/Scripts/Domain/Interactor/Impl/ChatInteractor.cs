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

    private Subject<Unit> onClearChatMessagesBehaviour = new Subject<Unit>();
    
    public IObservable<ChatMessage> ChatMessages => ChatClient
        .ChatMessages.Select(ConvertChatMessage);

    public IObservable<Unit> OnClearChatMessages => onClearChatMessagesBehaviour
        .Concat(ChatClient.OnChatCleared);

    public ChatInteractor(IconAtlas atlas)
    {
        iconAtlas = atlas;

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
                    rect = iconAtlas.GetIcon(e.iconUrl),
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
}