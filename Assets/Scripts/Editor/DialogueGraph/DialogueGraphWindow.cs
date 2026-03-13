using System.Collections.Generic;
using Data.DialogueData;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Utilities.Enums;

namespace Editor.DialogueGraph
{
    public class DialogueGraphWindow : EditorWindow
    {
        private const float NodeWidth = 260f;
        private const float NodeMinHeight = 110f;
        private const float DefaultStartX = 60f;
        private const float DefaultStartY = 60f;
        private const float DefaultVerticalSpacing = 170f;
        private const float InspectorWidth = 360f;
        private const float FramePadding = 40f;

        private DialogueTable _selectedTable;
        private Label _tableStatusLabel;
        private ScrollView _graphScrollView;
        private VisualElement _graphCanvas;
        private ObjectField _tableField;
        private DialogueGraphInspectorView _inspectorView;
        private DialogueGraphValidationView _validationView;

        private readonly Dictionary<int, VisualElement> _nodeViewsByRowId = new();
        private HashSet<int> _invalidRowIds = new();
        private Dictionary<int, List<string>> _validationMessagesByRowId = new();

        private int _selectedRowId = -1;
        private int _selectedRowIndex = -1;

        public bool HasSelectedTable => _selectedTable != null;
        public bool HasSelectedRow => _selectedRowId >= 0;

        [MenuItem("Tools/Dialogue/Dialogue Graph")]
        public static void Open()
        {
            DialogueGraphWindow window = GetWindow<DialogueGraphWindow>();
            window.titleContent = new GUIContent("Dialogue Graph");
            window.minSize = new Vector2(1100f, 650f);
        }

        private void CreateGUI()
        {
            rootVisualElement.Clear();
            rootVisualElement.style.flexGrow = 1;
            rootVisualElement.style.backgroundColor = new Color(0.11f, 0.11f, 0.11f);

            VisualElement rootContainer = new VisualElement();
            rootContainer.style.flexGrow = 1;
            rootContainer.style.flexDirection = FlexDirection.Column;

            VisualElement toolbar = BuildToolbar();

            _tableStatusLabel = new Label("No DialogueTable selected.");
            _tableStatusLabel.style.paddingLeft = 10;
            _tableStatusLabel.style.paddingRight = 10;
            _tableStatusLabel.style.paddingTop = 6;
            _tableStatusLabel.style.paddingBottom = 6;
            _tableStatusLabel.style.color = new Color(0.78f, 0.78f, 0.78f);

            VisualElement mainArea = new VisualElement();
            mainArea.style.flexGrow = 1;
            mainArea.style.flexDirection = FlexDirection.Column;

            VisualElement contentRow = new VisualElement();
            contentRow.style.flexGrow = 1;
            contentRow.style.flexDirection = FlexDirection.Row;

            _graphScrollView = new ScrollView(ScrollViewMode.VerticalAndHorizontal);
            _graphScrollView.style.flexGrow = 1;
            _graphScrollView.style.backgroundColor = new Color(0.12f, 0.12f, 0.12f);

            _graphCanvas = new VisualElement();
            _graphCanvas.style.position = Position.Relative;
            _graphCanvas.style.width = 4000f;
            _graphCanvas.style.height = 4000f;
            _graphCanvas.style.backgroundColor = new Color(0.13f, 0.13f, 0.13f);
            _graphCanvas.generateVisualContent += OnGraphCanvasGenerateVisualContent;
            _graphCanvas.RegisterCallback<PointerDownEvent>(OnCanvasPointerDown);
            _graphCanvas.AddManipulator(new ContextualMenuManipulator(evt =>
                DialogueGraphContextMenus.BuildCanvasMenu(this, evt)));

            _graphScrollView.Add(_graphCanvas);

            _inspectorView = new DialogueGraphInspectorView(
                InspectorWidth,
                RefreshGraph,
                ClearSelection,
                SetSelectedRowIndex);

            _validationView = new DialogueGraphValidationView(SelectRowById);

            contentRow.Add(_graphScrollView);
            contentRow.Add(_inspectorView.Build());

            mainArea.Add(contentRow);
            mainArea.Add(_validationView.Build());

            rootContainer.Add(toolbar);
            rootContainer.Add(_tableStatusLabel);
            rootContainer.Add(mainArea);

            rootVisualElement.Add(rootContainer);

            RefreshAllViews();
        }

