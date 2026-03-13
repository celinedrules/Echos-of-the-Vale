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

            _titleLabel.text = selectedRowId >= 0 ? $"Inspector • Row {selectedRowId}" : "Inspector";

            if (selectedTable == null)
            {
                _root.Add(BuildMessage("Select a DialogueTable to inspect dialogue rows."));
                return;
            }

            if (selectedRowId < 0 || selectedRowIndex < 0)
            {
                _root.Add(BuildMessage("Click a node to inspect and edit its row."));
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