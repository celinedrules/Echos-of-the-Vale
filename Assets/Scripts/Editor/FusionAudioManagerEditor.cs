// Done
using Audio;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(FusionAudioManager))]
    public class FusionAudioManagerEditor : UnityEditor.Editor
    {
        private static readonly string[] TrackLabels =
        {
            "Background",
            "Player FX",
            "Enemy FX",
            "Environment FX",
            "Dialogue",
            "UI"
        };

        private const int TrackCount = 6;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            FusionAudioManager audioManager = (FusionAudioManager)target;

            SerializedProperty persistentProperty = serializedObject.FindProperty("isPersistent");
            EditorGUILayout.PropertyField(persistentProperty);
            EditorGUILayout.Space();

            if (audioManager.Tracks is not { Length: TrackCount })
                audioManager.Tracks = new AudioTrack[TrackCount];

            SerializedProperty tracksProperty = serializedObject.FindProperty("tracks");
            EditorGUILayout.LabelField("Audio Tracks");

            EditorGUI.indentLevel++;

            for (int i = 0; i < tracksProperty.arraySize; i++)
            {
                SerializedProperty trackProperty = tracksProperty.GetArrayElementAtIndex(i);
                EditorGUILayout.BeginVertical("box");

                string customLabel = i < TrackLabels.Length ? TrackLabels[i] : $"Track {i}";
                EditorGUILayout.PropertyField(trackProperty.FindPropertyRelative("audioComponent"),
                    new GUIContent(customLabel));

                // Volume slider
                SerializedProperty volumeProperty = trackProperty.FindPropertyRelative("volume");
                AudioTrack track = audioManager.Tracks[i];

                // Sync volume from AudioSource if it differs (someone changed it directly)
                if (track?.Source != null && !Mathf.Approximately(volumeProperty.floatValue, track.Source.volume))
                {
                    volumeProperty.floatValue = track.Source.volume;
                }

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.Slider(volumeProperty, 0f, 1f, new GUIContent("Volume"));
                if (EditorGUI.EndChangeCheck())
                {
                    // Apply volume change immediately to AudioSource
                    if (track?.Source != null)
                        track.Source.volume = volumeProperty.floatValue;
                }

                EditorGUILayout.Space();

                SerializedProperty audioArray = trackProperty.FindPropertyRelative("audio");

                Audio.Audio audioComponent = audioManager.Tracks[i]?.audioComponent;
                string[] availableIds = GetAvailableAudioIds(audioComponent);

                EditorGUI.indentLevel++;

                for (int j = 0; j < audioArray.arraySize; j++)
                {
                    SerializedProperty audioObject = audioArray.GetArrayElementAtIndex(j);
                    EditorGUILayout.BeginHorizontal();

                    SerializedProperty audioIdProperty = audioObject.FindPropertyRelative("audioId");

                    if (availableIds.Length > 0)
                    {
                        int currentIndex = System.Array.IndexOf(availableIds, audioIdProperty.stringValue);
                        if (currentIndex < 0) currentIndex = 0;

                        int newIndex = EditorGUILayout.Popup(currentIndex, availableIds, GUILayout.MaxWidth(200));
                        audioIdProperty.stringValue = availableIds[newIndex];
                    }
                    else
                    {
                        EditorGUILayout.LabelField("No Audio IDs available", GUILayout.MaxWidth(200));
                    }

                    EditorGUILayout.PropertyField(audioObject.FindPropertyRelative("clip"), GUIContent.none);

                    // Loop toggle
                    SerializedProperty loopProperty = audioObject.FindPropertyRelative("loop");
                    loopProperty.boolValue = GUILayout.Toggle(loopProperty.boolValue, "Loop", EditorStyles.miniButton, GUILayout.Width(52));

                    if (GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(18)))
                        audioArray.DeleteArrayElementAtIndex(j);

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.Space();

                if (GUILayout.Button("Add Audio Object"))
                    audioArray.InsertArrayElementAtIndex(audioArray.arraySize);

                EditorGUI.indentLevel--;
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }

            EditorGUI.indentLevel--;

            serializedObject.ApplyModifiedProperties();
        }

        private string[] GetAvailableAudioIds(Audio.Audio audioComponent)
        {
            if (audioComponent == null || audioComponent.AudioData == null)
                return System.Array.Empty<string>();

            return audioComponent.AudioData.AudioIds ?? System.Array.Empty<string>();
        }
    }
}