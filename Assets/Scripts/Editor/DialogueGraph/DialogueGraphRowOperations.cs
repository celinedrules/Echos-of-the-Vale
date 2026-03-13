using System.Collections.Generic;
using Data.DialogueData;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Utilities.Enums;

namespace Editor.DialogueGraph
{
    public static class DialogueGraphRowOperations
    {
        public static int CreateRow(
            DialogueTable table,
            DialogueRowKind rowKind,
            int selectedRowId,
            Vector2 newPosition)
        {
            if (table == null)
                return -1;

            Undo.RecordObject(table, $"Create {rowKind} Dialogue Row");

            SerializedObject tableObject = new SerializedObject(table);
            SerializedProperty rowsProperty = tableObject.FindProperty("rows");

            int newIndex = rowsProperty.arraySize;
            rowsProperty.InsertArrayElementAtIndex(newIndex);

            SerializedProperty newRowProperty = rowsProperty.GetArrayElementAtIndex(newIndex);
            int newRowId = GetNextAvailableRowId(table);

            newRowProperty.FindPropertyRelative("rowId").intValue = newRowId;
            newRowProperty.FindPropertyRelative("rowKind").enumValueIndex = (int)rowKind;
            newRowProperty.FindPropertyRelative("speaker").objectReferenceValue = null;

            SerializedProperty textLinesProperty = newRowProperty.FindPropertyRelative("textLines");
            textLinesProperty.arraySize = 1;
            textLinesProperty.GetArrayElementAtIndex(0).stringValue = GetDefaultTextLine(rowKind);

            newRowProperty.FindPropertyRelative("portraitOverride").objectReferenceValue = null;
            newRowProperty.FindPropertyRelative("rowAction").enumValueIndex = 0;
            newRowProperty.FindPropertyRelative("playerChoiceAnswer").stringValue =
                rowKind == DialogueRowKind.ChoiceResponse ? "New Choice Response" : string.Empty;

            SerializedProperty choiceRowIdsProperty = newRowProperty.FindPropertyRelative("choiceRowIds");
            choiceRowIdsProperty.arraySize = 0;

            newRowProperty.FindPropertyRelative("audioClip").objectReferenceValue = null;
            newRowProperty.FindPropertyRelative("audioStartTime").floatValue = 0f;
            newRowProperty.FindPropertyRelative("dialogSkip").boolValue = false;
            newRowProperty.FindPropertyRelative("leadsTo").intValue = -1;
            newRowProperty.FindPropertyRelative("changeStartRowId").boolValue = false;
            newRowProperty.FindPropertyRelative("newStartRowId").intValue = 0;

            tableObject.ApplyModifiedProperties();

            table.SetNodePosition(newRowId, newPosition);

            EditorUtility.SetDirty(table);
            AssetDatabase.SaveAssets();

            return newRowId;
        }

        public static int DuplicateRow(DialogueTable table, int sourceRowId, Vector2 newPosition)
        {
            return DuplicateRowInternal(table, sourceRowId, newPosition, resetLinks: false);
        }

        public static int DuplicateRowResetLinks(DialogueTable table, int sourceRowId, Vector2 newPosition)
        {
            return DuplicateRowInternal(table, sourceRowId, newPosition, resetLinks: true);
        }

