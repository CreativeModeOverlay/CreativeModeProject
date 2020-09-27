using UnityEditor;
using UnityEngine;

namespace CreativeMode
{
    [CustomEditor(typeof(BaseStyledElement<>), true)]
    public class StyledElementEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space();
            if (target is IResolveLogProvider provider)
            {
                EditorGUILayout.LabelField(provider.ResolverLog, new GUIStyle(EditorStyles.helpBox)
                {
                    richText = true
                });
            }
        }
    }
}