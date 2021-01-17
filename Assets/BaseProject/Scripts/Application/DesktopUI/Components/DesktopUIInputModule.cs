using UnityEngine.EventSystems;

public class DesktopUIInputModule : StandaloneInputModule
{
    public static DesktopUIInputModule Instance { get; private set; }
    
    public bool GetPointerData(int pointerId, out PointerEventData data)
    {
        return GetPointerData(pointerId, out data, false);
    }

    protected override void Awake()
    {
        base.Awake();
        Instance = this;
    }
}
