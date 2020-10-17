namespace CreativeMode
{
    public class NoParamsWidgetEditorUI : BaseWidgetEditorUI<AppWidget>
    {
        private AppWidget originalData;

        protected override void SetData(AppWidget widget)
        {
            originalData = widget;
        }

        protected override AppWidget GetData()
        {
            return originalData;
        }
    }
}