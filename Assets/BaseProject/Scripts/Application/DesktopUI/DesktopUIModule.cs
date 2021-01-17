using CreativeMode.Impl;

namespace CreativeMode
{
    public class DesktopUIModule : ModuleBase
    {
        protected override void ProvideImplementations()
        {
            base.ProvideImplementations();
            Instance<IDesktopUIManager>.BindUnityObject<DesktopUIManager>();
        }
    }
}