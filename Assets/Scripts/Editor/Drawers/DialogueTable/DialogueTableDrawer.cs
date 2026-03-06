// Done
using UnityEditor;
using UnityEngine;
using static Editor.Drawers.DialogueTable.DialogueTableConstants;

namespace Editor.Drawers.DialogueTable
{
    /// <summary>
    /// Coordinates the data table grid, row editor, and splitter
    /// for editing a DialogueTable ScriptableObject.
    /// </summary>
    internal class DialogueTableDrawer
    {
        private SerializedObject _serializedTable;
        private SerializedProperty _rowsProperty;
        private Data.DialogueData.DialogueTable _table;

        private float _splitRatio = 0.45f;
        private bool _isResizing;
        private Rect _lastTotalRect;

        private readonly DialogueTableGridDrawer _gridDrawer = new();
        private readonly DialogueRowEditorDrawer _rowEditorDrawer = new();
        private DialogueRowOperations _rowOperations;

        public EditorWindow HostWindow { get; set; }

        public void SetTable(Data.DialogueData.DialogueTable table)
        {
            if (_table == table && _serializedTable != null && _serializedTable.targetObject != null)
                return;

            _table = table;
            _serializedTable = table != null ? new SerializedObject(table) : null;
            _rowsProperty = _serializedTable?.FindProperty("rows");

            _gridDrawer.SelectedRowIndex = -1;

            _rowOperations = _serializedTable != null
                ? new DialogueRowOperations(_serializedTable, _rowsProperty)
                : null;
        }

        public void Draw()
        {
            if (_table == null || _serializedTable == null)
            {
                EditorGUILayout.HelpBox("No Dialogue Table selected.", MessageType.Info);
                return;
            }

            _serializedTable.Update();
            
            // --- Editable Name Field ---
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Table Name", GUILayout.Width(75));
            
            string currentName = _table.name;
            string newName = EditorGUILayout.DelayedTextField(currentName);
            
            if (newName != currentName && !string.IsNullOrWhiteSpace(newName))
            {
                string path = AssetDatabase.GetAssetPath(_table);
                if (!string.IsNullOrEmpty(path))
                {
                    AssetDatabase.RenameAsset(path, newName);
                    AssetDatabase.SaveAssets();
                }
            }
            
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(4);

            // Consume all remaining space; cache rect to survive layout/repaint mismatch
            Rect totalRect = GUILayoutUtility.GetRect(0, 0,
                GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            if (Event.current.type == EventType.Repaint)
                _lastTotalRect = totalRect;

            if (totalRect.height < 10)
                totalRect = _lastTotalRect;

            if (totalRect.height < 10)
                return;

            // Subdivide into table / splitter / row editor
            float tableH = (totalRect.height - SplitterHeight) * _splitRatio;
            float rowEditorH = totalRect.height - tableH - SplitterHeight;

            Rect tableRect = new(totalRect.x, totalRect.y, totalRect.width, tableH);
            Rect splitterRect = new(totalRect.x, tableRect.yMax, totalRect.width, SplitterHeight);
            Rect rowEditorRect = new(totalRect.x, splitterRect.yMax, totalRect.width, rowEditorH);

            // Sync selection state
            _rowOperations.SelectedRowIndex = _gridDrawer.SelectedRowIndex;

            // Draw
            _gridDrawer.Draw(tableRect, _rowsProperty);
            
            // After the grid draws, sync the (possibly changed) selection to operations
            _rowOperations.SelectedRowIndex = _gridDrawer.SelectedRowIndex;

            DrawSplitter(splitterRect, totalRect);
            
            _rowEditorDrawer.Draw(rowEditorRect, _rowsProperty, _gridDrawer.SelectedRowIndex, _rowOperations);

            // Sync selection back (operations may have changed it)
            _gridDrawer.SelectedRowIndex = _rowOperations.SelectedRowIndex;

            _serializedTable.ApplyModifiedProperties();

            if (_isResizing)
                HostWindow?.Repaint();
        }

        private void DrawSplitter(Rect rect, Rect totalRect)
        {
            EditorGUI.DrawRect(rect, SplitterBg);
            EditorGUIUtility.AddCursorRect(rect, MouseCursor.ResizeVertical);

            Event e = Event.current;

            if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition))
            {
                _isResizing = true;
                e.Use();
            }

            if (!_isResizing) return;

            if (e.type == EventType.MouseDrag)
            {
                _splitRatio = Mathf.Clamp(
                    (e.mousePosition.y - totalRect.y) / totalRect.height, 0.15f, 0.85f);
                e.Use();
            }

            if (e.type == EventType.MouseUp)
            {
                _isResizing = false;
                e.Use();
            }
        }
    }
}