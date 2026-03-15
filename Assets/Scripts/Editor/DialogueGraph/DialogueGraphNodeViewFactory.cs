using System.Collections.Generic;
using System.Text;
using Data.DialogueData;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.DialogueGraph
{
    public static class DialogueGraphNodeViewFactory
    {
        private const string WarningBadgeElementName = "dialogue-graph-warning-badge";
        private const string InputPortElementName = "dialogue-graph-input-port";
        private const string OutputPortElementName = "dialogue-graph-output-port";
        private const string SpeakerBodyElementName = "dialogue-graph-speaker-body";
        private const float SpeakerPortInsetWidth = 8f;

        public static VisualElement CreateStartNode(
            DialogueGraphWindow window,
            Vector2 position,
            float nodeWidth,
            float nodeMinHeight,
            int connectedRowId,
            bool isInvalid,
            string validationMessage)
        {
            VisualElement node = new VisualElement();
            node.style.position = Position.Absolute;
            node.style.left = position.x;
            node.style.top = position.y;
            node.style.width = nodeWidth;
            node.style.minHeight = nodeMinHeight * 0.8f;
            node.style.paddingLeft = 10f;
            node.style.paddingRight = 10f;
            node.style.paddingTop = 10f;
            node.style.paddingBottom = 10f;
            node.style.backgroundColor = new Color(0.18f, 0.34f, 0.20f);
            node.style.borderTopWidth = 2f;
            node.style.borderBottomWidth = 2f;
            node.style.borderLeftWidth = 2f;
            node.style.borderRightWidth = 2f;
            node.style.borderTopColor = new Color(0.45f, 0.9f, 0.5f);
            node.style.borderBottomColor = new Color(0.45f, 0.9f, 0.5f);
            node.style.borderLeftColor = new Color(0.45f, 0.9f, 0.5f);
            node.style.borderRightColor = new Color(0.45f, 0.9f, 0.5f);
            node.style.borderTopLeftRadius = 8f;
            node.style.borderTopRightRadius = 8f;
            node.style.borderBottomLeftRadius = 8f;
            node.style.borderBottomRightRadius = 8f;

            node.tooltip = string.IsNullOrWhiteSpace(validationMessage) ? null : validationMessage;

            VisualElement headerRow = new VisualElement();
            headerRow.style.flexDirection = FlexDirection.Row;
            headerRow.style.alignItems = Align.Center;
            headerRow.style.justifyContent = Justify.SpaceBetween;
            headerRow.style.marginBottom = 6f;

            Label titleLabel = new Label("Start");
            titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            titleLabel.style.fontSize = 14;
            titleLabel.style.color = Color.white;
            titleLabel.style.flexGrow = 1;
            titleLabel.style.marginRight = 6f;

            Label warningBadge = new Label("⚠");
            warningBadge.name = WarningBadgeElementName;
            warningBadge.tooltip = "This node has validation issues.";
            warningBadge.style.color = new Color(1f, 0.72f, 0.22f);
            warningBadge.style.unityFontStyleAndWeight = FontStyle.Bold;
            warningBadge.style.fontSize = 14f;
            warningBadge.style.unityTextAlign = TextAnchor.MiddleCenter;
            warningBadge.style.minWidth = 18f;
            warningBadge.style.display = isInvalid ? DisplayStyle.Flex : DisplayStyle.None;

            headerRow.Add(titleLabel);
            headerRow.Add(warningBadge);

            string startText = connectedRowId >= 0
                ? $"Default entry → Row {connectedRowId}"
                : "Drag from this node to choose the default entry row.";

            Label infoLabel = new Label(startText);
            infoLabel.style.whiteSpace = WhiteSpace.Normal;
            infoLabel.style.color = new Color(0.92f, 0.96f, 0.92f);
            infoLabel.style.fontSize = 11;

            VisualElement outputPort = CreatePort(
                OutputPortElementName,
                new Color(0.45f, 1f, 0.55f),
                left: StyleKeyword.Auto,
                right: -8f);

            outputPort.tooltip = "Start output";
            outputPort.userData = DialogueGraphWindow.StartNodeRowId;
            outputPort.AddManipulator(new DialogueGraphPortConnectManipulator(
                outputPort,
                DialogueGraphWindow.StartNodeRowId,
                -1,
                window));

            node.Add(outputPort);
            node.Add(headerRow);
            node.Add(infoLabel);

            node.RegisterCallback<ClickEvent>(evt =>
            {
                if (evt.button != (int)MouseButton.LeftMouse)
                    return;

                if (evt.target is VisualElement clickedElement &&
                    (IsPortElement(clickedElement) || IsTextInputElement(clickedElement)))
                {
                    return;
                }

                window.HandleNodeClicked(DialogueGraphWindow.StartNodeRowId, -1);
                evt.StopPropagation();
            });

            node.AddManipulator(new ContextualMenuManipulator(evt =>
                DialogueGraphContextMenus.BuildStartNodeMenu(window, evt)));

            node.AddManipulator(new DialogueGraphNodeDragManipulator(
                node,
                DialogueGraphWindow.StartNodeRowId,
                -1,
                window));

            return node;
        }

        public static VisualElement CreateSpeakerNode(
            DialogueGraphWindow window,
            DialogueBlackboardSpeakerNodeData speakerNode,
            int rowIndex)
        {
            VisualElement node = new VisualElement();
            node.style.position = Position.Absolute;
            node.style.left = speakerNode.Position.x;
            node.style.top = speakerNode.Position.y;
            node.style.width = DialogueGraphBlackboardItemFactory.ItemWidth + SpeakerPortInsetWidth;
            node.style.minWidth = DialogueGraphBlackboardItemFactory.ItemWidth + SpeakerPortInsetWidth;
            node.style.maxWidth = DialogueGraphBlackboardItemFactory.ItemWidth + SpeakerPortInsetWidth;
            node.style.height = DialogueGraphBlackboardItemFactory.ItemHeight;
            node.style.minHeight = DialogueGraphBlackboardItemFactory.ItemHeight;
            node.style.maxHeight = DialogueGraphBlackboardItemFactory.ItemHeight;
            node.style.backgroundColor = Color.clear;
            node.style.overflow = Overflow.Visible;

            VisualElement body = DialogueGraphBlackboardItemFactory.CreateLabelItem(
                speakerNode.Speaker != null ? speakerNode.Speaker.SpeakerName : "(Missing Speaker)",
                new Color(0.86f, 0.60f, 0.16f),
                clipContents: true);

            body.name = SpeakerBodyElementName;
            body.style.position = Position.Absolute;
            body.style.left = 0f;
            body.style.top = 0f;
            body.style.marginBottom = 0f;

            Label nameLabel = body.Q<Label>();
            if (nameLabel != null)
                nameLabel.style.paddingRight = 18f;

            VisualElement outputPort = CreatePort(
                OutputPortElementName,
                new Color(1f, 0.78f, 0.35f),
                left: StyleKeyword.Auto,
                right: 0f);

            outputPort.style.top = (DialogueGraphBlackboardItemFactory.ItemHeight - 16f) * 0.5f;
            outputPort.tooltip = "Speaker output";
            outputPort.userData = speakerNode.NodeId;
            outputPort.AddManipulator(new DialogueGraphPortConnectManipulator(
                outputPort,
                speakerNode.NodeId,
                rowIndex,
                window));

            node.Add(body);
            node.Add(outputPort);

            node.RegisterCallback<ClickEvent>(evt =>
            {
                if (evt.button != (int)MouseButton.LeftMouse)
                    return;

                if (evt.target is VisualElement clickedElement &&
                    (IsPortElement(clickedElement) || IsTextInputElement(clickedElement)))
                {
                    return;
                }

                window.HandleNodeClicked(speakerNode.NodeId, rowIndex);
                evt.StopPropagation();
            });

            node.AddManipulator(new DialogueGraphNodeDragManipulator(
                node,
                speakerNode.NodeId,
                rowIndex,
                window));

            return node;
        }

        public static VisualElement CreateNode(
            DialogueGraphWindow window,
            DialogueRow row,
            Vector2 position,
            int rowIndex,
            float nodeWidth,
            float nodeMinHeight,
            IReadOnlyList<string> validationMessages)
        {
            VisualElement node = new VisualElement();
            node.style.position = Position.Absolute;
            node.style.left = position.x;
            node.style.top = position.y;
            node.style.width = nodeWidth;
            node.style.minHeight = nodeMinHeight;
            node.style.paddingLeft = 10f;
            node.style.paddingRight = 10f;
            node.style.paddingTop = 8f;
            node.style.paddingBottom = 10f;
            node.style.backgroundColor = GetNodeColor(row);
            node.style.borderTopWidth = 1f;
            node.style.borderBottomWidth = 1f;
            node.style.borderLeftWidth = 1f;
            node.style.borderRightWidth = 1f;
            node.style.borderTopLeftRadius = 8f;
            node.style.borderTopRightRadius = 8f;
            node.style.borderBottomLeftRadius = 8f;
            node.style.borderBottomRightRadius = 8f;

            SetNodeBorderColor(node, false, false, false);
            SetNodeTooltip(node, validationMessages);

            VisualElement inputPort = CreatePort(
                InputPortElementName,
                new Color(0.64f, 0.78f, 1f),
                left: -8f,
                right: StyleKeyword.Auto);

            inputPort.tooltip = "Input port";
            inputPort.userData = row.RowId;

            VisualElement outputPort = CreatePort(
                OutputPortElementName,
                new Color(1f, 0.78f, 0.35f),
                left: StyleKeyword.Auto,
                right: -8f);

            outputPort.tooltip = "Output port";
            outputPort.userData = row.RowId;
            outputPort.AddManipulator(new DialogueGraphPortConnectManipulator(outputPort, row.RowId, rowIndex, window));

            node.Add(inputPort);
            node.Add(outputPort);

            VisualElement headerRow = new VisualElement();
            headerRow.style.flexDirection = FlexDirection.Row;
            headerRow.style.alignItems = Align.Center;
            headerRow.style.justifyContent = Justify.SpaceBetween;
            headerRow.style.marginBottom = 6f;

            Label titleLabel = new Label($"Row {row.RowId} • {row.RowKind}");
            titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            titleLabel.style.fontSize = 13;
            titleLabel.style.color = Color.white;
            titleLabel.style.flexGrow = 1;
            titleLabel.style.marginRight = 6f;

            Label warningBadge = new Label("⚠");
            warningBadge.name = WarningBadgeElementName;
            warningBadge.tooltip = "This node has validation issues.";
            warningBadge.style.color = new Color(1f, 0.72f, 0.22f);
            warningBadge.style.unityFontStyleAndWeight = FontStyle.Bold;
            warningBadge.style.fontSize = 14f;
            warningBadge.style.unityTextAlign = TextAnchor.MiddleCenter;
            warningBadge.style.minWidth = 18f;
            warningBadge.style.display = DisplayStyle.None;

            headerRow.Add(titleLabel);
            headerRow.Add(warningBadge);

            Label dialogueLabel = new Label("Dialogue");
            dialogueLabel.style.color = new Color(0.85f, 0.85f, 0.85f);
            dialogueLabel.style.fontSize = 11;
            dialogueLabel.style.marginBottom = 4f;

            string editableText = row.IsChoiceResponseRow
                ? row.PlayerChoiceAnswer
                : row.GetFirstLine();

            TextField dialogueField = new TextField
            {
                value = editableText ?? string.Empty,
                multiline = true
            };

            dialogueField.style.whiteSpace = WhiteSpace.Normal;
            dialogueField.style.marginBottom = 8f;
            dialogueField.style.minHeight = 56f;
            dialogueField.style.backgroundColor = new Color(0.16f, 0.16f, 0.16f);
            dialogueField.style.color = new Color(0.94f, 0.94f, 0.94f);

            dialogueField.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue == evt.previousValue)
                    return;

                window.UpdateNodeDialogueText(row.RowId, rowIndex, evt.newValue);
            });

            node.Add(headerRow);
            node.Add(dialogueLabel);
            node.Add(dialogueField);

            node.RegisterCallback<ClickEvent>(evt =>
            {
                if (evt.button != (int)MouseButton.LeftMouse)
                    return;

                if (evt.target is VisualElement clickedElement &&
                    (IsPortElement(clickedElement) || IsTextInputElement(clickedElement)))
                {
                    return;
                }

                window.HandleNodeClicked(row.RowId, rowIndex);
                evt.StopPropagation();
            });

            node.AddManipulator(new ContextualMenuManipulator(evt =>
                DialogueGraphContextMenus.BuildNodeMenu(window, evt, row.RowId, rowIndex)));
            node.AddManipulator(new DialogueGraphNodeDragManipulator(node, row.RowId, rowIndex, window));

            return node;
        }

        public static void SetNodeState(
            VisualElement node,
            bool isSelected,
            bool isInvalid,
            bool isConnectSource,
            bool isValidConnectTarget,
            bool isInvalidConnectTarget,
            bool isHoveredConnectTarget,
            bool isHoveredInvalidTarget)
        {
            VisualElement borderTarget = node.Q<VisualElement>(SpeakerBodyElementName) ?? node;

            SetNodeBorderColor(borderTarget, isSelected, isInvalid, isConnectSource);
            SetWarningBadgeVisibility(node, isInvalid);
            SetPortState(
                node,
                isSelected,
                isConnectSource,
                isValidConnectTarget,
                isInvalidConnectTarget,
                isHoveredConnectTarget,
                isHoveredInvalidTarget);
        }

        public static Vector2 GetInputPortCenter(VisualElement node)
        {
            return GetPortCenter(node, InputPortElementName, useLeftSideFallback: true);
        }

        public static Vector2 GetOutputPortCenter(VisualElement node)
        {
            return GetPortCenter(node, OutputPortElementName, useLeftSideFallback: false);
        }

        public static bool IsPortElement(VisualElement element)
        {
            if (element == null)
                return false;

            return element.name == InputPortElementName || element.name == OutputPortElementName;
        }

        public static bool IsInputPortElement(VisualElement element)
        {
            return element != null && element.name == InputPortElementName;
        }

        public static bool IsTextInputElement(VisualElement element)
        {
            VisualElement current = element;

            while (current != null)
            {
                if (current is TextField)
                    return true;

                if (current.GetType().Name.Contains("TextInput"))
                    return true;

                current = current.parent;
            }

            return false;
        }

        public static bool TryGetRowIdFromPort(VisualElement element, out int rowId)
        {
            rowId = -1;

            if (element == null || !IsPortElement(element))
                return false;

            if (element.userData is int portRowId)
            {
                rowId = portRowId;
                return true;
            }

            return false;
        }

        public static VisualElement FindPortElementInHierarchy(VisualElement element, bool inputOnly)
        {
            VisualElement current = element;

            while (current != null)
            {
                if (inputOnly)
                {
                    if (IsInputPortElement(current))
                        return current;
                }
                else if (IsPortElement(current))
                {
                    return current;
                }

                current = current.parent;
            }

            return null;
        }

        private static VisualElement CreatePort(string name, Color color, StyleLength left, StyleLength right)
        {
            VisualElement port = new VisualElement();
            port.name = name;
            port.pickingMode = PickingMode.Position;
            port.style.position = Position.Absolute;
            port.style.top = 22f;
            port.style.left = left;
            port.style.right = right;
            port.style.width = 16f;
            port.style.height = 16f;
            port.style.backgroundColor = color;
            port.style.borderTopLeftRadius = 8f;
            port.style.borderTopRightRadius = 8f;
            port.style.borderBottomLeftRadius = 8f;
            port.style.borderBottomRightRadius = 8f;
            port.style.borderTopWidth = 2f;
            port.style.borderBottomWidth = 2f;
            port.style.borderLeftWidth = 2f;
            port.style.borderRightWidth = 2f;
            port.style.borderTopColor = new Color(0.08f, 0.08f, 0.08f);
            port.style.borderBottomColor = new Color(0.08f, 0.08f, 0.08f);
            port.style.borderLeftColor = new Color(0.08f, 0.08f, 0.08f);
            port.style.borderRightColor = new Color(0.08f, 0.08f, 0.08f);
            return port;
        }

        private static Vector2 GetPortCenter(VisualElement node, string portElementName, bool useLeftSideFallback)
        {
            VisualElement port = node.Q<VisualElement>(portElementName);
            Rect nodeRect = node.layout;

            if (port != null && port.layout.width > 0f && port.layout.height > 0f)
            {
                return new Vector2(
                    nodeRect.xMin + port.layout.center.x,
                    nodeRect.yMin + port.layout.center.y);
            }

            return useLeftSideFallback
                ? new Vector2(nodeRect.xMin, nodeRect.center.y)
                : new Vector2(nodeRect.xMax, nodeRect.center.y);
        }

        private static void SetPortState(
            VisualElement node,
            bool isSelected,
            bool isConnectSource,
            bool isValidConnectTarget,
            bool isInvalidConnectTarget,
            bool isHoveredConnectTarget,
            bool isHoveredInvalidTarget)
        {
            VisualElement inputPort = node.Q<VisualElement>(InputPortElementName);
            VisualElement outputPort = node.Q<VisualElement>(OutputPortElementName);

            Color defaultBorder = new Color(0.08f, 0.08f, 0.08f);
            Color selectedBorder = new Color(1f, 0.95f, 0.5f);
            Color validTargetBorder = new Color(0.62f, 1f, 0.74f);
            Color invalidTargetBorder = new Color(0.92f, 0.42f, 0.42f);
            Color hoveredValidBorder = new Color(1f, 0.95f, 0.5f);
            Color hoveredInvalidBorder = new Color(1f, 0.35f, 0.35f);

            Color defaultInputFill = new Color(0.64f, 0.78f, 1f);
            Color validInputFill = new Color(0.45f, 0.9f, 0.62f);
            Color invalidInputFill = new Color(0.78f, 0.38f, 0.38f);
            Color hoveredValidInputFill = new Color(1f, 0.86f, 0.38f);
            Color hoveredInvalidInputFill = new Color(0.95f, 0.32f, 0.32f);
            Color defaultOutputFill = new Color(1f, 0.78f, 0.35f);

            float inputBorderWidth = 2f;
            Color inputBorderColor = defaultBorder;
            Color inputFillColor = defaultInputFill;

            if (isHoveredInvalidTarget)
            {
                inputBorderWidth = 3f;
                inputBorderColor = hoveredInvalidBorder;
                inputFillColor = hoveredInvalidInputFill;
            }
            else if (isHoveredConnectTarget)
            {
                inputBorderWidth = 3f;
                inputBorderColor = hoveredValidBorder;
                inputFillColor = hoveredValidInputFill;
            }
            else if (isValidConnectTarget)
            {
                inputBorderWidth = 3f;
                inputBorderColor = validTargetBorder;
                inputFillColor = validInputFill;
            }
            else if (isInvalidConnectTarget)
            {
                inputBorderWidth = 3f;
                inputBorderColor = invalidTargetBorder;
                inputFillColor = invalidInputFill;
            }
            else if (isSelected)
            {
                inputBorderWidth = 3f;
                inputBorderColor = selectedBorder;
            }

            SetPortBorder(inputPort, inputBorderWidth, inputBorderColor);
            if (inputPort != null)
                inputPort.style.backgroundColor = inputFillColor;

            SetPortBorder(outputPort, isConnectSource ? 3f : 2f, isConnectSource ? selectedBorder : defaultBorder);
            if (outputPort != null)
                outputPort.style.backgroundColor = defaultOutputFill;
        }

        private static void SetPortBorder(VisualElement port, float borderWidth, Color borderColor)
        {
            if (port == null)
                return;

            port.style.borderTopWidth = borderWidth;
            port.style.borderBottomWidth = borderWidth;
            port.style.borderLeftWidth = borderWidth;
            port.style.borderRightWidth = borderWidth;
            port.style.borderTopColor = borderColor;
            port.style.borderBottomColor = borderColor;
            port.style.borderLeftColor = borderColor;
            port.style.borderRightColor = borderColor;
        }

        private static void SetNodeTooltip(VisualElement node, IReadOnlyList<string> validationMessages)
        {
            if (validationMessages == null || validationMessages.Count == 0)
            {
                node.tooltip = null;
                return;
            }

            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < validationMessages.Count; i++)
            {
                if (i > 0)
                    builder.Append('\n');

                builder.Append("• ");
                builder.Append(validationMessages[i]);
            }

            node.tooltip = builder.ToString();
        }

        private static void SetWarningBadgeVisibility(VisualElement node, bool isInvalid)
        {
            VisualElement warningBadge = node.Q<VisualElement>(WarningBadgeElementName);
            if (warningBadge == null)
                return;

            warningBadge.style.display = isInvalid ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private static void SetNodeBorderColor(VisualElement node, bool isSelected, bool isInvalid,
            bool isConnectSource)
        {
            Color borderColor;
            float borderWidth;

            if (isConnectSource)
            {
                borderColor = new Color(1f, 0.9f, 0.35f);
                borderWidth = 3f;
            }
            else if (isInvalid && isSelected)
            {
                borderColor = new Color(1f, 0.45f, 0.2f);
                borderWidth = 3f;
            }
            else if (isInvalid)
            {
                borderColor = new Color(0.9f, 0.25f, 0.25f);
                borderWidth = 2f;
            }
            else if (isSelected)
            {
                borderColor = new Color(0.98f, 0.82f, 0.28f);
                borderWidth = 2f;
            }
            else
            {
                borderColor = new Color(0.08f, 0.08f, 0.08f);
                borderWidth = 1f;
            }

            node.style.borderTopWidth = borderWidth;
            node.style.borderBottomWidth = borderWidth;
            node.style.borderLeftWidth = borderWidth;
            node.style.borderRightWidth = borderWidth;

            node.style.borderTopColor = borderColor;
            node.style.borderBottomColor = borderColor;
            node.style.borderLeftColor = borderColor;
            node.style.borderRightColor = borderColor;
        }

        private static Color GetNodeColor(DialogueRow row)
        {
            if (row.IsChoicePromptRow)
                return new Color(0.27f, 0.22f, 0.16f);

            if (row.IsChoiceResponseRow)
                return new Color(0.19f, 0.24f, 0.18f);

            return new Color(0.20f, 0.20f, 0.24f);
        }
    }
}