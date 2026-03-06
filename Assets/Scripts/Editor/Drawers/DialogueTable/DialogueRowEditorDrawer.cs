// Done
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using static Editor.Drawers.DialogueTable.DialogueTableConstants;

namespace Editor.Drawers.DialogueTable
{
    /// <summary>
    /// Draws the row editor area: toolbar + property inspector for the selected row.
    /// </summary>
    internal class DialogueRowEditorDrawer
    {
        private Vector2 _scrollPos;

        public void Draw(Rect rect, SerializedProperty rowsProperty, int selectedRowIndex,
            DialogueRowOperations operations)
        {
            // ── Toolbar (pure Rect-based) ──
            Rect toolbarRect = new(rect.x, rect.y, rect.width, ToolbarHeight);
            EditorGUI.DrawRect(toolbarRect, HeaderBg);
            DrawToolbar(toolbarRect, rowsProperty, selectedRowIndex, operations);

            // ── Row properties (scrollable) ──
            Rect propsRect = new(rect.x, rect.y + ToolbarHeight, rect.width, rect.height - ToolbarHeight);

            if (selectedRowIndex >= 0 && selectedRowIndex < rowsProperty.arraySize)
            {
                GUILayout.BeginArea(propsRect);
                {
                    _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
                    {
                        GUILayout.Space(4);
                        SerializedProperty row = rowsProperty.GetArrayElementAtIndex(selectedRowIndex);

                        SirenixEditorGUI.BeginBox("Row Value");
                        DrawProp(row, "rowId", "Row ID");
                        DrawProp(row, "speaker", "Speaker");
                        DrawProp(row, "textLines", "Text Lines");
                        DrawProp(row, "portraitOverride", "Portrait Override");
                        DrawProp(row, "actionType", "Action Type");
                        DrawLeadsToDropdown(row, rowsProperty);
                        SirenixEditorGUI.EndBox();

                        SirenixEditorGUI.BeginBox("Choice Settings");
                        DrawProp(row, "playerChoiceAnswer", "Choice Answer");
                        DrawChoiceRowIds(row, rowsProperty);
                        SirenixEditorGUI.EndBox();

                        SirenixEditorGUI.BeginBox("Audio");
                        DrawProp(row, "audioClip", "Audio Clip");
                        DrawProp(row, "audioStartTime", "Audio Start Time");
                        SirenixEditorGUI.EndBox();

                        SirenixEditorGUI.BeginBox("Flags");
                        DrawProp(row, "dialogSkip", "Dialog Skip");
                        SirenixEditorGUI.EndBox();
                    }
                    EditorGUILayout.EndScrollView();
                }
                GUILayout.EndArea();
            }
        }

        /// <summary>
        /// Builds dropdown display names and corresponding row IDs from all rows in the table.
        /// </summary>
        private static void BuildRowDropdownOptions(SerializedProperty rowsProperty,
            out string[] displayNames, out int[] rowIds)
        {
            int count = rowsProperty.arraySize;
            displayNames = new string[count];
            rowIds = new int[count];

            for (int i = 0; i < count; i++)
            {
                SerializedProperty r = rowsProperty.GetArrayElementAtIndex(i);
                int id = r.FindPropertyRelative("rowId").intValue;
                SerializedProperty textLines = r.FindPropertyRelative("textLines");
                string text = textLines.arraySize > 0
                    ? textLines.GetArrayElementAtIndex(0).stringValue
                    : "";

                // Truncate long lines for readability
                if (text.Length > 60)
                    text = text.Substring(0, 57) + "...";

                displayNames[i] = $"Id:{id} - {text}";
                rowIds[i] = id;
            }
        }

