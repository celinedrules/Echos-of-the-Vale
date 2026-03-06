// Done
using Data.DialogueData;
using UnityEditor;
using UnityEngine;
using static Editor.Drawers.DialogueTable.DialogueTableConstants;

namespace Editor.Drawers.DialogueTable
{
    /// <summary>
    /// Draws the table grid (header + scrollable rows) for a DialogueTable.
    /// </summary>
    internal class DialogueTableGridDrawer
    {
        private Vector2 _scrollPos;
        private int _mouseDownIndex = -1;
        private int _hoveredRowIndex = -1;
        private bool _isDragging;
        private Vector2 _mouseDownPos;

        private const float DragThreshold = 6f;

        /// <summary>
        /// The currently selected row index. -1 means no selection.
        /// </summary>
        public int SelectedRowIndex { get; set; } = -1;

        /// <summary>
        /// Returns true if the selection changed during this draw call.
        /// </summary>
        public bool SelectionChanged { get; private set; }

        public void Draw(Rect rect, SerializedProperty rowsProperty)
        {
            SelectionChanged = false;

            float textColW = GetTextColumnWidth(rect.width);

            // Header
            Rect headerRect = new(rect.x, rect.y, rect.width, ToolbarHeight);
            DrawHeader(headerRect, textColW);

            // Scrollable rows
            Rect scrollViewRect = new(rect.x, rect.y + ToolbarHeight, rect.width, rect.height - ToolbarHeight);
            int rowCount = rowsProperty.arraySize;
            float contentH = rowCount * RowHeight;
            Rect contentRect = new(0, 0, scrollViewRect.width - ScrollBarWidth,
                Mathf.Max(contentH, scrollViewRect.height));
            
            // Reset hover each frame; rows will claim it during draw
            int previousHover = _hoveredRowIndex;
            _hoveredRowIndex = -1;


            _scrollPos = GUI.BeginScrollView(scrollViewRect, _scrollPos, contentRect);
            {
                for (int i = 0; i < rowCount; i++)
                {
                    Rect rowRect = new(0, i * RowHeight, contentRect.width, RowHeight);
                    DrawRow(rowRect, rowsProperty.GetArrayElementAtIndex(i), i, textColW);
                }
            }
            GUI.EndScrollView();

            // Repaint when hover changes so highlight updates immediately
            if (_hoveredRowIndex != previousHover && Event.current.type != EventType.Layout)
                HandleUtility.Repaint();
            
            // Reset mouse-down tracking if mouse was released outside any row
            if (Event.current.type == EventType.MouseUp)
                _mouseDownIndex = -1;
        }

        private void DrawHeader(Rect rect, float textColW)
        {
            EditorGUI.DrawRect(rect, HeaderBg);
            float x = rect.x + 2;

            GUI.Label(new Rect(x, rect.y, ColId, rect.height), "#", EditorStyles.miniBoldLabel);
            x += ColId;
            GUI.Label(new Rect(x, rect.y, ColSpeaker, rect.height), "Speaker", EditorStyles.miniBoldLabel);
            x += ColSpeaker;
            GUI.Label(new Rect(x, rect.y, textColW, rect.height), "Text", EditorStyles.miniBoldLabel);
            x += textColW;
            GUI.Label(new Rect(x, rect.y, ColAction, rect.height), "Action", EditorStyles.miniBoldLabel);
            x += ColAction;
            GUI.Label(new Rect(x, rect.y, ColLeadsTo, rect.height), "LeadsTo", EditorStyles.miniBoldLabel);
            x += ColLeadsTo;
            GUI.Label(new Rect(x, rect.y, ColSkip, rect.height), "Skip", EditorStyles.miniBoldLabel);
            x += ColSkip;
            GUI.Label(new Rect(x, rect.y, ColChoiceAnswer, rect.height), "ChoiceAnswer", EditorStyles.miniBoldLabel);
            x += ColChoiceAnswer;
            GUI.Label(new Rect(x, rect.y, ColAudio, rect.height), "Audio", EditorStyles.miniBoldLabel);
        }