        public static bool ConnectRows(DialogueTable table, int sourceRowId, int targetRowId, out string errorMessage)
        {
            errorMessage = null;

            if (table == null)
            {
                errorMessage = "No DialogueTable selected.";
                return false;
            }

            if (sourceRowId < 0 || targetRowId < 0)
            {
                errorMessage = "Source or target row is invalid.";
                return false;
            }

            if (sourceRowId == targetRowId)
            {
                errorMessage = "Cannot connect a row to itself.";
                return false;
            }

            int sourceRowIndex = FindRowIndexById(table, sourceRowId);
            int targetRowIndex = FindRowIndexById(table, targetRowId);

            if (sourceRowIndex < 0 || targetRowIndex < 0)
            {
                errorMessage = "Source or target row was not found.";
                return false;
            }

            Undo.RecordObject(table, $"Connect Dialogue Row {sourceRowId} -> {targetRowId}");

            SerializedObject tableObject = new SerializedObject(table);
            SerializedProperty rowsProperty = tableObject.FindProperty("rows");
            SerializedProperty sourceRowProperty = rowsProperty.GetArrayElementAtIndex(sourceRowIndex);
            SerializedProperty targetRowProperty = rowsProperty.GetArrayElementAtIndex(targetRowIndex);

            DialogueRowKind sourceKind =
                (DialogueRowKind)sourceRowProperty.FindPropertyRelative("rowKind").enumValueIndex;

            DialogueRowKind targetKind =
                (DialogueRowKind)targetRowProperty.FindPropertyRelative("rowKind").enumValueIndex;

            if (sourceKind == DialogueRowKind.ChoicePrompt)
            {
                if (targetKind != DialogueRowKind.ChoiceResponse)
                {
                    errorMessage = "Choice Prompt rows can only connect to Choice Response rows.";
                    return false;
                }

                SerializedProperty choiceRowIdsProperty = sourceRowProperty.FindPropertyRelative("choiceRowIds");

                for (int i = 0; i < choiceRowIdsProperty.arraySize; i++)
                {
                    if (choiceRowIdsProperty.GetArrayElementAtIndex(i).intValue == targetRowId)
                    {
                        tableObject.ApplyModifiedProperties();
                        EditorUtility.SetDirty(table);
                        AssetDatabase.SaveAssets();
                        return true;
                    }
                }

                int newChoiceIndex = choiceRowIdsProperty.arraySize;
                choiceRowIdsProperty.arraySize++;
                choiceRowIdsProperty.GetArrayElementAtIndex(newChoiceIndex).intValue = targetRowId;
            }
            else
            {
                sourceRowProperty.FindPropertyRelative("leadsTo").intValue = targetRowId;
            }

            tableObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(table);
            AssetDatabase.SaveAssets();
            return true;
        }

        public static bool ClearOutgoingLinks(DialogueTable table, int rowId)
        {
            if (table == null || rowId < 0)
                return false;

            int rowIndex = FindRowIndexById(table, rowId);
            if (rowIndex < 0)
                return false;

            Undo.RecordObject(table, $"Clear Dialogue Links {rowId}");

            SerializedObject tableObject = new SerializedObject(table);
            SerializedProperty rowsProperty = tableObject.FindProperty("rows");
            SerializedProperty rowProperty = rowsProperty.GetArrayElementAtIndex(rowIndex);

            DialogueRowKind rowKind =
                (DialogueRowKind)rowProperty.FindPropertyRelative("rowKind").enumValueIndex;

            if (rowKind == DialogueRowKind.ChoicePrompt)
            {
                SerializedProperty choiceRowIdsProperty = rowProperty.FindPropertyRelative("choiceRowIds");
                choiceRowIdsProperty.arraySize = 0;
            }
            else
            {
                rowProperty.FindPropertyRelative("leadsTo").intValue = -1;
            }

            tableObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(table);
            AssetDatabase.SaveAssets();
            return true;
        }

        public static bool DeleteSelectedRow(DialogueTable table, int selectedRowId)
        {
            if (table == null || selectedRowId < 0)
                return false;

            DialogueRow selectedRow = table.GetRowById(selectedRowId);
            if (selectedRow == null)
                return false;

            bool confirmed = EditorUtility.DisplayDialog(
                "Delete Dialogue Row",
                $"Delete row {selectedRowId}?\n\nReferences to this row will be cleaned up.",
                "Delete",
                "Cancel");

            if (!confirmed)
                return false;

            Undo.RecordObject(table, $"Delete Dialogue Row {selectedRowId}");

            int rowIndexToDelete = FindRowIndexById(table, selectedRowId);

            SerializedObject tableObject = new SerializedObject(table);
            SerializedProperty rowsProperty = tableObject.FindProperty("rows");

            CleanupReferencesToDeletedRow(rowsProperty, selectedRowId);

            if (rowIndexToDelete >= 0 && rowIndexToDelete < rowsProperty.arraySize)
                rowsProperty.DeleteArrayElementAtIndex(rowIndexToDelete);

            tableObject.ApplyModifiedProperties();

            table.RemoveNodePosition(selectedRowId);
            table.PruneMissingNodePositions();

            EditorUtility.SetDirty(table);
            AssetDatabase.SaveAssets();

            return true;
        }