        private VisualElement BuildToolbar()
        {
            VisualElement toolbar = new VisualElement();
            toolbar.style.flexDirection = FlexDirection.Row;
            toolbar.style.alignItems = Align.Center;
            toolbar.style.paddingLeft = 10;
            toolbar.style.paddingRight = 10;
            toolbar.style.paddingTop = 8;
            toolbar.style.paddingBottom = 8;
            toolbar.style.backgroundColor = new Color(0.15f, 0.15f, 0.15f);

            Label tableLabel = new Label("Dialogue Table");
            tableLabel.style.minWidth = 100f;
            tableLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            tableLabel.style.color = new Color(0.92f, 0.92f, 0.92f);

            _tableField = new ObjectField
            {
                objectType = typeof(DialogueTable),
                allowSceneObjects = false,
                value = _selectedTable
            };
            _tableField.style.flexGrow = 1;
            _tableField.style.marginLeft = 8f;
            _tableField.RegisterValueChangedCallback(evt =>
            {
                _selectedTable = evt.newValue as DialogueTable;
                ClearSelection();
                RefreshAllViews();
            });

            Button useSelectionButton = new Button(() =>
            {
                _selectedTable = Selection.activeObject as DialogueTable;
                _tableField.SetValueWithoutNotify(_selectedTable);
                ClearSelection();
                RefreshAllViews();
            })
            {
                text = "Use Selected"
            };
            useSelectionButton.style.marginLeft = 8f;

            Button addLineButton = new Button(() => CreateRow(DialogueRowKind.Line))
            {
                text = "Add Line"
            };
            addLineButton.style.marginLeft = 8f;

            Button addChoicePromptButton = new Button(() => CreateRow(DialogueRowKind.ChoicePrompt))
            {
                text = "Add Choice Prompt"
            };
            addChoicePromptButton.style.marginLeft = 8f;

            Button addChoiceResponseButton = new Button(() => CreateRow(DialogueRowKind.ChoiceResponse))
            {
                text = "Add Choice Response"
            };
            addChoiceResponseButton.style.marginLeft = 8f;

            Button duplicateSelectedButton = new Button(DuplicateSelectedRow)
            {
                text = "Duplicate Selected"
            };
            duplicateSelectedButton.style.marginLeft = 8f;

            Button duplicateSelectedResetLinksButton = new Button(DuplicateSelectedRowResetLinks)
            {
                text = "Duplicate Selected (Reset Links)"
            };
            duplicateSelectedResetLinksButton.style.marginLeft = 8f;

            Button deleteSelectedButton = new Button(DeleteSelectedRow)
            {
                text = "Delete Selected"
            };
            deleteSelectedButton.style.marginLeft = 8f;

            Button refreshButton = new Button(RefreshAllViews)
            {
                text = "Refresh"
            };
            refreshButton.style.marginLeft = 8f;

            toolbar.Add(tableLabel);
            toolbar.Add(_tableField);
            toolbar.Add(useSelectionButton);
            toolbar.Add(addLineButton);
            toolbar.Add(addChoicePromptButton);
            toolbar.Add(addChoiceResponseButton);
            toolbar.Add(duplicateSelectedButton);
            toolbar.Add(duplicateSelectedResetLinksButton);
            toolbar.Add(deleteSelectedButton);
            toolbar.Add(refreshButton);

            return toolbar;
        }

        private void RefreshAllViews()
        {
            RefreshValidationState();
            RefreshGraph();
            RefreshInspector();
            RefreshValidation();
        }

        private void RefreshValidationState()
        {
            if (_selectedTable == null)
            {
                _invalidRowIds = new HashSet<int>();
                _validationMessagesByRowId = new Dictionary<int, List<string>>();
                return;
            }

            _invalidRowIds = DialogueGraphValidationUtility.GetInvalidRowIds(_selectedTable);
            _validationMessagesByRowId = DialogueGraphValidationUtility.GetValidationMessagesByRowId(_selectedTable);
        }

        private void RefreshGraph()
        {
            if (_graphCanvas == null)
                return;

            _graphCanvas.Clear();
            _nodeViewsByRowId.Clear();

            if (_selectedTable == null)
            {
                _tableStatusLabel.text = "No DialogueTable selected.";
                _graphCanvas.Add(BuildCenteredMessage("Select a DialogueTable to build the dialogue graph view."));
                _graphCanvas.MarkDirtyRepaint();
                return;
            }

            _selectedTable.PruneMissingNodePositions();
            EditorUtility.SetDirty(_selectedTable);

            _tableStatusLabel.text = $"{_selectedTable.name}  •  Rows: {_selectedTable.RowCount}";

            if (_selectedRowId >= 0 && DialogueGraphRowOperations.FindRowIndexById(_selectedTable, _selectedRowId) < 0)
                ClearSelection();

            if (_selectedTable.RowCount == 0)
            {
                _graphCanvas.Add(BuildCenteredMessage("This DialogueTable has no rows."));
                _graphCanvas.MarkDirtyRepaint();
                return;
            }

            for (int i = 0; i < _selectedTable.RowCount; i++)
            {
                DialogueRow row = _selectedTable.GetRow(i);
                if (row == null)
                    continue;

                Vector2 position = _selectedTable.GetNodePosition(row.RowId, GetDefaultPosition(i));
                _validationMessagesByRowId.TryGetValue(row.RowId, out List<string> rowValidationMessages);

                VisualElement node = DialogueGraphNodeViewFactory.CreateNode(
                    this,
                    row,
                    position,
                    i,
                    NodeWidth,
                    NodeMinHeight,
                    rowValidationMessages);

                _nodeViewsByRowId[row.RowId] = node;
                _graphCanvas.Add(node);
            }

            UpdateNodeSelectionVisuals();
            _graphCanvas.MarkDirtyRepaint();
        }

