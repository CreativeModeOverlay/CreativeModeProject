using CreativeMode;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(VlcMediaPlayerBehaviour))]
public class VlcMediaPlayerBehaviourEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var source = (VlcMediaPlayerBehaviour) target;
        var message = $"- Sample rate: {source.SampleRate}\n" +
                      $"- Channel count: {source.ChannelCount}\n" +
                      $"- Buffer size: {source.BufferedSampleCount}\n" +
                      $"- Buffer duration: {source.BufferedLengthMs} ms\n" +
                      $"- VLC    Position: {source.Position}\n" +
                      $"- Source Position: {source.AudioSourcePosition}\n" +
                      $"- Position desync: {source.Position - source.AudioSourcePosition}\n" +
                      $"- Resolution: {source.VideoWidth} x {source.VideoHeight}";

        EditorGUILayout.Space();
        EditorGUILayout.BeginVertical();
        EditorGUILayout.HelpBox(message, MessageType.Info);
        EditorGUILayout.EndVertical();

        if (source.VideoTexture != null)
        {
            var height = ((float) source.VideoTexture.height / source.VideoTexture.width) * EditorGUIUtility.currentViewWidth;
            var previewRect = EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

            EditorGUILayout.LabelField("", GUILayout.Height(height));
    
            EditorGUI.DrawPreviewTexture(new Rect(previewRect.x, 
                previewRect.y + previewRect.height, 
                previewRect.width, -previewRect.height), source.VideoTexture);
            EditorGUILayout.EndHorizontal();
        }
    }

    public override bool RequiresConstantRepaint() => true;
}
