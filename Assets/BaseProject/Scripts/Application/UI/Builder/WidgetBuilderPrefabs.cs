using UnityEngine;

namespace CreativeMode
{
    [CreateAssetMenu]
    public class WidgetBuilderPrefabs : ScriptableObject
    {
        public ToggleFieldWidget toggleFieldWidgetPrefab;
        public IntFieldWidget intFieldWidgetPrefab;
        public FloatFieldWidget floatFieldWidgetPrefab;
        public TextFieldWidget textFieldWidgetPrefab;
        public VectorFieldWidget vectorFieldWidgetPrefab;
        public EnumFieldWidget enumFieldWidgetPrefab;

        public LinearLayoutWidget linearLayoutWidgetPrefab;
        public GroupWidget groupWidgetPrefab;

        public SpaceWidget spaceWidgetPrefab;
    }
}