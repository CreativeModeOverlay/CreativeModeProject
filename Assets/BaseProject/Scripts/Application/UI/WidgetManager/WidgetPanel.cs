using System.Collections.Generic;
using CreativeMode;
using UnityEngine;

public class WidgetPanel : MonoBehaviour
{
    public string id;
    public RectTransform widgetRoot;
    public WidgetContainer containerPrefab;

    private List<WidgetContainer> containers = new List<WidgetContainer>();

    private WidgetContainer CreateContainer()
    {
        var container = Instantiate(containerPrefab, widgetRoot);
        containers.Add(container);
        return container;
    }
}