        /// <summary>
        /// Draws the Choice Row IDs array with a dropdown per element to select any row in the table.
        /// </summary>
        private static void DrawChoiceRowIds(SerializedProperty row, SerializedProperty rowsProperty)
        {
            SerializedProperty choiceRowIds = row.FindPropertyRelative("choiceRowIds");

            // Build dropdown options from all rows
            BuildRowDropdownOptions(rowsProperty, out string[] displayNames, out int[] allRowIds);

            // Draw the array header (foldout)
            choiceRowIds.isExpanded = EditorGUILayout.Foldout(choiceRowIds.isExpanded,
                new GUIContent($"Choice Row IDs ({choiceRowIds.arraySize})"), true);

            if (!choiceRowIds.isExpanded)
                return;

            EditorGUI.indentLevel++;

            for (int i = 0; i < choiceRowIds.arraySize; i++)
            {
                SerializedProperty element = choiceRowIds.GetArrayElementAtIndex(i);
                int currentRowId = element.intValue;

                // Find which dropdown index matches the current value
                int selectedDropdownIndex = -1;
                for (int j = 0; j < allRowIds.Length; j++)
                {
                    if (allRowIds[j] == currentRowId)
                    {
                        selectedDropdownIndex = j;
                        break;
                    }
                }

                EditorGUILayout.BeginHorizontal();

                // Dropdown to select a row
                int newDropdownIndex = EditorGUILayout.Popup(
                    $"Choice {i + 1}",
                    selectedDropdownIndex,
                    displayNames);

                if (newDropdownIndex >= 0 && newDropdownIndex != selectedDropdownIndex)
                {
                    element.intValue = allRowIds[newDropdownIndex];
                    choiceRowIds.serializedObject.ApplyModifiedProperties();
                }

                // ▲ / ▼ reorder buttons
                using (new EditorGUI.DisabledScope(i <= 0))
                {
                    if (GUILayout.Button("↑", EditorStyles.miniButtonLeft, GUILayout.Width(20)))
                    {
                        choiceRowIds.MoveArrayElement(i, i - 1);
                        choiceRowIds.serializedObject.ApplyModifiedProperties();
                        EditorGUILayout.EndHorizontal();
                        break;
                    }
                }
                using (new EditorGUI.DisabledScope(i >= choiceRowIds.arraySize - 1))
                {
                    if (GUILayout.Button("↓", EditorStyles.miniButtonRight, GUILayout.Width(20)))
                    {
                        choiceRowIds.MoveArrayElement(i, i + 1);
                        choiceRowIds.serializedObject.ApplyModifiedProperties();
                        EditorGUILayout.EndHorizontal();
                        break;
                    }
                }

                // + / - buttons
                if (GUILayout.Button("+", EditorStyles.miniButton, GUILayout.Width(20)))
                {
                    choiceRowIds.InsertArrayElementAtIndex(i + 1);
                    choiceRowIds.serializedObject.ApplyModifiedProperties();
                    EditorGUILayout.EndHorizontal();
                    break;
                }
                if (GUILayout.Button("-", EditorStyles.miniButton, GUILayout.Width(20)))
                {
                    choiceRowIds.DeleteArrayElementAtIndex(i);
                    choiceRowIds.serializedObject.ApplyModifiedProperties();
                    EditorGUILayout.EndHorizontal();
                    break;
                }

                EditorGUILayout.EndHorizontal();
            }

            // "Add" button when the list is empty
            if (choiceRowIds.arraySize == 0)
            {
                if (GUILayout.Button("Add Choice", EditorStyles.miniButton))
                {
                    choiceRowIds.InsertArrayElementAtIndex(0);
                    if (allRowIds.Length > 0)
                        choiceRowIds.GetArrayElementAtIndex(0).intValue = allRowIds[0];
                    choiceRowIds.serializedObject.ApplyModifiedProperties();
                }
            }

            EditorGUI.indentLevel--;
        }
        
