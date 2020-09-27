using UnityEditor;
using UnityEngine;

namespace CreativeMode
{
    [CustomPropertyDrawer(typeof(StyleProperty<>))]
    public class StylePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty boolProperty = property.FindPropertyRelative("shouldOverride");
            SerializedProperty valueProperty = property.FindPropertyRelative("value");

            var overridePosition = new Rect(position.x, position.y, 96, position.height);
            var valuePosition = new Rect(position.x + 96, position.y, position.width - 96, position.height);

            boolProperty.boolValue = EditorGUI.ToggleLeft(overridePosition, "", boolProperty.boolValue);
            
            var oldEnabled = GUI.enabled;

            if (!boolProperty.boolValue)
                GUI.enabled = false;
            
            EditorGUI.PropertyField(valuePosition, valueProperty, 
                new GUIContent(property.displayName), false);
            
            if (!boolProperty.boolValue)
                GUI.enabled = oldEnabled;
        }
    }
}