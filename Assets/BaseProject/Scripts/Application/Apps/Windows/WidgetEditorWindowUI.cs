using System.Collections.Generic;
using CreativeMode;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class WidgetEditorWindowUI : WindowUI
{
    [Header("References")]
    public WidgetUIRenderer widgetRenderer;

    private IWidgetManager Manager => Instance<IWidgetManager>.Get();
    private IWidgetUIFactory Factory => Instance<IWidgetUIFactory>.Get();
    private IDesktopUIManager UIManager => Instance<IDesktopUIManager>.Get();
    private IWidgetRegistry WidgetRegistry => Instance<IWidgetRegistry>.Get();

    public RectTransform widgetListRoot;
    public RectTransform widgetEditorRoot;
    public GameObject rightPanelRoot;
    public GameObject widgetPreviewRoot;
    
    public GenericButton createNewWidgetButton;
    public GenericButton deleteButton;
    public GenericButton resetButton;
    public GenericButton applyButton;
    public InputField nameInputField;

    public GenericButton widgetListButtonPrefab;

    private WidgetData currentWidgetData;
    private IWidgetEditorUI currentWidgetEditorUi;

    private Dictionary<int, GenericButton> widgetButtonsById 
        = new Dictionary<int, GenericButton>();

    private void Start()
    {
        Manager.Widgets.SubscribeChanges(OnNewWidgetAdded, OnWidgetRemoved);
        Manager.WidgetUpdated.Subscribe(OnWidgetUpdated);

        createNewWidgetButton.OnClick = () =>
        {
            var contextMenuBuilder = new Menu.Builder();
            
            foreach (var widget in WidgetRegistry.GetWidgets())
            {
                contextMenuBuilder.Button(
                    title: widget.name,
                    icon: widget.icon,
                    onClick: () => Manager.CreateWidget(widget.dataType));
            }
            
            var position = Input.mousePosition;
            UIManager.ShowContextMenu(position, contextMenuBuilder.Build());
        };
        
        OnCloseWidgetEditor();
    }

    private void OnShowWidgetEditor(WidgetData data)
    {
        OnCloseWidgetEditor();
        
        rightPanelRoot.SetActive(true);
        widgetPreviewRoot.SetActive(true);

        currentWidgetData = data;
        currentWidgetEditorUi = Factory.CreateWidgetEditorUI(data.type);
        
        TransformUtils.FillRectParent(currentWidgetEditorUi.Root, widgetEditorRoot);
        
        void ApplyWidgetData()
        {
            widgetRenderer.Data = data.data;
            nameInputField.text = data.name;
            currentWidgetEditorUi.Data = data.data;
        }
        
        deleteButton.OnClick = () => { Manager.RemoveWidget(data.id); };
        resetButton.OnClick = ApplyWidgetData;
        applyButton.OnClick = () =>
        {
            Manager.UpdateWidget(new WidgetData
            {
                id = data.id,
                data = currentWidgetEditorUi.Data,
                type = data.type,
                name = nameInputField.text
            });
        };
        
        ApplyWidgetData();
    }

    private void OnCloseWidgetEditor()
    {
        rightPanelRoot.SetActive(false);
        widgetPreviewRoot.SetActive(false);

        Destroy(currentWidgetEditorUi?.Root);

        currentWidgetData = null;
        currentWidgetEditorUi = null;
    }

    private void OnNewWidgetAdded(WidgetData data)
    {
        var instance = Instantiate(widgetListButtonPrefab, widgetListRoot);
        var info = WidgetRegistry.GetWidgetInfo(data.type);

        instance.gameObject.SetActive(true);
        instance.Text = data.name;
        instance.Icon = info.icon;
        instance.OnClick = () => OnShowWidgetEditor(Manager.GetWidget(data.id));

        widgetButtonsById[data.id] = instance;
    }

    private void OnWidgetUpdated(WidgetData data)
    {
        if(widgetButtonsById.TryGetValue(data.id, out var button))
            button.Text = data.name;

        if (currentWidgetData?.id == data.id)
        {
            widgetRenderer.Data = data.data;
        }
    }

    private void OnWidgetRemoved(WidgetData data)
    {
        if (widgetButtonsById.TryGetValue(data.id, out var widget))
            Destroy(widget.gameObject);

        widgetButtonsById.Remove(data.id);

        if (currentWidgetData?.id == data.id)
            OnCloseWidgetEditor();
    }
}
