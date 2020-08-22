using UnityEditor;
using TextEditor = UnityEditor.UI.TextEditor;

[CustomEditor(typeof(CreativeText))]
public class CreativeTextEditor : TextEditor
{
    private SerializedProperty iconScaleProperty;
    private SerializedProperty modifierIconScaleProperty;

    protected override void OnEnable()
    {
        base.OnEnable();
        
        iconScaleProperty = serializedObject.FindProperty(nameof(CreativeText.iconScale));
        modifierIconScaleProperty = serializedObject.FindProperty(nameof(CreativeText.modifierIconScale));
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        EditorGUILayout.PropertyField(iconScaleProperty);
        EditorGUILayout.PropertyField(modifierIconScaleProperty);
        serializedObject.ApplyModifiedProperties();
    }
}
