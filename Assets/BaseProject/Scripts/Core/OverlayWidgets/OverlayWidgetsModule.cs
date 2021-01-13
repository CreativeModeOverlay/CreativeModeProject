

using CreativeMode.Impl;

namespace CreativeMode
{
    public static class OverlayWidgetsModule
    {
        public static void Init()
        {
            Instance<IOverlayWidgetManager>.BindUnityObject<OverlayWidgetManager>();
            Instance<IOverlayWidgetUIFactory>.Bind(() => new OverlayWidgetUIFactory());
        }
    }
}