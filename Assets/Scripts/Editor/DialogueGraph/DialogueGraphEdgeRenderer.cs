using System.Collections.Generic;
using Data.DialogueData;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.DialogueGraph
{
    public static class DialogueGraphEdgeRenderer
    {
        private static readonly Color LeadsToEdgeColor = new(0.58f, 0.79f, 1f);
        private static readonly Color ChoiceEdgeColor = new(0.95f, 0.74f, 0.32f);
        private static readonly Color InvalidEdgeColor = new(0.95f, 0.28f, 0.28f);

        private const float MissingEdgeLength = 120f;

        public static void Draw(
            DialogueTable table,
            IReadOnlyDictionary<int, VisualElement> nodeViewsByRowId,
            MeshGenerationContext context)
        {
            if (table == null || table.RowCount == 0 || nodeViewsByRowId == null || nodeViewsByRowId.Count == 0)
                return;

            Painter2D painter = context.painter2D;
            painter.lineWidth = 3f;

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

            if (!nodeViewsByRowId.TryGetValue(targetRowId, out VisualElement targetNode))
            {
                if (drawDanglingIfMissing)
                    DrawDanglingEdge(painter, sourceRect, color);

                return;
            }

            Rect targetRect = targetNode.layout;
            if (targetRect.width <= 0f || targetRect.height <= 0f)
                return;

            Vector2 start = new Vector2(sourceRect.xMax, sourceRect.center.y);
            Vector2 end = new Vector2(targetRect.xMin, targetRect.center.y);

            float tangentOffset = Mathf.Max(60f, Mathf.Abs(end.x - start.x) * 0.35f);
            Vector2 startTangent = start + Vector2.right * tangentOffset;
            Vector2 endTangent = end + Vector2.left * tangentOffset;

            painter.strokeColor = color;
            painter.BeginPath();
            painter.MoveTo(start);
            painter.BezierCurveTo(startTangent, endTangent, end);
            painter.Stroke();

            DrawArrowHead(painter, end, color);
        }

        private static void DrawDanglingEdge(Painter2D painter, Rect sourceRect, Color color)
        {
            Vector2 start = new Vector2(sourceRect.xMax, sourceRect.center.y);
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