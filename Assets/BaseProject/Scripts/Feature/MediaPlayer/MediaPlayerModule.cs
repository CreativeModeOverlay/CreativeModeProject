using CreativeMode.Impl;

namespace CreativeMode
{
    public class MediaPlayerModule : ModuleBase
    {
        public string youtubeDlPath = "youtube-dl";

        protected override void ProvideImplementations()
        {
            base.ProvideImplementations();

            Instance<IMediaVisualizationProvider>.BindUnityObject<MediaVisualizationProvider>();
            Instance<IMediaPlaylistProvider>.BindUnityObject<MediaPlaylistProvider>();
            Instance<IMediaPlayer>.BindUnityObject<MediaPlayer>();
            
            Instance<IMediaProvider>.Bind(() =>
            {
                var youtube = new YoutubeDL(youtubeDlPath);
                return new MediaProvider(youtube);
            });
            Instance<IMediaPlayerStorage>.Bind(() =>
            {
                var db = DatabaseUtils.OpenDb("MediaPlayer");
                return new MediaPlayerStorage(db);
            });
        }
    }
}
