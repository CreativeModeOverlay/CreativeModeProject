namespace CreativeMode
{
    public class OverlaySceneModule
    {
        public static void Init()
        {
            Instance<IOverlaySceneManager>.BindUnityObject<OverlaySceneManager>();
        }
    }
}