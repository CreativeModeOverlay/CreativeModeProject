using CreativeMode.Impl;

namespace CreativeMode
{
    public static class MediaPlayerModule
    {
        public static void Init()
        {
            var musicPlayerDb = DatabaseUtils.OpenDb("MusicPlayer");
            
            Instance<IMediaPlayerStorage>.Bind().Instance(new MediaPlayerStorage(musicPlayerDb));
        }
    }
}
