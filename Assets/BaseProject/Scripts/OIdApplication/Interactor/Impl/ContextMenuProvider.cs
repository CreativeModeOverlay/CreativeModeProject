using UnityEngine;

namespace CreativeMode.Impl
{
    public class ContextMenuProvider : MonoBehaviour, IContextMenuProvider
    {
        public Menu CreateChatAuthorMenu(string authorId)
        {
            return new Menu.Builder()
                .Build();
        }

        public Menu CreateChatMessageMenu(ChatMessage message)
        {
            return new Menu.Builder()
                .SubMenu("Author", CreateChatAuthorMenu(message.authorId))
                .SubMenu("Administration", new Menu.Builder()
                    .Button("Delete message", () => { })
                    .Button("Ban author", () => { }))
                .Build();
        }
    }
}