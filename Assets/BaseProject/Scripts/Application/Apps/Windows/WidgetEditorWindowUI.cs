using CreativeMode;
using UnityEngine;

public class WidgetEditorWindowUI : WindowUI
{
    [Header("References")]
    public WidgetUIRenderer widgetRenderer;

    private void Start()
    {
        widgetRenderer.Data = new SongLyricsWidget();
    }
}
