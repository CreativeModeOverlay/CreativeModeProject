using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace CreativeMode
{
    [CustomEditor(typeof(BaseStyledElement<>), true)]
    public class StyledElementEditor : Editor
    {
        private static bool hideDefault;
        
        private const string defaultOriginName = "Default";
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            EditorGUILayout.Space();

            if (target is IStylePropertyProvider provider)
            {
                if(provider.Properties == null)
                    return;
                
                EditorGUILayout.LabelField("Resolved properties", EditorStyles.centeredGreyMiniLabel);
                //hideDefault = EditorGUILayout.Toggle("Hide default properties", hideDefault);

                var labelStyle = new GUIStyle(EditorStyles.miniLabel)
                {
                    richText = true
                };
                
                foreach (var typeProperties in provider.Properties.GroupBy(t => t.valueTarget))
                {
                    var typeName = typeProperties.Key.gameObject.name + "#" + typeProperties.Key.GetType().Name;
                    
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField($"{typeName} properties:", EditorStyles.miniLabel);
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    
                    foreach (var property in typeProperties
                        .GroupBy(p => p.propertyName)
                        .Select(p => p.First()))
                    {
                        if(!property.valueOrigin && hideDefault)
                            continue;

                        var rect = EditorGUILayout.BeginHorizontal();
                        var originName = defaultOriginName;

                        if (property.valueOrigin)
                        {
                            originName = $"<b>{property.valueOrigin.name}</b>";
                            EditorGUI.DrawRect(rect, new Color(0, 1f, 0f, 0.1f));
                        }
                        
                        var fullValue = $"{originName}, {property.value}";
                        
                        EditorGUILayout.LabelField(property.propertyName, fullValue, labelStyle);
                        EditorGUILayout.EndHorizontal();
                    }
                    
                    EditorGUILayout.EndVertical();
                }
            }
        }
    }
}