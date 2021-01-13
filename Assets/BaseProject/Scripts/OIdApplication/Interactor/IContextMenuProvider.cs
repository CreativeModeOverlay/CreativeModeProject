namespace CreativeMode
{
    public interface IContextMenuProvider
    {
        Menu CreateChatAuthorMenu(string authorId);
        Menu CreateChatMessageMenu(ChatMessage message);
    }
}