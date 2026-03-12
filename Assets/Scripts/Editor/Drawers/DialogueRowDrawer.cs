using Data.DialogueData;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Editor.Drawers
{
    public class DialogueRowDrawer : OdinValueDrawer<DialogueRow>
    {
        protected override void DrawPropertyLayout(GUIContent label)
        {
            DialogueRow row = ValueEntry.SmartValue;
            if (row == null)
            {
                CallNextDrawer(label);
                return;
            }

            InspectorProperty property = Property;
            property.State.Expanded = DrawHeader(row, property.State.Expanded);

            if (!property.State.Expanded)
                return;

            SirenixEditorGUI.BeginBox();
            
            foreach (InspectorProperty inspectorProperty in property.Children)
                inspectorProperty.Draw();

            SirenixEditorGUI.EndBox();
        }

        private bool DrawHeader(DialogueRow row, bool expanded)
        {
            Rect rect = EditorGUILayout.GetControlRect(false, 22f);
            rect = EditorGUI.IndentedRect(rect);

            Rect foldoutRect = new Rect(rect.x, rect.y, 18f, rect.height);
            Rect kindRect = new Rect(rect.xMax - 116f, rect.y, 110f, rect.height);
            Rect titleRect = new Rect(rect.x + 18f, rect.y, rect.width - 18f - 92f, rect.height);

            expanded = EditorGUI.Foldout(foldoutRect, expanded, GUIContent.none, true);
            EditorGUI.LabelField(titleRect, row.HeaderTitle, EditorStyles.label);

            GUIStyle kindStyle = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleRight,
                fontStyle = FontStyle.Italic,
                fontSize = 10
            };

            Color previousColor = GUI.color;
            GUI.color = new Color(0.8f, 0.8f, 0.8f);
            EditorGUI.LabelField(kindRect, $"({row.RowKind})", kindStyle);
            GUI.color = previousColor;

            return expanded;
        }
    }
}