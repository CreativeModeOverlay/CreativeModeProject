namespace CreativeMode
{
    public class VideoOutputModule : ModuleBase
    {
        protected override void Initialize()
        {
            base.Initialize();
            
            Instance<IVideoSceneManager>.BindUnityObject<VideoSceneManager>();
        }
    }
}