using CreativeMode.Impl;

namespace CreativeMode
{
    public static class ChatModule
    {
        public static void Init()
        {
            var chatDb = DatabaseUtils.OpenDb("Chat");
            
            Instance<IChatProvider>.Bind(() => new TwitchProvider(
                "", "", "", "", ""));
        
            Instance<IChatStorage>.Bind(() => new ChatStorage(chatDb));
            Instance<IChatInteractor>.Bind(() => new ChatInteractor(EmoteSize.Size2x));
        }
    }
}
