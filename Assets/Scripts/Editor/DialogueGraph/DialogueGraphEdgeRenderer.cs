using System.Collections.Generic;
using Data.DialogueData;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.DialogueGraph
{
    public static class DialogueGraphEdgeRenderer
    {
        public readonly struct EdgeReference
        {
            public readonly int SourceRowId;
            public readonly int TargetRowId;
            public readonly bool IsStartEdge;

            public EdgeReference(int sourceRowId, int targetRowId, bool isStartEdge)
            {
                SourceRowId = sourceRowId;
                TargetRowId = targetRowId;
                IsStartEdge = isStartEdge;
            }
        }

        private static readonly Color StartEdgeColor = new(0.45f, 1f, 0.55f);
        private static readonly Color LeadsToEdgeColor = new(0.58f, 0.79f, 1f);
        private static readonly Color ChoiceEdgeColor = new(0.95f, 0.74f, 0.32f);
        private static readonly Color InvalidEdgeColor = new(0.95f, 0.28f, 0.28f);
        private static readonly Color PreviewEdgeColor = new(1f, 0.92f, 0.45f);
        private static readonly Color PreviewValidEdgeColor = new(0.45f, 0.9f, 0.62f);
        private static readonly Color PreviewInvalidEdgeColor = new(0.95f, 0.32f, 0.32f);

        private const float MissingEdgeLength = 120f;
        private const int BezierSegments = 24;
        private const float EdgeHitDistance = 10f;

        public static void Draw(
            DialogueTable table,
            IReadOnlyDictionary<int, VisualElement> nodeViewsByRowId,
            MeshGenerationContext context,
            int previewSourceRowId,
            Vector2 previewEnd,
            bool isPreviewHoveringTarget,
            bool isPreviewTargetValid)
        {
            if (nodeViewsByRowId == null || nodeViewsByRowId.Count == 0)
                return;

            Painter2D painter = context.painter2D;
            painter.lineWidth = 3f;

            if (table != null)
            {
                DrawStartEdge(table, nodeViewsByRowId, painter);

                for (int i = 0; i < table.RowCount; i++)
                {
                    DialogueRow sourceRow = table.GetRow(i);
                    if (sourceRow == null)
                        continue;

                    if (sourceRow.IsChoicePromptRow)
                    {
                        int[] choiceRowIds = sourceRow.ChoiceRowIds;
                        if (choiceRowIds == null)
                            continue;

                        for (int j = 0; j < choiceRowIds.Length; j++)
                        {
                            int targetRowId = choiceRowIds[j];
                            bool isValidChoiceTarget = IsValidChoiceTarget(table, sourceRow.RowId, targetRowId);

                            DrawEdge(
                                painter,
                                nodeViewsByRowId,
                                sourceRow.RowId,
                                targetRowId,
                                isValidChoiceTarget ? ChoiceEdgeColor : InvalidEdgeColor,
                                drawDanglingIfMissing: !isValidChoiceTarget);
                        }
                    }
                    else if (sourceRow.UsesLeadsTo && sourceRow.LeadsTo >= 0)
                    {
                        bool isValidLeadsToTarget = IsValidLeadsToTarget(table, sourceRow.RowId, sourceRow.LeadsTo);

                        DrawEdge(
                            painter,
                            nodeViewsByRowId,
                            sourceRow.RowId,
                            sourceRow.LeadsTo,
                            isValidLeadsToTarget ? LeadsToEdgeColor : InvalidEdgeColor,
                            drawDanglingIfMissing: !isValidLeadsToTarget);
                    }
                }
            }

            if (previewSourceRowId != -1 &&
                nodeViewsByRowId.TryGetValue(previewSourceRowId, out VisualElement previewSourceNode))
            {
                Vector2 start = DialogueGraphNodeViewFactory.GetOutputPortCenter(previewSourceNode);

                Color previewColor = PreviewEdgeColor;
                if (isPreviewHoveringTarget)
                    previewColor = isPreviewTargetValid ? PreviewValidEdgeColor : PreviewInvalidEdgeColor;

                DrawPreviewEdge(painter, start, previewEnd, previewColor);
            }
        }

        public static bool TryFindEdgeAtPosition(
            DialogueTable table,
            IReadOnlyDictionary<int, VisualElement> nodeViewsByRowId,
            Vector2 canvasPosition,
            out EdgeReference edgeReference)
        {
            edgeReference = default;

            if (table == null || nodeViewsByRowId == null || nodeViewsByRowId.Count == 0)
                return false;

            if (table.StartRowId >= 0 &&
                TryGetEdgePoints(nodeViewsByRowId, DialogueGraphWindow.StartNodeRowId, table.StartRowId, out Vector2 startStart, out Vector2 startEnd) &&
                IsPointNearBezier(canvasPosition, startStart, startEnd))
            {
                edgeReference = new EdgeReference(DialogueGraphWindow.StartNodeRowId, table.StartRowId, true);
                return true;
            }

            for (int i = 0; i < table.RowCount; i++)
            {
                DialogueRow sourceRow = table.GetRow(i);
                if (sourceRow == null)
                    continue;

                if (sourceRow.IsChoicePromptRow)
                {
                    int[] choiceRowIds = sourceRow.ChoiceRowIds;
                    if (choiceRowIds == null)
                        continue;

                    for (int j = 0; j < choiceRowIds.Length; j++)
                    {
                        int targetRowId = choiceRowIds[j];
                        if (!TryGetEdgePoints(nodeViewsByRowId, sourceRow.RowId, targetRowId, out Vector2 start, out Vector2 end))
                            continue;

                        if (!IsPointNearBezier(canvasPosition, start, end))
                            continue;

                        edgeReference = new EdgeReference(sourceRow.RowId, targetRowId, false);
                        return true;
                    }
                }
                else if (sourceRow.UsesLeadsTo && sourceRow.LeadsTo >= 0)
                {
                    if (!TryGetEdgePoints(nodeViewsByRowId, sourceRow.RowId, sourceRow.LeadsTo, out Vector2 start, out Vector2 end))
                        continue;

                    if (!IsPointNearBezier(canvasPosition, start, end))
                        continue;

                    edgeReference = new EdgeReference(sourceRow.RowId, sourceRow.LeadsTo, false);
                    return true;
                }
            }

            return false;
        }

        private static void DrawStartEdge(
            DialogueTable table,
            IReadOnlyDictionary<int, VisualElement> nodeViewsByRowId,
            Painter2D painter)
        {
            if (table.StartRowId < 0)
                return;

            if (!nodeViewsByRowId.ContainsKey(DialogueGraphWindow.StartNodeRowId))
                return;

            DrawEdge(
                painter,
                nodeViewsByRowId,
                DialogueGraphWindow.StartNodeRowId,
                table.StartRowId,
                table.HasRowId(table.StartRowId) ? StartEdgeColor : InvalidEdgeColor,
                drawDanglingIfMissing: !table.HasRowId(table.StartRowId));
        }

        private static bool IsValidLeadsToTarget(DialogueTable table, int sourceRowId, int targetRowId)
        {
            if (targetRowId < 0)
                return false;

            if (sourceRowId == targetRowId)
                return false;

            return table.GetRowById(targetRowId) != null;
        }

        private static bool IsValidChoiceTarget(DialogueTable table, int sourceRowId, int targetRowId)
        {
            if (targetRowId < 0)
                return false;

            if (sourceRowId == targetRowId)
                return false;

            DialogueRow targetRow = table.GetRowById(targetRowId);
            if (targetRow == null)
                return false;

            return targetRow.IsChoiceResponseRow;
        }

        private static void DrawEdge(
            Painter2D painter,
            IReadOnlyDictionary<int, VisualElement> nodeViewsByRowId,
            int sourceRowId,
            int targetRowId,
            Color color,
            bool drawDanglingIfMissing)
        {
            if (!nodeViewsByRowId.TryGetValue(sourceRowId, out VisualElement sourceNode))
                return;

            Rect sourceRect = sourceNode.layout;
            if (sourceRect.width <= 0f || sourceRect.height <= 0f)
                return;

            Vector2 start = DialogueGraphNodeViewFactory.GetOutputPortCenter(sourceNode);

            if (!nodeViewsByRowId.TryGetValue(targetRowId, out VisualElement targetNode))
            {
                if (drawDanglingIfMissing)
                    DrawDanglingEdge(painter, start, color);

                return;
            }

            Rect targetRect = targetNode.layout;
            if (targetRect.width <= 0f || targetRect.height <= 0f)
                return;

            Vector2 end = DialogueGraphNodeViewFactory.GetInputPortCenter(targetNode);

            DrawBezierEdge(painter, start, end, color);
            DrawArrowHead(painter, end, color);
        }

        private static void DrawPreviewEdge(Painter2D painter, Vector2 start, Vector2 end, Color color)
        {
            DrawBezierEdge(painter, start, end, color);
        }

        private static void DrawBezierEdge(Painter2D painter, Vector2 start, Vector2 end, Color color)
        {
            GetBezierTangents(start, end, out Vector2 startTangent, out Vector2 endTangent);

            painter.strokeColor = color;
            painter.BeginPath();
            painter.MoveTo(start);
            painter.BezierCurveTo(startTangent, endTangent, end);
            painter.Stroke();
        }

        private static void DrawDanglingEdge(Painter2D painter, Vector2 start, Color color)
        {
            Vector2 end = start + Vector2.right * MissingEdgeLength;

            float tangentOffset = MissingEdgeLength * 0.5f;
            Vector2 startTangent = start + Vector2.right * tangentOffset;
            Vector2 endTangent = end + Vector2.left * tangentOffset;

            painter.strokeColor = color;
            painter.BeginPath();
            painter.MoveTo(start);
            painter.BezierCurveTo(startTangent, endTangent, end);
            painter.Stroke();

            DrawArrowHead(painter, end, color);
            DrawMissingTargetMarker(painter, end, color);
        }

        private static bool TryGetEdgePoints(
            IReadOnlyDictionary<int, VisualElement> nodeViewsByRowId,
            int sourceRowId,
            int targetRowId,
            out Vector2 start,
            out Vector2 end)
        {
            start = default;
            end = default;

            if (!nodeViewsByRowId.TryGetValue(sourceRowId, out VisualElement sourceNode))
                return false;

            if (!nodeViewsByRowId.TryGetValue(targetRowId, out VisualElement targetNode))
                return false;

            Rect sourceRect = sourceNode.layout;
            Rect targetRect = targetNode.layout;

            if (sourceRect.width <= 0f || sourceRect.height <= 0f || targetRect.width <= 0f || targetRect.height <= 0f)
                return false;

            start = DialogueGraphNodeViewFactory.GetOutputPortCenter(sourceNode);
            end = DialogueGraphNodeViewFactory.GetInputPortCenter(targetNode);
            return true;
        }

        private static bool IsPointNearBezier(Vector2 point, Vector2 start, Vector2 end)
        {
            GetBezierTangents(start, end, out Vector2 startTangent, out Vector2 endTangent);

            Vector2 previous = start;

            for (int i = 1; i <= BezierSegments; i++)
            {
                float t = i / (float)BezierSegments;
                Vector2 current = EvaluateBezier(start, startTangent, endTangent, end, t);

                if (DistanceToSegment(point, previous, current) <= EdgeHitDistance)
                    return true;

                previous = current;
            }

            return false;
        }

        private static void GetBezierTangents(Vector2 start, Vector2 end, out Vector2 startTangent, out Vector2 endTangent)
        {
            float tangentOffset = Mathf.Max(60f, Mathf.Abs(end.x - start.x) * 0.35f);
            startTangent = start + Vector2.right * tangentOffset;
            endTangent = end + Vector2.left * tangentOffset;
        }

        private static Vector2 EvaluateBezier(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
        {
            float u = 1f - t;
            float tt = t * t;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * t;

            return uuu * p0 +
                   3f * uu * t * p1 +
                   3f * u * tt * p2 +
                   ttt * p3;
        }

        private static float DistanceToSegment(Vector2 point, Vector2 a, Vector2 b)
        {
            Vector2 ab = b - a;
            float lengthSq = ab.sqrMagnitude;

            if (lengthSq <= Mathf.Epsilon)
                return Vector2.Distance(point, a);

            float t = Mathf.Clamp01(Vector2.Dot(point - a, ab) / lengthSq);
            Vector2 projection = a + ab * t;
            return Vector2.Distance(point, projection);
        }

        private static void DrawMissingTargetMarker(Painter2D painter, Vector2 center, Color color)
        {
            const float radius = 6f;

            painter.strokeColor = color;
            painter.lineWidth = 2f;
            painter.BeginPath();
            painter.Arc(center, radius, 0f, 360f);
            painter.Stroke();
        }

        private static void DrawArrowHead(Painter2D painter, Vector2 tip, Color color)
        {
            const float arrowLength = 10f;
            const float arrowHalfHeight = 5f;

            Vector2 upper = new Vector2(tip.x - arrowLength, tip.y - arrowHalfHeight);
            Vector2 lower = new Vector2(tip.x - arrowLength, tip.y + arrowHalfHeight);

            painter.fillColor = color;
            painter.BeginPath();
            painter.MoveTo(tip);
            painter.LineTo(upper);
            painter.LineTo(lower);
            painter.ClosePath();
            painter.Fill();
        }
    }
}