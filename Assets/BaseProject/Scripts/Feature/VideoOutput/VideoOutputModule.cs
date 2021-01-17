namespace CreativeMode
{
    public class OverlaySceneModule : ModuleBase
    {
        protected override void Initialize()
        {
            base.Initialize();
            
            Instance<IVideoSceneManager>.BindUnityObject<VideoSceneManager>();
        }
    }
}