using CreativeMode.Impl;

namespace CreativeMode
{
    public class AssetLoaderModule : ModuleBase
    {
        public int maxThreadCount = 4;
        
        protected override void ProvideImplementations()
        {
            base.ProvideImplementations();
            
            Instance<IImageLoader>.Bind(() => new ImageLoader
            {
                MaxThreadCount = maxThreadCount
            });
        }
    }
}