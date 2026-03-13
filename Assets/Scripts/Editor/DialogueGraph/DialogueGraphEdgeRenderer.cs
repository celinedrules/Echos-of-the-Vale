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
                        DrawEdge(painter, nodeViewsByRowId, sourceRow.RowId, choiceRowIds[j], ChoiceEdgeColor);
                }
                else if (sourceRow.UsesLeadsTo && sourceRow.LeadsTo >= 0)
                {
                    DrawEdge(painter, nodeViewsByRowId, sourceRow.RowId, sourceRow.LeadsTo, LeadsToEdgeColor);
                }
            }
        }

        private static void DrawEdge(
            Painter2D painter,
            IReadOnlyDictionary<int, VisualElement> nodeViewsByRowId,
            int sourceRowId,
            int targetRowId,
            Color color)
        {
            if (!nodeViewsByRowId.TryGetValue(sourceRowId, out VisualElement sourceNode))
                return;

            if (!nodeViewsByRowId.TryGetValue(targetRowId, out VisualElement targetNode))
                return;

            Rect sourceRect = sourceNode.layout;
            Rect targetRect = targetNode.layout;

            if (sourceRect.width <= 0f || sourceRect.height <= 0f || targetRect.width <= 0f || targetRect.height <= 0f)
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