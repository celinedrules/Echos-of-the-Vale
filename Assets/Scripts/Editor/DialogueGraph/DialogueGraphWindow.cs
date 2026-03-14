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
         public const int StartNodeRowId = -1000;

        private const float NodeWidth = 260f;
        private const float NodeMinHeight = 110f;
        private const float DefaultStartX = 60f;
        private const float DefaultStartY = 60f;
        private const float DefaultVerticalSpacing = 170f;
        private const float InspectorWidth = 360f;
        private const float FramePadding = 40f;

        private const float MinorGridSpacing = 20f;
        private const float MajorGridSpacing = 100f;
        private const float MinZoom = 0.5f;
        private const float MaxZoom = 1.75f;
        private const float ZoomStep = 0.05f;
        private const float VirtualCanvasSize = 4000f;

        private static readonly Color MinorGridColor = new(0.24f, 0.27f, 0.32f, 0.16f);
        private static readonly Color MajorGridColor = new(0.30f, 0.35f, 0.42f, 0.34f);

        private DialogueTable _selectedTable;
        private Label _tableStatusLabel;
        private VisualElement _graphViewport;
        private VisualElement _gridBackground;
        private VisualElement _graphContentRoot;
        private VisualElement _graphCanvas;
        private ObjectField _tableField;
        private DialogueGraphInspectorView _inspectorView;
        private DialogueGraphValidationView _validationView;
        private Label _zoomLabel;

        private readonly Dictionary<int, VisualElement> _nodeViewsByRowId = new();
        private HashSet<int> _invalidRowIds = new();
        private Dictionary<int, List<string>> _validationMessagesByRowId = new();
        private bool _isStartNodeInvalid;

        private int _selectedRowId = -1;
        private int _selectedRowIndex = -1;

        private bool _isConnectModeActive;
        private int _connectSourceRowId = -1;
        private bool _isPortDragActive;
        private Vector2 _portDragPreviewPosition;
        private int _hoveredConnectTargetRowId = -1;
        private bool _hoveredConnectTargetValid;
        private bool _isGridSnapEnabled = true;

        public bool HasSelectedTable => _selectedTable != null;
        public bool HasSelectedRow => _selectedRowId >= 0 || _selectedRowId == StartNodeRowId;
        public bool IsConnectModeActive => _isConnectModeActive;
        public bool IsGridSnapEnabled => _isGridSnapEnabled;
        public Vector2 GraphPanPosition => _selectedTable != null ? _selectedTable.GraphPanPosition : Vector2.zero;
        public float CurrentZoom => _selectedTable != null ? _selectedTable.GraphZoomScale : 1f;
        
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

            _graphViewport = new VisualElement();
            _graphViewport.style.flexGrow = 1;
            _graphViewport.style.backgroundColor = new Color(0.12f, 0.12f, 0.12f);
            _graphViewport.style.overflow = Overflow.Hidden;
            _graphViewport.RegisterCallback<WheelEvent>(OnGraphWheel, TrickleDown.TrickleDown);
            _graphViewport.AddManipulator(new DialogueGraphPanManipulator(this));

            _gridBackground = new VisualElement();
            _gridBackground.style.position = Position.Absolute;
            _gridBackground.style.left = 0f;
            _gridBackground.style.top = 0f;
            _gridBackground.style.right = 0f;
            _gridBackground.style.bottom = 0f;
            _gridBackground.pickingMode = PickingMode.Ignore;
            _gridBackground.generateVisualContent += OnGridBackgroundGenerateVisualContent;

            _graphContentRoot = new VisualElement();
            _graphContentRoot.style.position = Position.Absolute;
            _graphContentRoot.style.left = 0f;
            _graphContentRoot.style.top = 0f;
            _graphContentRoot.style.width = VirtualCanvasSize;
            _graphContentRoot.style.height = VirtualCanvasSize;
            _graphContentRoot.pickingMode = PickingMode.Ignore;

            _graphCanvas = new VisualElement();
            _graphCanvas.style.position = Position.Absolute;
            _graphCanvas.style.left = 0f;
            _graphCanvas.style.top = 0f;
            _graphCanvas.style.width = VirtualCanvasSize;
            _graphCanvas.style.height = VirtualCanvasSize;
            _graphCanvas.style.backgroundColor = Color.clear;
            _graphCanvas.generateVisualContent += OnGraphCanvasGenerateVisualContent;
            _graphCanvas.RegisterCallback<PointerDownEvent>(OnCanvasPointerDown);
            _graphCanvas.AddManipulator(new ContextualMenuManipulator(BuildGraphCanvasContextMenu));

            _graphContentRoot.Add(_graphCanvas);

            _graphViewport.Add(_gridBackground);
            _graphViewport.Add(_graphContentRoot);

            _inspectorView = new DialogueGraphInspectorView(
                InspectorWidth,
                RefreshGraph,
                ClearSelection,
                SetSelectedRowIndex);

            _validationView = new DialogueGraphValidationView(SelectRowById);

            contentRow.Add(_graphViewport);
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
            toolbar.style.paddingTop = 8f;
            toolbar.style.paddingBottom = 8f;
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
                CancelConnectMode();
                ClearSelection();
                RefreshAllViews();
            });

            Button useSelectionButton = new Button(() =>
            {
                _selectedTable = Selection.activeObject as DialogueTable;
                _tableField.SetValueWithoutNotify(_selectedTable);
                CancelConnectMode();
                ClearSelection();
                RefreshAllViews();
            })
            {
                text = "Use Selected"
            };
            useSelectionButton.style.marginLeft = 8f;

            ToolbarToggle gridSnapToggle = new ToolbarToggle
            {
                text = "Snap"
            };
            gridSnapToggle.value = _isGridSnapEnabled;
            gridSnapToggle.style.marginLeft = 8f;
            gridSnapToggle.tooltip = "Snap node movement to minor grid increments. Hold Shift to bypass while dragging.";
            gridSnapToggle.RegisterValueChangedCallback(evt =>
            {
                _isGridSnapEnabled = evt.newValue;
            });

            Button zoomOutButton = new Button(() => AdjustZoom(-ZoomStep))
            {
                text = "−"
            };
            zoomOutButton.style.marginLeft = 8f;
            zoomOutButton.tooltip = "Zoom out";

            _zoomLabel = new Label("100%");
            _zoomLabel.style.minWidth = 48f;
            _zoomLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            _zoomLabel.style.color = new Color(0.9f, 0.9f, 0.9f);
            _zoomLabel.style.marginLeft = 4f;
            _zoomLabel.style.marginRight = 4f;

            Button zoomInButton = new Button(() => AdjustZoom(ZoomStep))
            {
                text = "+"
            };
            zoomInButton.tooltip = "Zoom in";

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

            Button connectSelectedButton = new Button(BeginConnectSelected)
            {
                text = "Connect Selected"
            };
            connectSelectedButton.style.marginLeft = 8f;

            Button clearLinksSelectedButton = new Button(ClearLinksSelected)
            {
                text = "Clear Links Selected"
            };
            clearLinksSelectedButton.style.marginLeft = 8f;

            Button autoLayoutButton = new Button(AutoLayout)
            {
                text = "Auto Layout"
            };
            autoLayoutButton.style.marginLeft = 8f;

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
            toolbar.Add(gridSnapToggle);
            toolbar.Add(zoomOutButton);
            toolbar.Add(_zoomLabel);
            toolbar.Add(zoomInButton);
            toolbar.Add(addLineButton);
            toolbar.Add(addChoicePromptButton);
            toolbar.Add(addChoiceResponseButton);
            toolbar.Add(connectSelectedButton);
            toolbar.Add(clearLinksSelectedButton);
            toolbar.Add(autoLayoutButton);
            toolbar.Add(duplicateSelectedButton);
            toolbar.Add(duplicateSelectedResetLinksButton);
            toolbar.Add(deleteSelectedButton);
            toolbar.Add(refreshButton);

            return toolbar;
        }

        public Vector2 SnapToGrid(Vector2 position)
        {
            if (!_isGridSnapEnabled)
                return position;

            float x = Mathf.Round(position.x / MinorGridSpacing) * MinorGridSpacing;
            float y = Mathf.Round(position.y / MinorGridSpacing) * MinorGridSpacing;
            return new Vector2(x, y);
        }

        public void SetGraphPan(Vector2 panPosition)
        {
            if (_selectedTable != null)
            {
                _selectedTable.GraphPanPosition = panPosition;
                EditorUtility.SetDirty(_selectedTable);
            }

            ApplyGraphTransform();
        }

        private void AdjustZoom(float delta)
        {
            float currentZoom = _selectedTable != null ? _selectedTable.GraphZoomScale : 1f;
            SetZoom(currentZoom + delta);
        }

        private void SetZoom(float zoom)
        {
            float clampedZoom = Mathf.Clamp(zoom, MinZoom, MaxZoom);

            if (_selectedTable != null)
            {
                _selectedTable.GraphZoomScale = clampedZoom;
                EditorUtility.SetDirty(_selectedTable);
            }

            ApplyGraphTransform();
        }

        private void ApplyGraphTransform()
        {
            if (_graphContentRoot == null)
                return;

            float zoom = _selectedTable != null ? _selectedTable.GraphZoomScale : 1f;
            Vector2 pan = _selectedTable != null ? _selectedTable.GraphPanPosition : Vector2.zero;

            _graphContentRoot.transform.position = new Vector3(pan.x, pan.y, 0f);
            _graphContentRoot.transform.scale = new Vector3(zoom, zoom, 1f);

            if (_zoomLabel != null)
                _zoomLabel.text = $"{Mathf.RoundToInt(zoom * 100f)}%";

            _gridBackground?.MarkDirtyRepaint();
            _graphCanvas?.MarkDirtyRepaint();
        }

        private void OnGraphWheel(WheelEvent evt)
        {
            float direction = evt.delta.y > 0f ? -1f : 1f;
            AdjustZoom(direction * ZoomStep);
            evt.StopPropagation();
        }

        private void RefreshAllViews()
        {
            RefreshValidationState();
            RefreshGraph();
            RefreshInspector();
            RefreshValidation();
            ApplyGraphTransform();
        }

        private void RefreshValidationState()
        {
            if (_selectedTable == null)
            {
                _invalidRowIds = new HashSet<int>();
                _validationMessagesByRowId = new Dictionary<int, List<string>>();
                _isStartNodeInvalid = false;
                return;
            }

            _invalidRowIds = DialogueGraphValidationUtility.GetInvalidRowIds(_selectedTable);
            _validationMessagesByRowId = DialogueGraphValidationUtility.GetValidationMessagesByRowId(_selectedTable);
            _isStartNodeInvalid = DialogueGraphValidationUtility.HasStartNodeIssue(_selectedTable);
        }

        private void RefreshGraph()
        {
            if (_graphCanvas == null)
                return;

            _graphCanvas.Clear();
            _nodeViewsByRowId.Clear();

            if (_selectedTable == null)
            {
                UpdateStatusLabel();
                _graphCanvas.Add(BuildCenteredMessage("Select a DialogueTable to build the dialogue graph view."));
                _graphCanvas.MarkDirtyRepaint();
                return;
            }

            _selectedTable.PruneMissingNodePositions();
            EditorUtility.SetDirty(_selectedTable);

            if (_selectedRowId >= 0 &&
                _selectedRowId != StartNodeRowId &&
                DialogueGraphRowOperations.FindRowIndexById(_selectedTable, _selectedRowId) < 0)
            {
                ClearSelection();
            }

            string startNodeValidationMessage =
                DialogueGraphValidationUtility.GetStartNodeValidationMessage(_selectedTable);

            VisualElement startNode = DialogueGraphNodeViewFactory.CreateStartNode(
                this,
                _selectedTable.StartNodePosition,
                NodeWidth * 0.8f,
                NodeMinHeight * 0.8f,
                _selectedTable.StartRowId,
                _isStartNodeInvalid,
                startNodeValidationMessage);

            _nodeViewsByRowId[StartNodeRowId] = startNode;
            _graphCanvas.Add(startNode);

            if (_selectedTable.RowCount == 0)
            {
                UpdateStatusLabel();
                UpdateNodeSelectionVisuals();
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

            UpdateStatusLabel();
            UpdateNodeSelectionVisuals();
            _graphCanvas.MarkDirtyRepaint();
        }

        private void UpdateStatusLabel()
        {
            if (_tableStatusLabel == null)
                return;

            if (_selectedTable == null)
            {
                _tableStatusLabel.text = "No DialogueTable selected.";
                return;
            }

            string startText = _selectedTable.StartRowId >= 0
                ? $"  •  Start: Row {_selectedTable.StartRowId}"
                : "  •  Start: Unassigned";

            string baseText = $"{_selectedTable.name}  •  Rows: {_selectedTable.RowCount}{startText}";

            if (_isConnectModeActive && _connectSourceRowId >= 0)
            {
                if (_hoveredConnectTargetRowId >= 0)
                {
                    string sourceLabel = _connectSourceRowId == StartNodeRowId
                        ? "Start"
                        : $"Row {_connectSourceRowId}";

                    _tableStatusLabel.text = _hoveredConnectTargetValid
                        ? $"{baseText}  •  Release to connect {sourceLabel} → Row {_hoveredConnectTargetRowId}"
                        : $"{baseText}  •  Invalid target Row {_hoveredConnectTargetRowId} for {sourceLabel}";
                    return;
                }

                if (_connectSourceRowId == StartNodeRowId)
                {
                    _tableStatusLabel.text = $"{baseText}  •  Drag from Start to a row input handle";
                    return;
                }

                _tableStatusLabel.text =
                    $"{baseText}  •  Drag from an output handle to an input handle for Row {_connectSourceRowId}";
                return;
            }

            _tableStatusLabel.text = baseText;
        }

        public void UpdateNodeDialogueText(int rowId, int rowIndex, string newText)
        {
            if (_selectedTable == null || rowId == StartNodeRowId)
                return;

            int resolvedRowIndex = DialogueGraphRowOperations.FindRowIndexById(_selectedTable, rowId);
            if (resolvedRowIndex < 0)
                return;

            DialogueRow row = _selectedTable.GetRow(resolvedRowIndex);
            if (row == null)
                return;

            Undo.RecordObject(_selectedTable, $"Edit Dialogue Row {rowId}");

            if (row.IsChoiceResponseRow)
            {
                row.PlayerChoiceAnswer = newText ?? string.Empty;
            }
            else
            {
                if (row.TextLines == null || row.TextLines.Length == 0)
                    row.TextLines = new[] { newText ?? string.Empty };
                else
                    row.TextLines[0] = newText ?? string.Empty;
            }

            EditorUtility.SetDirty(_selectedTable);
            RefreshValidationState();
            RefreshInspector();
        }

        public void HandleNodeClicked(int rowId, int rowIndex)
        {
            SelectRow(rowId, rowIndex);
        }

        public void BeginPortDragConnection(int rowId, int rowIndex)
        {
            SelectRow(rowId, rowIndex);
            BeginConnectFromRow(rowId);
            _isPortDragActive = true;
            _hoveredConnectTargetRowId = -1;
            _hoveredConnectTargetValid = false;

            if (_nodeViewsByRowId.TryGetValue(rowId, out VisualElement node))
                _portDragPreviewPosition = DialogueGraphNodeViewFactory.GetOutputPortCenter(node);

            MarkGraphDirty();
        }

        public void UpdatePortDragPreview(Vector2 worldPointerPosition)
        {
            if (!_isConnectModeActive || !_isPortDragActive || _graphCanvas == null)
                return;

            _portDragPreviewPosition = _graphCanvas.WorldToLocal(worldPointerPosition);
            UpdateHoveredConnectTarget(worldPointerPosition);
            UpdateNodeSelectionVisuals();
            UpdateStatusLabel();
            MarkGraphDirty();
        }

        public void CompletePortDragConnection(Vector2 worldPointerPosition)
        {
            if (!_isConnectModeActive || !_isPortDragActive || _graphCanvas == null)
            {
                CancelConnectMode();
                return;
            }

            _portDragPreviewPosition = _graphCanvas.WorldToLocal(worldPointerPosition);
            UpdateHoveredConnectTarget(worldPointerPosition);

            if (_hoveredConnectTargetRowId >= 0 && _hoveredConnectTargetValid)
            {
                int targetRowIndex =
                    DialogueGraphRowOperations.FindRowIndexById(_selectedTable, _hoveredConnectTargetRowId);
                CompleteConnectionTo(_hoveredConnectTargetRowId, targetRowIndex);
                return;
            }

            CancelConnectMode();
        }

        public void SelectRow(int rowId, int rowIndex)
        {
            _selectedRowId = rowId;
            _selectedRowIndex = rowIndex;
            UpdateNodeSelectionVisuals();
            RefreshInspector();
            RefreshValidation();
            UpdateStatusLabel();
        }

        public void SelectRowById(int rowId)
        {
            if (_selectedTable == null)
                return;

            if (rowId == StartNodeRowId)
            {
                SelectRow(StartNodeRowId, -1);
                if (_nodeViewsByRowId.TryGetValue(StartNodeRowId, out VisualElement startNode))
                {
                    startNode.BringToFront();
                    FrameNode(startNode);
                }

                return;
            }

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

        private void BeginConnectSelected()
        {
            if (_selectedTable == null)
            {
                EditorUtility.DisplayDialog("Dialogue Graph", "Select a DialogueTable first.", "OK");
                return;
            }

            if (_selectedRowId < 0)
            {
                EditorUtility.DisplayDialog("Dialogue Graph", "Select a source row first.", "OK");
                return;
            }

            if (_selectedRowId == StartNodeRowId)
            {
                EditorUtility.DisplayDialog("Dialogue Graph",
                    "Drag from the Start node output handle to a row input handle.", "OK");
                return;
            }

            EditorUtility.DisplayDialog("Dialogue Graph",
                "Drag from an output handle to an input handle to connect nodes.", "OK");
        }

        private void BeginConnectFromRow(int sourceRowId)
        {
            if (_selectedTable == null || sourceRowId < 0 && sourceRowId != StartNodeRowId)
                return;

            _isConnectModeActive = true;
            _connectSourceRowId = sourceRowId;
            _hoveredConnectTargetRowId = -1;
            _hoveredConnectTargetValid = false;
            UpdateNodeSelectionVisuals();
            UpdateStatusLabel();
            MarkGraphDirty();
        }

        private void CompleteConnectionTo(int targetRowId, int targetRowIndex)
        {
            if (!_isConnectModeActive || _connectSourceRowId < 0 && _connectSourceRowId != StartNodeRowId ||
                _selectedTable == null)
                return;

            int sourceRowId = _connectSourceRowId;
            CancelConnectMode();

            if (sourceRowId == StartNodeRowId)
            {
                Undo.RecordObject(_selectedTable, "Set Dialogue Start Row");
                _selectedTable.StartRowId = targetRowId;
                EditorUtility.SetDirty(_selectedTable);
                RefreshAllViews();
                return;
            }

            bool connected =
                DialogueGraphRowOperations.ConnectRows(_selectedTable, sourceRowId, targetRowId,
                    out string errorMessage);
            if (!connected)
            {
                if (!string.IsNullOrWhiteSpace(errorMessage))
                    EditorUtility.DisplayDialog("Dialogue Graph", errorMessage, "OK");

                RefreshAllViews();
                return;
            }

            SelectRow(targetRowId, targetRowIndex);
            RefreshAllViews();
        }

        public void BeginConnectSelectedFromMenu()
        {
            BeginConnectSelected();
        }

        private void ClearLinksSelected()
        {
            if (_selectedTable == null)
            {
                EditorUtility.DisplayDialog("Dialogue Graph", "Select a DialogueTable first.", "OK");
                return;
            }

            if (_selectedRowId < 0 && _selectedRowId != StartNodeRowId)
            {
                EditorUtility.DisplayDialog("Dialogue Graph", "Select a row first.", "OK");
                return;
            }

            CancelConnectMode();

            if (_selectedRowId == StartNodeRowId)
            {
                Undo.RecordObject(_selectedTable, "Clear Dialogue Start Row");
                _selectedTable.StartRowId = -1;
                EditorUtility.SetDirty(_selectedTable);
                RefreshAllViews();
                return;
            }

            if (!DialogueGraphRowOperations.ClearOutgoingLinks(_selectedTable, _selectedRowId))
                return;

            RefreshAllViews();
        }

        public void ClearLinksSelectedFromMenu()
        {
            ClearLinksSelected();
        }

        public void CancelConnectMode()
        {
            _isConnectModeActive = false;
            _connectSourceRowId = -1;
            _isPortDragActive = false;
            _hoveredConnectTargetRowId = -1;
            _hoveredConnectTargetValid = false;
            UpdateNodeSelectionVisuals();
            UpdateStatusLabel();
            MarkGraphDirty();
        }

        private void FrameNode(VisualElement node)
        {
            if (_graphViewport == null || _graphContentRoot == null || _selectedTable == null || node == null)
                return;

            Rect nodeRect = node.layout;
            if (nodeRect.width <= 0f || nodeRect.height <= 0f)
                return;

            float zoom = _selectedTable.GraphZoomScale;
            Vector2 pan = _selectedTable.GraphPanPosition;

            Rect transformedRect = new Rect(
                pan.x + nodeRect.xMin * zoom,
                pan.y + nodeRect.yMin * zoom,
                nodeRect.width * zoom,
                nodeRect.height * zoom);

            Rect viewportRect = _graphViewport.contentRect;
            if (viewportRect.width <= 0f || viewportRect.height <= 0f)
                return;

            Vector2 updatedPan = pan;

            if (transformedRect.xMin - FramePadding < 0f)
                updatedPan.x += - (transformedRect.xMin - FramePadding);
            else if (transformedRect.xMax + FramePadding > viewportRect.width)
                updatedPan.x -= transformedRect.xMax + FramePadding - viewportRect.width;

            if (transformedRect.yMin - FramePadding < 0f)
                updatedPan.y += - (transformedRect.yMin - FramePadding);
            else if (transformedRect.yMax + FramePadding > viewportRect.height)
                updatedPan.y -= transformedRect.yMax + FramePadding - viewportRect.height;

            SetGraphPan(updatedPan);
        }

        private void FrameAllNodes()
        {
            if (_graphViewport == null || _selectedTable == null || _nodeViewsByRowId.Count == 0)
                return;

            bool hasBounds = false;
            Rect bounds = default;

            foreach (KeyValuePair<int, VisualElement> pair in _nodeViewsByRowId)
            {
                Rect rect = pair.Value.layout;
                if (rect.width <= 0f || rect.height <= 0f)
                    continue;

                if (!hasBounds)
                {
                    bounds = rect;
                    hasBounds = true;
                }
                else
                {
                    bounds.xMin = Mathf.Min(bounds.xMin, rect.xMin);
                    bounds.yMin = Mathf.Min(bounds.yMin, rect.yMin);
                    bounds.xMax = Mathf.Max(bounds.xMax, rect.xMax);
                    bounds.yMax = Mathf.Max(bounds.yMax, rect.yMax);
                }
            }

            if (!hasBounds)
                return;

            float zoom = _selectedTable.GraphZoomScale;
            Vector2 targetPan = new Vector2(
                FramePadding - bounds.xMin * zoom,
                FramePadding - bounds.yMin * zoom);

            SetGraphPan(targetPan);
        }

        private void ClearSelection()
        {
            _selectedRowId = -1;
            _selectedRowIndex = -1;
            UpdateNodeSelectionVisuals();
            UpdateStatusLabel();
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
                int rowId = pair.Key;
                bool isSelected = rowId == _selectedRowId;
                bool isInvalid = rowId == StartNodeRowId
                    ? _isStartNodeInvalid
                    : rowId >= 0 && _invalidRowIds.Contains(rowId);
                bool isConnectSource = _isConnectModeActive && rowId == _connectSourceRowId;
                bool isValidConnectTarget = IsValidConnectTarget(rowId);
                bool isInvalidConnectTarget = _isConnectModeActive &&
                                              !isConnectSource &&
                                              !isValidConnectTarget &&
                                              rowId >= 0;
                bool isHoveredConnectTarget = rowId == _hoveredConnectTargetRowId && _hoveredConnectTargetValid;
                bool isHoveredInvalidTarget = rowId == _hoveredConnectTargetRowId && !_hoveredConnectTargetValid;

                DialogueGraphNodeViewFactory.SetNodeState(
                    pair.Value,
                    isSelected,
                    isInvalid,
                    isConnectSource,
                    isValidConnectTarget,
                    isInvalidConnectTarget,
                    isHoveredConnectTarget,
                    isHoveredInvalidTarget);
            }
        }

        private void UpdateHoveredConnectTarget(Vector2 worldPointerPosition)
        {
            _hoveredConnectTargetRowId = -1;
            _hoveredConnectTargetValid = false;

            if (_selectedTable == null || !_isConnectModeActive)
                return;

            VisualElement pickedElement = _graphCanvas.panel?.Pick(worldPointerPosition) as VisualElement;
            VisualElement inputPort =
                DialogueGraphNodeViewFactory.FindPortElementInHierarchy(pickedElement, inputOnly: true);

            if (inputPort == null)
                return;

            if (!DialogueGraphNodeViewFactory.TryGetRowIdFromPort(inputPort, out int targetRowId))
                return;

            _hoveredConnectTargetRowId = targetRowId;
            _hoveredConnectTargetValid = IsValidConnectTarget(targetRowId);
        }

        private bool IsValidConnectTarget(int targetRowId)
        {
            if (!_isConnectModeActive || _selectedTable == null ||
                _connectSourceRowId < 0 && _connectSourceRowId != StartNodeRowId)
                return false;

            if (targetRowId < 0)
                return false;

            if (_connectSourceRowId == StartNodeRowId)
                return _selectedTable.HasRowId(targetRowId);

            if (targetRowId == _connectSourceRowId)
                return false;

            DialogueRow sourceRow = _selectedTable.GetRowById(_connectSourceRowId);
            DialogueRow targetRow = _selectedTable.GetRowById(targetRowId);

            if (sourceRow == null || targetRow == null)
                return false;

            if (sourceRow.IsChoicePromptRow)
                return targetRow.IsChoiceResponseRow;

            return true;
        }

        private void RefreshInspector()
        {
            _inspectorView?.Refresh(_selectedTable, _selectedRowId, _selectedRowIndex);
        }

        private void RefreshValidation()
        {
            _validationView?.Refresh(_selectedTable);
        }

        private void AutoLayout()
        {
            if (_selectedTable == null)
            {
                EditorUtility.DisplayDialog("Dialogue Graph", "Select a DialogueTable first.", "OK");
                return;
            }

            CancelConnectMode();
            DialogueGraphAutoLayoutUtility.AutoLayout(_selectedTable);
            RefreshAllViews();

            rootVisualElement.schedule.Execute(FrameAllNodes);
        }

        public void AutoLayoutFromMenu()
        {
            AutoLayout();
        }

        private void CreateRow(DialogueRowKind rowKind)
        {
            if (_selectedTable == null)
            {
                EditorUtility.DisplayDialog("Dialogue Graph", "Select a DialogueTable first.", "OK");
                return;
            }

            Vector2 position = DialogueGraphRowOperations.GetNewRowPosition(
                _selectedRowId >= 0 ? _selectedRowId : _selectedTable.StartRowId,
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

            if (_selectedRowId < 0 || _selectedRowId == StartNodeRowId)
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

            if (_selectedRowId < 0 || _selectedRowId == StartNodeRowId)
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

            if (_selectedRowId < 0 || _selectedRowId == StartNodeRowId)
            {
                EditorUtility.DisplayDialog("Dialogue Graph", "Select a row node to delete.", "OK");
                return;
            }

            CancelConnectMode();

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

        public void SaveNodePosition(int rowId, VisualElement node, bool snapToGrid = true)
        {
            if (_selectedTable == null)
                return;

            Vector2 position = new Vector2(node.resolvedStyle.left, node.resolvedStyle.top);

            if (snapToGrid)
                position = SnapToGrid(position);

            node.style.left = position.x;
            node.style.top = position.y;

            if (rowId == StartNodeRowId)
            {
                _selectedTable.StartNodePosition = position;
            }
            else
            {
                _selectedTable.SetNodePosition(rowId, position);
            }

            EditorUtility.SetDirty(_selectedTable);
            MarkGraphDirty();
        }

        public void MarkGraphDirty()
        {
            _graphCanvas?.MarkDirtyRepaint();
        }

        private void BuildGraphCanvasContextMenu(ContextualMenuPopulateEvent evt)
        {
            if (_graphCanvas == null || _selectedTable == null)
            {
                DialogueGraphContextMenus.BuildCanvasMenu(this, evt);
                return;
            }

            Vector2 canvasPosition = evt.localMousePosition;

            if (DialogueGraphEdgeRenderer.TryFindEdgeAtPosition(
                    _selectedTable,
                    _nodeViewsByRowId,
                    canvasPosition,
                    out DialogueGraphEdgeRenderer.EdgeReference edge))
            {
                string label = edge.IsStartEdge
                    ? "Delete Connection"
                    : $"Delete Connection ({edge.SourceRowId} → {edge.TargetRowId})";

                evt.menu.AppendAction(
                    label,
                    _ => DeleteEdge(edge),
                    DropdownMenuAction.Status.Normal);

                return;
            }

            DialogueGraphContextMenus.BuildCanvasMenu(this, evt);
        }

        private void DeleteEdge(DialogueGraphEdgeRenderer.EdgeReference edge)
        {
            if (_selectedTable == null)
                return;

            CancelConnectMode();

            if (edge.IsStartEdge)
            {
                Undo.RecordObject(_selectedTable, "Delete Start Connection");
                _selectedTable.StartRowId = -1;
                EditorUtility.SetDirty(_selectedTable);
                RefreshAllViews();
                return;
            }

            if (!DialogueGraphRowOperations.RemoveConnection(_selectedTable, edge.SourceRowId, edge.TargetRowId))
                return;

            RefreshAllViews();
        }

        private bool DeleteEdgeAtCanvasPosition(Vector2 canvasPosition)
        {
            if (_selectedTable == null)
                return false;

            if (!DialogueGraphEdgeRenderer.TryFindEdgeAtPosition(_selectedTable, _nodeViewsByRowId, canvasPosition,
                    out DialogueGraphEdgeRenderer.EdgeReference edge))
                return false;

            if (edge.IsStartEdge)
            {
                Undo.RecordObject(_selectedTable, "Delete Start Connection");
                _selectedTable.StartRowId = -1;
                EditorUtility.SetDirty(_selectedTable);
                RefreshAllViews();
                return true;
            }

            bool removed =
                DialogueGraphRowOperations.RemoveConnection(_selectedTable, edge.SourceRowId, edge.TargetRowId);
            if (!removed)
                return false;

            RefreshAllViews();
            return true;
        }


        private void OnCanvasPointerDown(PointerDownEvent evt)
        {
            if (evt.button != (int)MouseButton.LeftMouse)
                return;

            if (evt.target == _graphCanvas)
            {
                if (_isConnectModeActive)
                    CancelConnectMode();

                ClearSelection();
                RefreshInspector();
                RefreshValidation();
            }
        }

        private void OnGraphCanvasGenerateVisualContent(MeshGenerationContext context)
        {
            int previewSourceRowId =
                _isPortDragActive && (_connectSourceRowId >= 0 || _connectSourceRowId == StartNodeRowId)
                    ? _connectSourceRowId
                    : -1;

            DialogueGraphEdgeRenderer.Draw(
                _selectedTable,
                _nodeViewsByRowId,
                context,
                previewSourceRowId,
                _isPortDragActive ? _portDragPreviewPosition : Vector2.zero,
                _isPortDragActive && _hoveredConnectTargetRowId >= 0,
                _hoveredConnectTargetValid);
        }

        private void OnGridBackgroundGenerateVisualContent(MeshGenerationContext context)
        {
            DrawGrid(context);
        }

        private void DrawGrid(MeshGenerationContext context)
        {
            if (_graphViewport == null || _selectedTable == null)
                return;

            Rect viewportRect = _graphViewport.contentRect;
            if (viewportRect.width <= 0f || viewportRect.height <= 0f)
                return;

            Painter2D painter = context.painter2D;

            float zoom = _selectedTable.GraphZoomScale;
            Vector2 pan = _selectedTable.GraphPanPosition;

            DrawInfiniteGridLines(painter, viewportRect, MinorGridSpacing, zoom, pan, MinorGridColor, 1f);
            DrawInfiniteGridLines(painter, viewportRect, MajorGridSpacing, zoom, pan, MajorGridColor, 1.25f);
            //DrawViewportCenterCrossLines(painter, viewportRect);
        }

        private static void DrawInfiniteGridLines(
            Painter2D painter,
            Rect viewportRect,
            float worldSpacing,
            float zoom,
            Vector2 pan,
            Color color,
            float lineWidth)
        {
            float screenSpacing = worldSpacing * zoom;
            if (screenSpacing <= 0.01f)
                return;

            painter.strokeColor = color;
            painter.lineWidth = lineWidth;

            float xOffset = Repeat(pan.x, screenSpacing);
            float yOffset = Repeat(pan.y, screenSpacing);

            for (float x = xOffset; x <= viewportRect.width; x += screenSpacing)
            {
                painter.BeginPath();
                painter.MoveTo(new Vector2(x, 0f));
                painter.LineTo(new Vector2(x, viewportRect.height));
                painter.Stroke();
            }

            if (xOffset > 0f)
            {
                for (float x = xOffset - screenSpacing; x >= 0f; x -= screenSpacing)
                {
                    painter.BeginPath();
                    painter.MoveTo(new Vector2(x, 0f));
                    painter.LineTo(new Vector2(x, viewportRect.height));
                    painter.Stroke();
                }
            }

            for (float y = yOffset; y <= viewportRect.height; y += screenSpacing)
            {
                painter.BeginPath();
                painter.MoveTo(new Vector2(0f, y));
                painter.LineTo(new Vector2(viewportRect.width, y));
                painter.Stroke();
            }

            if (yOffset > 0f)
            {
                for (float y = yOffset - screenSpacing; y >= 0f; y -= screenSpacing)
                {
                    painter.BeginPath();
                    painter.MoveTo(new Vector2(0f, y));
                    painter.LineTo(new Vector2(viewportRect.width, y));
                    painter.Stroke();
                }
            }
        }

        private static float Repeat(float value, float length)
        {
            if (length <= 0f)
                return 0f;

            return value - Mathf.Floor(value / length) * length;
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