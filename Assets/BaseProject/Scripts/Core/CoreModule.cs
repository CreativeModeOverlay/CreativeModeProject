using UniRx;

namespace CreativeMode
{
    public class CoreModule : ModuleBase
    {
        protected override void Initialize()
        {
            base.Initialize();
            
            MainThreadDispatcher.Initialize();
            UnityUtils.FixFreezingWindow();
        }
    }
}