        public void SelectRow(int rowId, int rowIndex)
        {
            _selectedRowId = rowId;
            _selectedRowIndex = rowIndex;
            UpdateNodeSelectionVisuals();
            RefreshInspector();
            RefreshValidation();
        }

        public void SelectRowById(int rowId)
        {
            if (_selectedTable == null)
                return;

            int rowIndex = DialogueGraphRowOperations.FindRowIndexById(_selectedTable, rowId);
            if (rowIndex < 0)
                return;

            SelectRow(rowId, rowIndex);

            if (_nodeViewsByRowId.TryGetValue(rowId, out VisualElement node))
            {
                node.BringToFront();
                FrameNode(node);
            }
        }

        private void FrameNode(VisualElement node)
        {
            if (_graphScrollView == null || node == null)
                return;

            Rect nodeRect = node.layout;
            if (nodeRect.width <= 0f || nodeRect.height <= 0f)
                return;

            Vector2 currentOffset = _graphScrollView.scrollOffset;
            float viewportWidth = _graphScrollView.contentViewport.layout.width;
            float viewportHeight = _graphScrollView.contentViewport.layout.height;

            if (viewportWidth <= 0f || viewportHeight <= 0f)
                return;

            float targetX = currentOffset.x;
            float targetY = currentOffset.y;

            if (nodeRect.xMin - FramePadding < currentOffset.x)
            {
                targetX = Mathf.Max(0f, nodeRect.xMin - FramePadding);
            }
            else if (nodeRect.xMax + FramePadding > currentOffset.x + viewportWidth)
            {
                targetX = Mathf.Max(0f, nodeRect.xMax + FramePadding - viewportWidth);
            }

            if (nodeRect.yMin - FramePadding < currentOffset.y)
            {
                targetY = Mathf.Max(0f, nodeRect.yMin - FramePadding);
            }
            else if (nodeRect.yMax + FramePadding > currentOffset.y + viewportHeight)
            {
                targetY = Mathf.Max(0f, nodeRect.yMax + FramePadding - viewportHeight);
            }

            _graphScrollView.scrollOffset = new Vector2(targetX, targetY);
        }

        private void ClearSelection()
        {
            _selectedRowId = -1;
            _selectedRowIndex = -1;
            UpdateNodeSelectionVisuals();
        }

        private void SetSelectedRowIndex(DialogueTable table, int rowIndex)
        {
            if (table == _selectedTable)
                _selectedRowIndex = rowIndex;
        }

        private void UpdateNodeSelectionVisuals()
        {
            foreach (KeyValuePair<int, VisualElement> pair in _nodeViewsByRowId)
            {
                bool isSelected = pair.Key == _selectedRowId;
                bool isInvalid = _invalidRowIds.Contains(pair.Key);
                DialogueGraphNodeViewFactory.SetNodeState(pair.Value, isSelected, isInvalid);
            }
        }

        private void RefreshInspector()
        {
            _inspectorView?.Refresh(_selectedTable, _selectedRowId, _selectedRowIndex);
        }

        private void RefreshValidation()
        {
            _validationView?.Refresh(_selectedTable);
        }

        private void CreateRow(DialogueRowKind rowKind)
        {
            if (_selectedTable == null)
            {
                EditorUtility.DisplayDialog("Dialogue Graph", "Select a DialogueTable first.", "OK");
                return;
            }

            Vector2 position = DialogueGraphRowOperations.GetNewRowPosition(
                _selectedRowId,
                _nodeViewsByRowId,
                _selectedTable.RowCount,
                DefaultStartX,
                DefaultStartY,
                DefaultVerticalSpacing);

            int newRowId = DialogueGraphRowOperations.CreateRow(_selectedTable, rowKind, _selectedRowId, position);
            if (newRowId < 0)
                return;

            RefreshValidationState();
            SelectRow(newRowId, DialogueGraphRowOperations.FindRowIndexById(_selectedTable, newRowId));
            RefreshAllViews();
        }

