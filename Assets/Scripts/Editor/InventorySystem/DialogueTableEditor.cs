using Data.DialogueData;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Editor.InventorySystem
{
    [CustomEditor(typeof(DialogueTable))]
    public class DialogueTableEditor : OdinEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Validation", EditorStyles.boldLabel);

            if (GUILayout.Button("Validate Table", GUILayout.Height(30f)))
            {
                DialogueTable table = (DialogueTable)target;
                DialogueValidationWindow.ShowWindow(table, table.GetValidationMessages());
            }
        }
    }
}