        private void DrawRow(Rect rect, SerializedProperty row, int index, float textColW)
        {
            bool selected = index == SelectedRowIndex;
            Color bg = selected ? RowSelected : (index % 2 == 0 ? RowEven : RowOdd);
            EditorGUI.DrawRect(rect, bg);

            // Hover highlight
            if (Event.current.type == EventType.Repaint && rect.Contains(Event.current.mousePosition) && !selected)
            {
                _hoveredRowIndex = index;
                EditorGUI.DrawRect(rect, RowHovered);
            }
            else if (Event.current.type != EventType.Repaint && rect.Contains(Event.current.mousePosition))
            {
                _hoveredRowIndex = index;
            }
            
            Event e = Event.current;
            int rowId = row.FindPropertyRelative("rowId").intValue;

            // MouseDown: just record it, don't select yet
            if (e.type == EventType.MouseDown && e.button == 0 && rect.Contains(e.mousePosition))
            {
                _mouseDownIndex = index;
                _mouseDownPos = e.mousePosition;
                _isDragging = false;
                e.Use();
            }

            // MouseDrag: only start drag after moving past the threshold
            if (e.type == EventType.MouseDrag && _mouseDownIndex == index && !_isDragging)
            {
                float distance = Vector2.Distance(e.mousePosition, _mouseDownPos);
                if (distance >= DragThreshold)
                {
                    _isDragging = true;
                    DragAndDrop.PrepareStartDrag();
                    DragAndDrop.SetGenericData(DialogueRowDragData.DragId,
                        new DialogueRowDragData(rowId, index));
                    DragAndDrop.StartDrag($"Row {rowId}");
                    _mouseDownIndex = -1;
                    e.Use();
                }
            }

            // MouseUp on the same row we pressed: this was a click, so select it
            if (e.type == EventType.MouseUp && e.button == 0 && _mouseDownIndex == index
                && rect.Contains(e.mousePosition) && !_isDragging)
            {
                SelectedRowIndex = index;
                SelectionChanged = true;
                _mouseDownIndex = -1;
                e.Use();
                GUI.FocusControl(null);
            }

            Color prevColor = GUI.contentColor;
            if (selected) GUI.contentColor = SelectedText;

            GUIStyle style = selected ? EditorStyles.boldLabel : EditorStyles.label;
            GUIStyle textStyle = selected ? EditorStyles.boldLabel : EditorStyles.miniLabel;
            float x = rect.x + 2;

            // #
            GUI.Label(new Rect(x, rect.y, ColId, rect.height), rowId.ToString(), style);
            x += ColId;

            // Speaker
            Object speakerObj = row.FindPropertyRelative("speaker").objectReferenceValue;
            string speakerName = speakerObj is DialogueSpeakerData sd
                ? sd.SpeakerName
                : (speakerObj != null ? speakerObj.name : "None");
            GUI.Label(new Rect(x, rect.y, ColSpeaker, rect.height), speakerName, style);
            x += ColSpeaker;

            // Text
            SerializedProperty textLines = row.FindPropertyRelative("textLines");
            string textPreview = textLines.arraySize > 0 ? textLines.GetArrayElementAtIndex(0).stringValue : "";
            GUI.Label(new Rect(x, rect.y, textColW, rect.height), textPreview, textStyle);
            x += textColW;

            // Action
            SerializedProperty actionProp = row.FindPropertyRelative("actionType");
            GUI.Label(new Rect(x, rect.y, ColAction, rect.height),
                actionProp.enumDisplayNames[actionProp.enumValueIndex], style);
            x += ColAction;

            // LeadsTo
            SerializedProperty leadsToProp = row.FindPropertyRelative("leadsTo");
            int leadsTo = leadsToProp.intValue;
            GUI.Label(new Rect(x, rect.y, ColLeadsTo, rect.height),
                leadsTo >= 0 ? leadsTo.ToString() : "" , style);
            x += ColLeadsTo;
            
            // Skip
            GUI.Label(new Rect(x, rect.y, ColSkip, rect.height),
                row.FindPropertyRelative("dialogSkip").boolValue ? "True" : "False", style);
            x += ColSkip;

            // ChoiceAnswer
            GUI.Label(new Rect(x, rect.y, ColChoiceAnswer, rect.height),
                row.FindPropertyRelative("playerChoiceAnswer").stringValue, textStyle);
            x += ColChoiceAnswer;

            // Audio
            Object audioObj = row.FindPropertyRelative("audioClip").objectReferenceValue;
            GUI.Label(new Rect(x, rect.y, ColAudio, rect.height),
                audioObj != null ? audioObj.name : "None", style);

            GUI.contentColor = prevColor;
        }
    }
}