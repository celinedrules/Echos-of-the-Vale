using Data.DialogueData;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.DialogueGraph
{
    public class DialogueGraphInspectorView
    {
        private readonly float _width;
        private readonly System.Action _refreshGraph;
        private readonly System.Action _clearSelection;
        private readonly System.Action<DialogueTable, int> _setSelectedRowIndex;

        private VisualElement _root;
        private Label _titleLabel;

        public VisualElement Root => _root;

        public DialogueGraphInspectorView(
            float width,
            System.Action refreshGraph,
            System.Action clearSelection,
            System.Action<DialogueTable, int> setSelectedRowIndex)
        {
            _width = width;
            _refreshGraph = refreshGraph;
            _clearSelection = clearSelection;
            _setSelectedRowIndex = setSelectedRowIndex;
        }

        public VisualElement Build()
        {
            _root = new VisualElement();
            _root.style.width = _width;
            _root.style.minWidth = _width;
            _root.style.maxWidth = _width;
            _root.style.flexShrink = 0;
            _root.style.backgroundColor = new Color(0.15f, 0.15f, 0.15f);
            _root.style.borderLeftWidth = 1f;
            _root.style.borderLeftColor = new Color(0.22f, 0.22f, 0.22f);
            _root.style.paddingLeft = 10f;
            _root.style.paddingRight = 10f;
            _root.style.paddingTop = 10f;
            _root.style.paddingBottom = 10f;

            _titleLabel = new Label("Inspector");
            _titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            _titleLabel.style.fontSize = 14f;
            _titleLabel.style.color = Color.white;
            _titleLabel.style.marginBottom = 8f;

            _root.Add(_titleLabel);
            return _root;
        }

        public void Refresh(DialogueTable selectedTable, int selectedRowId, int selectedRowIndex)
        {
            if (_root == null)
                return;

            while (_root.childCount > 1)
                _root.RemoveAt(1);

            _titleLabel.text = selectedRowId >= 0 && selectedRowId != DialogueGraphWindow.StartNodeRowId
                ? $"Inspector • Row {selectedRowId}"
                : selectedRowId == DialogueGraphWindow.StartNodeRowId
                    ? "Inspector • Start"
                    : "Inspector";

            if (selectedTable == null)
            {
                _root.Add(BuildMessage("Select a DialogueTable to inspect dialogue data."));
                return;
            }

            if (selectedRowId == DialogueGraphWindow.StartNodeRowId)
            {
                _root.Add(BuildTableOverview(selectedTable, "Start node selected."));
                return;
            }

            if (selectedRowId < 0 || selectedRowIndex < 0)
            {
                _root.Add(BuildTableOverview(selectedTable, "Click a node to inspect and edit its row."));
                return;
            }

            int currentRowIndex = DialogueGraphRowOperations.FindRowIndexById(selectedTable, selectedRowId);
            if (currentRowIndex < 0)
            {
                _clearSelection?.Invoke();
                _root.Add(BuildMessage("The selected row no longer exists."));
                return;
            }

            _setSelectedRowIndex?.Invoke(selectedTable, currentRowIndex);

            SerializedObject tableObject = new SerializedObject(selectedTable);
            SerializedProperty rowsProperty = tableObject.FindProperty("rows");
            SerializedProperty rowProperty = rowsProperty.GetArrayElementAtIndex(currentRowIndex);

            ScrollView inspectorScrollView = new ScrollView();
            inspectorScrollView.style.flexGrow = 1;

            IMGUIContainer header = new IMGUIContainer(() =>
            {
                EditorGUILayout.Space(2);
                EditorGUILayout.LabelField($"Row {selectedRowId}", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Editing the selected row directly in the DialogueTable asset.", EditorStyles.wordWrappedMiniLabel);
                EditorGUILayout.Space(4);
            });

            inspectorScrollView.Add(header);

            PropertyField rowField = new PropertyField(rowProperty, "Selected Row");
            rowField.Bind(tableObject);
            inspectorScrollView.Add(rowField);

            IMGUIContainer actions = new IMGUIContainer(() =>
            {
                EditorGUILayout.Space(8);

                if (GUILayout.Button("Ping DialogueTable"))
                    EditorGUIUtility.PingObject(selectedTable);

                if (GUI.changed)
                {
                    tableObject.ApplyModifiedProperties();
                    EditorUtility.SetDirty(selectedTable);
                }
            });

            inspectorScrollView.Add(actions);
            _root.Add(inspectorScrollView);

            rowField.RegisterValueChangeCallback(_ =>
            {
                tableObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(selectedTable);
                _refreshGraph?.Invoke();
            });
        }

        private VisualElement BuildTableOverview(DialogueTable table, string footerMessage)
        {
            ScrollView scrollView = new ScrollView();
            scrollView.style.flexGrow = 1;

            VisualElement card = new VisualElement();
            card.style.backgroundColor = new Color(0.17f, 0.17f, 0.17f);
            card.style.borderTopWidth = 1f;
            card.style.borderBottomWidth = 1f;
            card.style.borderLeftWidth = 1f;
            card.style.borderRightWidth = 1f;
            card.style.borderTopColor = new Color(0.24f, 0.24f, 0.24f);
            card.style.borderBottomColor = new Color(0.24f, 0.24f, 0.24f);
            card.style.borderLeftColor = new Color(0.24f, 0.24f, 0.24f);
            card.style.borderRightColor = new Color(0.24f, 0.24f, 0.24f);
            card.style.borderTopLeftRadius = 6f;
            card.style.borderTopRightRadius = 6f;
            card.style.borderBottomLeftRadius = 6f;
            card.style.borderBottomRightRadius = 6f;
            card.style.paddingLeft = 10f;
            card.style.paddingRight = 10f;
            card.style.paddingTop = 10f;
            card.style.paddingBottom = 10f;

            Label sectionTitle = new Label("Dialogue Overview");
            sectionTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
            sectionTitle.style.color = Color.white;
            sectionTitle.style.marginBottom = 8f;

            string assetName = table.name;
            string tableName = string.IsNullOrWhiteSpace(table.TableName) ? "(Unnamed Table)" : table.TableName;
            string startRowText = table.StartRowId >= 0 ? $"Row {table.StartRowId}" : "Not connected";
            int validationIssueCount = (table.GetValidationMessages()?.Count ?? 0) +
                                       (DialogueGraphValidationUtility.HasStartNodeIssue(table) ? 1 : 0);

            card.Add(sectionTitle);
            card.Add(BuildInfoRow("Asset", assetName));
            card.Add(BuildInfoRow("Table Name", tableName));
            card.Add(BuildInfoRow("Rows", table.RowCount.ToString()));
            card.Add(BuildInfoRow("Start Row", startRowText));
            card.Add(BuildInfoRow("Validation Issues", validationIssueCount.ToString()));

            Label hintLabel = new Label(footerMessage);
            hintLabel.style.whiteSpace = WhiteSpace.Normal;
            hintLabel.style.color = new Color(0.78f, 0.78f, 0.78f);
            hintLabel.style.marginTop = 10f;

            Label controlsLabel = new Label("Tip: Left-drag nodes to move them, drag from ports to connect, and right-click connections to edit them.");
            controlsLabel.style.whiteSpace = WhiteSpace.Normal;
            controlsLabel.style.color = new Color(0.68f, 0.68f, 0.68f);
            controlsLabel.style.fontSize = 11f;
            controlsLabel.style.marginTop = 8f;

            card.Add(hintLabel);
            card.Add(controlsLabel);

            scrollView.Add(card);
            return scrollView;
        }

        private static VisualElement BuildInfoRow(string label, string value)
        {
            VisualElement row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.justifyContent = Justify.SpaceBetween;
            row.style.marginBottom = 4f;

            Label labelElement = new Label(label);
            labelElement.style.color = new Color(0.72f, 0.72f, 0.72f);
            labelElement.style.unityFontStyleAndWeight = FontStyle.Bold;
            labelElement.style.minWidth = 110f;

            Label valueElement = new Label(value);
            valueElement.style.color = new Color(0.92f, 0.92f, 0.92f);
            valueElement.style.flexGrow = 1;
            valueElement.style.unityTextAlign = TextAnchor.MiddleRight;

            row.Add(labelElement);
            row.Add(valueElement);
            return row;
        }

        private static Label BuildMessage(string text)
        {
            Label label = new Label(text);
            label.style.whiteSpace = WhiteSpace.Normal;
            label.style.color = new Color(0.78f, 0.78f, 0.78f);
            label.style.marginTop = 6f;
            return label;
        }
    }
}