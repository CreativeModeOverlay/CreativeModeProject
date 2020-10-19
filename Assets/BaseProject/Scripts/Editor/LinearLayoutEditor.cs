using UnityEditor;
using UnityEditor.UI;

namespace CreativeMode
{
    [CustomEditor(typeof(LinearLayoutGroup))]
    public class LinearLayoutEditor : HorizontalOrVerticalLayoutGroupEditor
    {
        private SerializedProperty orientationProperty;
        
        protected override void OnEnable()
        {
            base.OnEnable();
            orientationProperty = serializedObject.FindProperty(LinearLayoutGroup.layoutOrientationProperty);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            serializedObject.Update();
            EditorGUILayout.PropertyField(orientationProperty);
            serializedObject.ApplyModifiedProperties();
        }
    }
}