        public static int FindRowIndexById(DialogueTable table, int rowId)
        {
            if (table == null)
                return -1;

            for (int i = 0; i < table.RowCount; i++)
            {
                DialogueRow row = table.GetRow(i);
                if (row != null && row.RowId == rowId)
                    return i;
            }

            return -1;
        }

        public static int GetNextAvailableRowId(DialogueTable table)
        {
            if (table == null)
                return 0;

            int candidateId = 0;
            while (table.HasRowId(candidateId))
                candidateId++;

            return candidateId;
        }

        public static Vector2 GetNewRowPosition(
            int selectedRowId,
            IReadOnlyDictionary<int, VisualElement> nodeViewsByRowId,
            int rowCount,
            float defaultStartX,
            float defaultStartY,
            float defaultVerticalSpacing)
        {
            if (selectedRowId >= 0 &&
                nodeViewsByRowId != null &&
                nodeViewsByRowId.TryGetValue(selectedRowId, out VisualElement selectedNode))
            {
                return new Vector2(
                    selectedNode.resolvedStyle.left + 320f,
                    selectedNode.resolvedStyle.top);
            }

            return new Vector2(defaultStartX, defaultStartY + rowCount * defaultVerticalSpacing);
        }

        private static int DuplicateRowInternal(DialogueTable table, int sourceRowId, Vector2 newPosition, bool resetLinks)
        {
            if (table == null || sourceRowId < 0)
                return -1;

            int sourceRowIndex = FindRowIndexById(table, sourceRowId);
            if (sourceRowIndex < 0)
                return -1;

            string undoLabel = resetLinks
                ? $"Duplicate Dialogue Row {sourceRowId} (Reset Links)"
                : $"Duplicate Dialogue Row {sourceRowId}";

            Undo.RecordObject(table, undoLabel);

            SerializedObject tableObject = new SerializedObject(table);
            SerializedProperty rowsProperty = tableObject.FindProperty("rows");

            SerializedProperty sourceRowProperty = rowsProperty.GetArrayElementAtIndex(sourceRowIndex);

            int newIndex = rowsProperty.arraySize;
            rowsProperty.InsertArrayElementAtIndex(newIndex);

            SerializedProperty duplicatedRowProperty = rowsProperty.GetArrayElementAtIndex(newIndex);

            CopyRowProperties(sourceRowProperty, duplicatedRowProperty);

            int newRowId = GetNextAvailableRowId(table);
            duplicatedRowProperty.FindPropertyRelative("rowId").intValue = newRowId;

            if (resetLinks)
                ResetLinkProperties(duplicatedRowProperty);

            tableObject.ApplyModifiedProperties();

            table.SetNodePosition(newRowId, newPosition);

            EditorUtility.SetDirty(table);
            AssetDatabase.SaveAssets();

            return newRowId;
        }

        private static void CopyRowProperties(SerializedProperty sourceRowProperty, SerializedProperty targetRowProperty)
        {
            targetRowProperty.FindPropertyRelative("rowKind").enumValueIndex =
                sourceRowProperty.FindPropertyRelative("rowKind").enumValueIndex;

            targetRowProperty.FindPropertyRelative("speaker").objectReferenceValue =
                sourceRowProperty.FindPropertyRelative("speaker").objectReferenceValue;

            CopyStringArray(
                sourceRowProperty.FindPropertyRelative("textLines"),
                targetRowProperty.FindPropertyRelative("textLines"));

            targetRowProperty.FindPropertyRelative("portraitOverride").objectReferenceValue =
                sourceRowProperty.FindPropertyRelative("portraitOverride").objectReferenceValue;

            targetRowProperty.FindPropertyRelative("rowAction").enumValueIndex =
                sourceRowProperty.FindPropertyRelative("rowAction").enumValueIndex;

            targetRowProperty.FindPropertyRelative("playerChoiceAnswer").stringValue =
                sourceRowProperty.FindPropertyRelative("playerChoiceAnswer").stringValue;

            CopyIntArray(
                sourceRowProperty.FindPropertyRelative("choiceRowIds"),
                targetRowProperty.FindPropertyRelative("choiceRowIds"));

            targetRowProperty.FindPropertyRelative("audioClip").objectReferenceValue =
                sourceRowProperty.FindPropertyRelative("audioClip").objectReferenceValue;

            targetRowProperty.FindPropertyRelative("audioStartTime").floatValue =
                sourceRowProperty.FindPropertyRelative("audioStartTime").floatValue;

            targetRowProperty.FindPropertyRelative("dialogSkip").boolValue =
                sourceRowProperty.FindPropertyRelative("dialogSkip").boolValue;

            targetRowProperty.FindPropertyRelative("leadsTo").intValue =
                sourceRowProperty.FindPropertyRelative("leadsTo").intValue;

            targetRowProperty.FindPropertyRelative("changeStartRowId").boolValue =
                sourceRowProperty.FindPropertyRelative("changeStartRowId").boolValue;

            targetRowProperty.FindPropertyRelative("newStartRowId").intValue =
                sourceRowProperty.FindPropertyRelative("newStartRowId").intValue;
        }

