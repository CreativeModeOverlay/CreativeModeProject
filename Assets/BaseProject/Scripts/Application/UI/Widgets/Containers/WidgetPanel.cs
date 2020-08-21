using System.Collections.Generic;
using CreativeMode;
using UnityEngine;

public class WidgetPanel : MonoBehaviour
{
    public RectTransform widgetRoot;
    public WidgetContainer containerPrefab;

    private List<WidgetContainer> containers = new List<WidgetContainer>();

    public IWidgetContainer CreateContainer()
    {
        var container = Instantiate(containerPrefab, widgetRoot);
        containers.Add(container);
        return container;
    }

    public interface IWidgetContainer
    { 
        bool IsEmpty { get; }
        int Size { get; set; }

        void PutWidget(GameObject widget);
        GameObject PopWidget();
    }
}
