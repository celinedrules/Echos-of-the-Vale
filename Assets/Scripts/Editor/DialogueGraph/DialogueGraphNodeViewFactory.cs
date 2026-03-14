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

            string previewText = row.IsChoiceResponseRow
                ? row.PlayerChoiceAnswer
                : row.GetFirstLine();

            if (string.IsNullOrWhiteSpace(previewText))
                previewText = "(No preview text)";

            Label previewLabel = new Label(previewText);
            previewLabel.style.whiteSpace = WhiteSpace.Normal;
            previewLabel.style.color = new Color(0.93f, 0.93f, 0.93f);
            previewLabel.style.marginBottom = 8f;

            string leadsToText = row.UsesLeadsTo ? row.LeadsTo.ToString() : "N/A";
            Label metaLabel = new Label($"Leads To: {leadsToText}");
            metaLabel.style.color = new Color(0.8f, 0.8f, 0.8f);
            metaLabel.style.fontSize = 11;

            Label positionLabel = new Label($"({Mathf.RoundToInt(position.x)}, {Mathf.RoundToInt(position.y)})");
            positionLabel.style.color = new Color(0.72f, 0.72f, 0.72f);
            positionLabel.style.fontSize = 10;
            positionLabel.style.marginTop = 6f;

            node.Add(headerRow);
            node.Add(previewLabel);
            node.Add(metaLabel);
            node.Add(positionLabel);

            node.RegisterCallback<ClickEvent>(evt =>
            {
                if (evt.button != (int)MouseButton.LeftMouse)
                    return;

                if (evt.target is VisualElement clickedElement &&
                    IsPortElement(clickedElement))
                {
                    return;
                }

                window.HandleNodeClicked(row.RowId, rowIndex);
                evt.StopPropagation();
            });

            node.AddManipulator(new ContextualMenuManipulator(evt =>
                DialogueGraphContextMenus.BuildNodeMenu(window, evt, row.RowId, rowIndex)));
            node.AddManipulator(new DialogueGraphNodeDragManipulator(node, row.RowId, rowIndex, window, positionLabel));

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
            SetNodeBorderColor(node, isSelected, isInvalid, isConnectSource);
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

        private static void SetNodeBorderColor(VisualElement node, bool isSelected, bool isInvalid, bool isConnectSource)
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