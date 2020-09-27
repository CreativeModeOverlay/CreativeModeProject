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

            var overridePosition = new Rect(position.x, position.y, 128, position.height);
            var valuePosition = new Rect(position.x + 24, position.y, position.width - 24, position.height);
            var oldEnabled = GUI.enabled;
            
            boolProperty.boolValue = EditorGUI.ToggleLeft(overridePosition, "", boolProperty.boolValue);
            
            if (!boolProperty.boolValue)
                GUI.enabled = false;
            
            EditorGUI.PropertyField(valuePosition, valueProperty, 
                new GUIContent(property.displayName), false);
            
            if (!boolProperty.boolValue)
                GUI.enabled = oldEnabled;
        }
    }
}