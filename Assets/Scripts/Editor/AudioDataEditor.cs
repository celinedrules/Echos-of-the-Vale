// Done
using Data.Audio;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(AudioData), true)]
    public class AudioDataEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space(10);

            AudioData audioData = (AudioData)target;

            if (GUILayout.Button($"Regenerate {audioData.GetType().Name} Enum", GUILayout.Height(25)))
            {
                AudioIdEnumGenerator.GenerateEnumForAsset(audioData);
            }
        }
    }
}