        /// <summary>
        /// Draws the "Leads To" field as a dropdown to pick any row in the table, or "None" to clear it.
        /// </summary>
        private static void DrawLeadsToDropdown(SerializedProperty row, SerializedProperty rowsProperty)
        {
            SerializedProperty leadsTo = row.FindPropertyRelative("leadsTo");

            BuildRowDropdownOptions(rowsProperty, out string[] rowDisplayNames, out int[] rowIds);

            // Build options: first entry is "None (-1)", then all rows
            string[] displayNames = new string[rowDisplayNames.Length + 1];
            displayNames[0] = "None";
            for (int i = 0; i < rowDisplayNames.Length; i++)
                displayNames[i + 1] = rowDisplayNames[i];

            // Find which index is currently selected
            int selectedIndex = 0; // default to "None"
            for (int i = 0; i < rowIds.Length; i++)
            {
                if (rowIds[i] == leadsTo.intValue)
                {
                    selectedIndex = i + 1; // offset by 1 because of "None"
                    break;
                }
            }

            int newIndex = EditorGUILayout.Popup("Leads To", selectedIndex, displayNames);

            if (newIndex != selectedIndex)
            {
                leadsTo.intValue = newIndex == 0 ? -1 : rowIds[newIndex - 1];
                leadsTo.serializedObject.ApplyModifiedProperties();
            }
        }

        private static void DrawToolbar(Rect toolbarRect, SerializedProperty rowsProperty,
            int selectedRowIndex, DialogueRowOperations operations)
        {
            float x = toolbarRect.x + 4;
            float y = toolbarRect.y;
            float h = toolbarRect.height;
            float btnW = 24f;

            // + button (always enabled)
            if (GUI.Button(new Rect(x, y, btnW, h), "+", EditorStyles.toolbarButton))
                operations.AddRow();
            x += btnW + 2;

            // ✕ button
            bool hasSelection = selectedRowIndex >= 0 && selectedRowIndex < rowsProperty.arraySize;
            using (new EditorGUI.DisabledScope(!hasSelection))
            {
                if (GUI.Button(new Rect(x, y, btnW, h), "✕", EditorStyles.toolbarButton))
                    operations.RemoveSelectedRow();
            }
            x += btnW + 10;

            // ▲ button
            using (new EditorGUI.DisabledScope(selectedRowIndex <= 0))
            {
                if (GUI.Button(new Rect(x, y, btnW, h), "↑", EditorStyles.toolbarButton))
                    operations.MoveRow(-1);
            }
            x += btnW + 2;

            // ▼ button
            using (new EditorGUI.DisabledScope(!hasSelection || selectedRowIndex >= rowsProperty.arraySize - 1))
            {
                if (GUI.Button(new Rect(x, y, btnW, h), "↓", EditorStyles.toolbarButton))
                    operations.MoveRow(1);
            }

            // Right side: row info
            if (hasSelection)
            {
                SerializedProperty sel = rowsProperty.GetArrayElementAtIndex(selectedRowIndex);
                int rowId = sel.FindPropertyRelative("rowId").intValue;

                float rightX = toolbarRect.xMax - 130;
                GUI.Label(new Rect(rightX, y, 60, h), "Row Name:", EditorStyles.miniLabel);
                rightX += 62;

                SerializedProperty idProp = sel.FindPropertyRelative("rowId");
                idProp.intValue = EditorGUI.IntField(new Rect(rightX, y + 1, 50, h - 2), idProp.intValue);

                // Center label
                string label = $"Row Editor  —  Row ID: {rowId}";
                float labelW = EditorStyles.boldLabel.CalcSize(new GUIContent(label)).x;
                float labelX = toolbarRect.x + (toolbarRect.width - labelW) * 0.5f;
                GUI.Label(new Rect(labelX, y, labelW, h), label, EditorStyles.boldLabel);
            }
            else
            {
                float labelX = toolbarRect.x + (toolbarRect.width - 80) * 0.5f;
                GUI.Label(new Rect(labelX, y, 80, h), "Row Editor", EditorStyles.boldLabel);
            }
        }

        private static void DrawProp(SerializedProperty parent, string childName, string label)
        {
            SerializedProperty prop = parent.FindPropertyRelative(childName);
            if (prop != null)
                EditorGUILayout.PropertyField(prop, new GUIContent(label), true);
        }
    }
}