        public void CreateRowFromMenu(DialogueRowKind rowKind)
        {
            CreateRow(rowKind);
        }

        private void DuplicateSelectedRow()
        {
            if (_selectedTable == null)
            {
                EditorUtility.DisplayDialog("Dialogue Graph", "Select a DialogueTable first.", "OK");
                return;
            }

            if (_selectedRowId < 0)
            {
                EditorUtility.DisplayDialog("Dialogue Graph", "Select a row node to duplicate.", "OK");
                return;
            }

            Vector2 position = DialogueGraphRowOperations.GetNewRowPosition(
                _selectedRowId,
                _nodeViewsByRowId,
                _selectedTable.RowCount,
                DefaultStartX,
                DefaultStartY,
                DefaultVerticalSpacing);

            int newRowId = DialogueGraphRowOperations.DuplicateRow(_selectedTable, _selectedRowId, position);
            if (newRowId < 0)
                return;

            RefreshValidationState();
            SelectRow(newRowId, DialogueGraphRowOperations.FindRowIndexById(_selectedTable, newRowId));
            RefreshAllViews();
        }

        public void DuplicateSelectedRowFromMenu()
        {
            DuplicateSelectedRow();
        }

        private void DuplicateSelectedRowResetLinks()
        {
            if (_selectedTable == null)
            {
                EditorUtility.DisplayDialog("Dialogue Graph", "Select a DialogueTable first.", "OK");
                return;
            }

            if (_selectedRowId < 0)
            {
                EditorUtility.DisplayDialog("Dialogue Graph", "Select a row node to duplicate.", "OK");
                return;
            }

            Vector2 position = DialogueGraphRowOperations.GetNewRowPosition(
                _selectedRowId,
                _nodeViewsByRowId,
                _selectedTable.RowCount,
                DefaultStartX,
                DefaultStartY,
                DefaultVerticalSpacing);

            int newRowId = DialogueGraphRowOperations.DuplicateRowResetLinks(_selectedTable, _selectedRowId, position);
            if (newRowId < 0)
                return;

            RefreshValidationState();
            SelectRow(newRowId, DialogueGraphRowOperations.FindRowIndexById(_selectedTable, newRowId));
            RefreshAllViews();
        }

        public void DuplicateSelectedRowResetLinksFromMenu()
        {
            DuplicateSelectedRowResetLinks();
        }

        private void DeleteSelectedRow()
        {
            if (_selectedTable == null)
            {
                EditorUtility.DisplayDialog("Dialogue Graph", "Select a DialogueTable first.", "OK");
                return;
            }

            if (_selectedRowId < 0)
            {
                EditorUtility.DisplayDialog("Dialogue Graph", "Select a row node to delete.", "OK");
                return;
            }

            bool deleted = DialogueGraphRowOperations.DeleteSelectedRow(_selectedTable, _selectedRowId);
            if (!deleted)
                return;

            ClearSelection();
            RefreshAllViews();
        }

        public void DeleteSelectedRowFromMenu()
        {
            DeleteSelectedRow();
        }

        public void SaveNodePosition(int rowId, VisualElement node, Label positionLabel)
        {
            if (_selectedTable == null)
                return;

            Vector2 position = new Vector2(node.resolvedStyle.left, node.resolvedStyle.top);
            _selectedTable.SetNodePosition(rowId, position);
            EditorUtility.SetDirty(_selectedTable);
            positionLabel.text = $"({Mathf.RoundToInt(position.x)}, {Mathf.RoundToInt(position.y)})";
            MarkGraphDirty();
        }

        public void MarkGraphDirty()
        {
            _graphCanvas?.MarkDirtyRepaint();
        }

        private void OnCanvasPointerDown(PointerDownEvent evt)
        {
            if (evt.button != (int)MouseButton.LeftMouse)
                return;

            if (evt.target == _graphCanvas)
            {
                ClearSelection();
                RefreshInspector();
                RefreshValidation();
            }
        }

        private void OnGraphCanvasGenerateVisualContent(MeshGenerationContext context)
        {
            DialogueGraphEdgeRenderer.Draw(_selectedTable, _nodeViewsByRowId, context);
        }

        private static Label BuildCenteredMessage(string text)
        {
            Label label = new Label(text);
            label.style.position = Position.Absolute;
            label.style.left = 0f;
            label.style.right = 0f;
            label.style.top = 120f;
            label.style.unityTextAlign = TextAnchor.MiddleCenter;
            label.style.color = new Color(0.78f, 0.78f, 0.78f);
            label.style.fontSize = 14;
            return label;
        }

        private static Vector2 GetDefaultPosition(int index)
        {
            return new Vector2(DefaultStartX, DefaultStartY + index * DefaultVerticalSpacing);
        }
    }
}