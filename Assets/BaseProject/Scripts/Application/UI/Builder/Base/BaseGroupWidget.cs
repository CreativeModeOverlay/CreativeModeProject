using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CreativeMode
{
    public abstract class BaseGroupWidget : BaseInterfaceWidget, ILayoutWidget
    {
        public WidgetBuilderPrefabs prefabs;
        public RectTransform widgetRoot;

        [SerializeField] private float defaultMinLabelWidth = 120;
        [SerializeField] private float labelDecorWidth;

        protected virtual Orientation GroupDefaultOrientation => Orientation.Vertical;
        
        private LayoutParams SpaceDefaultLayoutParams => new LayoutParams
        {
            flexibleWidth = GroupDefaultOrientation == Orientation.Horizontal ? 1f : -1f,
            flexibleHeight = GroupDefaultOrientation == Orientation.Vertical ? 1f : -1f
        };
        
        public void Clear()
        {
            for (var i = 0; i < widgetRoot.childCount; i++)
                Destroy(widgetRoot.GetChild(i));
        }

        public float DefaultMinLabelWidth
        {
            get => defaultMinLabelWidth;
            set => defaultMinLabelWidth = value;
        }

        public IToggleFieldWidget AddToggleField(string title = "")
        {
            return AddInputField<ToggleFieldWidget, bool>(prefabs.toggleFieldWidgetPrefab, title);
        }

        public IIntFieldWidget AddIntField(string title = "")
        {
            return AddInputField<IntFieldWidget, int>(prefabs.intFieldWidgetPrefab, title);
        }

        public IFloatFieldWidget AddFloatField(string title = "")
        {
            return AddInputField<FloatFieldWidget, float>(prefabs.floatFieldWidgetPrefab, title);
        }

        public ITextFieldWidget AddTextField(string title = "")
        {
            return AddInputField<TextFieldWidget, string>(prefabs.textFieldWidgetPrefab, title);
        }

        public IVectorFieldWidget AddVector2Field(string title = "")
        {
            return AddInputField<VectorFieldWidget, Vector4>(prefabs.vectorFieldWidgetPrefab, title).SetFieldCount(2);
        }

        public IVectorFieldWidget AddVector3Field(string title = "")
        {
            return AddInputField<VectorFieldWidget, Vector4>(prefabs.vectorFieldWidgetPrefab, title).SetFieldCount(3);
        }

        public IVectorFieldWidget AddVector4Field(string title = "")
        {
            return AddInputField<VectorFieldWidget, Vector4>(prefabs.vectorFieldWidgetPrefab, title).SetFieldCount(4);
        }

        public IEnumFieldWidget AddEnumField<T>(string title = "") where T : Enum
        {
            return AddInputField<EnumFieldWidget, Enum>(prefabs.enumFieldWidgetPrefab, title).SetEnumType(typeof(T));
        }

        public ILinearLayoutWidget AddLayout(Orientation orientation)
        {
            var layout = AddField(prefabs.linearLayoutWidgetPrefab);
            layout.Orientation = orientation;
            layout.DefaultMinLabelWidth = DefaultMinLabelWidth - layout.labelDecorWidth;
            return layout;
        }

        public IGroupWidget AddGroup(string title = "")
        {
            var group = AddField(prefabs.groupWidgetPrefab);
            group.Title = title;
            group.Orientation = GroupDefaultOrientation;
            group.DefaultMinLabelWidth = DefaultMinLabelWidth - group.labelDecorWidth;
            return group;
        }

        public ISpaceWidget AddSpace()
        {
            return AddSpace(SpaceDefaultLayoutParams);
        }

        public ISpaceWidget AddSpace(LayoutParams p)
        {
            var field = AddField(prefabs.spaceWidgetPrefab);
            field.LayoutParams = p;
            return field;
        }

        private T AddInputField<T, V>(T prefab, string title)
            where T : MonoBehaviour, IFieldWidget<V>
        {
            var newInstance = AddField(prefab);
            newInstance.Title = title;
            newInstance.MinLabelWidth = defaultMinLabelWidth;
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

            return result;
        }

        protected void SetLinearLayoutOrientation(LinearLayoutGroup group, Orientation orientation)
        {
            group.Orientation = orientation;
            group.childControlWidth = true;
            group.childControlHeight = true;
            group.childForceExpandWidth = orientation == Orientation.Vertical;
            group.childForceExpandHeight = orientation == Orientation.Horizontal;
        }
    }
}