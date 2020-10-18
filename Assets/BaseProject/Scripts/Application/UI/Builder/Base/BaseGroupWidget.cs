using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CreativeMode
{
    public abstract class BaseGroupWidget : BaseInterfaceWidget, ILayoutWidget
    {
        public ToggleFieldWidget toggleFieldWidgetPrefab;
        public IntFieldWidget intFieldWidgetPrefab;
        public FloatFieldWidget floatFieldWidgetPrefab;
        public TextFieldWidget textFieldWidgetPrefab;
        public VectorFieldWidget vectorFieldWidgetPrefab;
        public EnumFieldWidget enumFieldWidgetPrefab;
        public GroupWidget groupWidgetPrefab;

        public LayoutGroupWidget horizontalWidgetPrefab;
        public LayoutGroupWidget verticalWidgetPrefab;

        public RectTransform widgetRoot;

        public void Clear()
        {
            for (var i = 0; i < widgetRoot.childCount; i++)
                Destroy(widgetRoot.GetChild(i));
        }

        public IToggleFieldWidget AddToggleField(string title = "")
        {
            return AddInputField<ToggleFieldWidget, bool>(toggleFieldWidgetPrefab, title);
        }

        public IIntFieldWidget AddIntField(string title = "")
        {
            return AddInputField<IntFieldWidget, int>(intFieldWidgetPrefab, title);
        }

        public IFloatFieldWidget AddFloatField(string title = "")
        {
            return AddInputField<FloatFieldWidget, float>(floatFieldWidgetPrefab, title);
        }

        public ITextFieldWidget AddTextField(string title = "")
        {
            return AddInputField<TextFieldWidget, string>(textFieldWidgetPrefab, title);
        }

        public IVectorFieldWidget AddVector2Field(string title = "")
        {
            return AddInputField<VectorFieldWidget, Vector4>(vectorFieldWidgetPrefab, title).SetFieldCount(2);
        }

        public IVectorFieldWidget AddVector3Field(string title = "")
        {
            return AddInputField<VectorFieldWidget, Vector4>(vectorFieldWidgetPrefab, title).SetFieldCount(3);
        }

        public IVectorFieldWidget AddVector4Field(string title = "")
        {
            return AddInputField<VectorFieldWidget, Vector4>(vectorFieldWidgetPrefab, title).SetFieldCount(4);
        }

        public IEnumFieldWidget AddEnumField<T>(string title = "") where T : Enum
        {
            return AddInputField<EnumFieldWidget, Enum>(enumFieldWidgetPrefab, title).SetEnumType(typeof(T));
        }

        public ILayoutWidget AddHorizontalGroup()
        {
            return AddField(horizontalWidgetPrefab);
        }

        public ILayoutWidget AddVerticalGroup()
        {
            return AddField(verticalWidgetPrefab);
        }

        public IGroupWidget AddGroup(string title = "")
        {
            var group = AddField(groupWidgetPrefab);
            group.Title = title;
            return group;
        }

        private T AddInputField<T, V>(T prefab, string title)
            where T : MonoBehaviour, IFieldWidget<V>
        {
            var newInstance = AddField(prefab);
            newInstance.Title = title;
            return newInstance;
        }

        public T AddField<T>(T prefab) where T : MonoBehaviour
        {
            return AddField<T>((MonoBehaviour) prefab);
        }

        public T AddField<T>(Object prefab) where T : MonoBehaviour
        {
            var newInstance = Instantiate(prefab, widgetRoot, false);
            T result = default;

            if (newInstance is T tInstance)
                result = tInstance;

            if (result == null && newInstance is GameObject gameObjectInstance)
                result = gameObjectInstance.GetComponentInChildren<T>();

            if (result == null && newInstance is Component componentInstance)
                result = componentInstance.gameObject.GetComponentInChildren<T>();

            Debug.Log($"Unknown prefab type {prefab.GetType().Name}");
            return result;
        }
    }
}