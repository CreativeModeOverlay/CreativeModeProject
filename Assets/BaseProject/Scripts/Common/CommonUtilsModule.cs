namespace CreativeMode
{
    public static class CommonUtilsModule
    {
        public static void Init()
        {
            Instance<ImageLoader>.Bind(() => new ImageLoader { MaxThreadCount = 4 });
        }
    }
}