        private static void ResetLinkProperties(SerializedProperty rowProperty)
        {
            rowProperty.FindPropertyRelative("leadsTo").intValue = -1;

            SerializedProperty choiceRowIdsProperty = rowProperty.FindPropertyRelative("choiceRowIds");
            if (choiceRowIdsProperty != null && choiceRowIdsProperty.isArray)
                choiceRowIdsProperty.arraySize = 0;

            rowProperty.FindPropertyRelative("changeStartRowId").boolValue = false;
            rowProperty.FindPropertyRelative("newStartRowId").intValue = 0;
        }

        private static void CopyStringArray(SerializedProperty sourceArray, SerializedProperty targetArray)
        {
            if (sourceArray == null || targetArray == null)
                return;

            targetArray.arraySize = sourceArray.arraySize;

            for (int i = 0; i < sourceArray.arraySize; i++)
                targetArray.GetArrayElementAtIndex(i).stringValue = sourceArray.GetArrayElementAtIndex(i).stringValue;
        }

        private static void CopyIntArray(SerializedProperty sourceArray, SerializedProperty targetArray)
        {
            if (sourceArray == null || targetArray == null)
                return;

            targetArray.arraySize = sourceArray.arraySize;

            for (int i = 0; i < sourceArray.arraySize; i++)
                targetArray.GetArrayElementAtIndex(i).intValue = sourceArray.GetArrayElementAtIndex(i).intValue;
        }

        private static void CleanupReferencesToDeletedRow(SerializedProperty rowsProperty, int deletedRowId)
        {
            for (int i = 0; i < rowsProperty.arraySize; i++)
            {
                SerializedProperty rowProperty = rowsProperty.GetArrayElementAtIndex(i);
                if (rowProperty == null)
                    continue;

                SerializedProperty rowIdProperty = rowProperty.FindPropertyRelative("rowId");
                if (rowIdProperty != null && rowIdProperty.intValue == deletedRowId)
                    continue;

                SerializedProperty leadsToProperty = rowProperty.FindPropertyRelative("leadsTo");
                if (leadsToProperty != null && leadsToProperty.intValue == deletedRowId)
                    leadsToProperty.intValue = -1;

                SerializedProperty choiceRowIdsProperty = rowProperty.FindPropertyRelative("choiceRowIds");
                if (choiceRowIdsProperty == null || !choiceRowIdsProperty.isArray)
                    continue;

                for (int j = choiceRowIdsProperty.arraySize - 1; j >= 0; j--)
                {
                    SerializedProperty choiceElement = choiceRowIdsProperty.GetArrayElementAtIndex(j);
                    if (choiceElement.intValue == deletedRowId)
                        choiceRowIdsProperty.DeleteArrayElementAtIndex(j);
                }
            }
        }

        private static string GetDefaultTextLine(DialogueRowKind rowKind)
        {
            switch (rowKind)
            {
                case DialogueRowKind.ChoicePrompt:
                    return "Choose an option.";
                case DialogueRowKind.ChoiceResponse:
                    return "Choice response text.";
                default:
                    return "New dialogue line.";
            }
        }
    }
}