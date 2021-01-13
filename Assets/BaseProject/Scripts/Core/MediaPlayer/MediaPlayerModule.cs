using CreativeMode.Impl;

namespace CreativeMode
{
    public static class MediaPlayerModule
    {
        private static string youtubeDlPath = "youtube-dl";
        
        public static void Init()
        {
            var musicPlayerDb = DatabaseUtils.OpenDb("MusicPlayer");
            var youtubeDl = new YoutubeDL(youtubeDlPath);
            
            Instance<IMediaVisualizationProvider>.BindUnityObject<MediaVisualizationProvider>();
            
            Instance<IMediaPlaylistProvider>.BindUnityObject<MediaPlaylistProvider>();
            Instance<IMediaPlayer>.BindUnityObject<MediaPlayer>();
            
            Instance<IMediaProvider>.Bind(() => new MediaProvider(youtubeDl));
            Instance<IMediaPlayerStorage>.Bind(() => new MediaPlayerStorage(musicPlayerDb));
        }
    }
}
