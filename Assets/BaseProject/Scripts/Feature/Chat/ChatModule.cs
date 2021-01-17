using CreativeMode.Impl;

namespace CreativeMode
{
    public class ChatModule : ModuleBase
    {
        public string twitchClientId;
        public string twitchAccessToken;
        public string twitchOauth;
        public string twitchUsername;
        public string twitchJoinChannel;
        
        protected override void ProvideImplementations()
        {
            base.ProvideImplementations();
  
            Instance<IChatProvider>.Bind(() => new TwitchProvider(
                twitchClientId, twitchAccessToken, twitchOauth, twitchUsername, twitchJoinChannel));
        
            Instance<IChatStorage>.Bind(() => new ChatStorage(DatabaseUtils.OpenDb("Chat")));
            Instance<IChatInteractor>.Bind(() => new ChatInteractor(EmoteSize.Size2x));
        }
    }
}
