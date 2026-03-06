// Done
using UnityEditor;

namespace Editor.Drawers.DialogueTable
{
    /// <summary>
    /// Handles add, remove, and move operations on DialogueTable rows.
    /// </summary>
    internal class DialogueRowOperations
    {
        private readonly SerializedObject _serializedTable;
        private readonly SerializedProperty _rowsProperty;

        /// <summary>
        /// The currently selected row index. Updated by operations that change selection.
        /// </summary>
        public int SelectedRowIndex { get; set; } = -1;

        public DialogueRowOperations(SerializedObject serializedTable, SerializedProperty rowsProperty)
        {
            _serializedTable = serializedTable;
            _rowsProperty = rowsProperty;
        }

        public void AddRow()
        {
            int insertAt = SelectedRowIndex >= 0 ? SelectedRowIndex + 1 : _rowsProperty.arraySize;
            _rowsProperty.InsertArrayElementAtIndex(insertAt);

            SerializedProperty newRow = _rowsProperty.GetArrayElementAtIndex(insertAt);

            // Auto-assign next available ID
            int nextId = 1;
            for (int i = 0; i < _rowsProperty.arraySize; i++)
            {
                int existing = _rowsProperty.GetArrayElementAtIndex(i).FindPropertyRelative("rowId").intValue;
                if (existing >= nextId) nextId = existing + 1;
            }

            newRow.FindPropertyRelative("rowId").intValue = nextId;
            newRow.FindPropertyRelative("speaker").objectReferenceValue = null;
            newRow.FindPropertyRelative("textLines").ClearArray();
            newRow.FindPropertyRelative("portraitOverride").objectReferenceValue = null;
            newRow.FindPropertyRelative("actionType").enumValueIndex = 0;
            newRow.FindPropertyRelative("playerChoiceAnswer").stringValue = "";
            newRow.FindPropertyRelative("choiceRowIds").ClearArray();
            newRow.FindPropertyRelative("audioClip").objectReferenceValue = null;
            newRow.FindPropertyRelative("audioStartTime").floatValue = 0f;
            newRow.FindPropertyRelative("dialogSkip").boolValue = false;
            newRow.FindPropertyRelative("leadsTo").intValue = -1;

            _serializedTable.ApplyModifiedProperties();
            SelectedRowIndex = insertAt;
        }

        public void RemoveSelectedRow()
        {
            if (SelectedRowIndex < 0 || SelectedRowIndex >= _rowsProperty.arraySize)
                return;

            _rowsProperty.DeleteArrayElementAtIndex(SelectedRowIndex);
            _serializedTable.ApplyModifiedProperties();

            if (SelectedRowIndex >= _rowsProperty.arraySize)
                SelectedRowIndex = _rowsProperty.arraySize - 1;
        }

        public void MoveRow(int direction)
        {
            int target = SelectedRowIndex + direction;
            if (target < 0 || target >= _rowsProperty.arraySize) return;

            _rowsProperty.MoveArrayElement(SelectedRowIndex, target);
            _serializedTable.ApplyModifiedProperties();
            SelectedRowIndex = target;
        }
    }
}