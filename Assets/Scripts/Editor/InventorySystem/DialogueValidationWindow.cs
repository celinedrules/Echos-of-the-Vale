using System.Collections.Generic;
using System.Text;
using Data.DialogueData;
using UnityEditor;
using UnityEngine;

namespace Editor.InventorySystem
{
    public class DialogueValidationWindow : EditorWindow
    {
        private Vector2 _scrollPosition;
        private string _tableName;
        private List<string> _messages;

        public static void ShowWindow(DialogueTable table, List<string> messages)
        {
            DialogueValidationWindow window = GetWindow<DialogueValidationWindow>(true, "Dialogue Table Validation");
            window.minSize = new Vector2(700f, 400f);
            window._tableName = table != null ? table.TableName : "Unknown Table";
            window._messages = messages ?? new List<string>();
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"Dialogue Table: {_tableName}", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            if (_messages == null || _messages.Count == 0)
            {
                EditorGUILayout.HelpBox("No validation issues found.", MessageType.Info);

                EditorGUILayout.Space();
                if (GUILayout.Button("Close", GUILayout.Height(30f)))
                    Close();

                return;
            }

            EditorGUILayout.HelpBox($"Found {_messages.Count} validation issue(s).", MessageType.Warning);
            EditorGUILayout.Space();

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            StringBuilder builder = new();
            for (int i = 0; i < _messages.Count; i++)
                builder.AppendLine($"• {_messages[i]}");

            EditorGUILayout.TextArea(builder.ToString(), GUILayout.ExpandHeight(true));

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();
            if (GUILayout.Button("Close", GUILayout.Height(30f)))
                Close();
        }
    }
}