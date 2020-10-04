namespace CreativeMode
{
    public class NoParamsWidgetEditorUI : BaseWidgetEditorUI<Widget>
    {
        private Widget originalData;

        protected override void SetData(Widget widget)
        {
            originalData = widget;
        }

        protected override Widget GetData()
        {
            return originalData;
        }
    }
}