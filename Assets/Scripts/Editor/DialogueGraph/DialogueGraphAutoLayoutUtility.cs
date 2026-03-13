using System.Collections.Generic;
using Data.DialogueData;
using UnityEditor;
using UnityEngine;

namespace Editor.DialogueGraph
{
    public static class DialogueGraphAutoLayoutUtility
    {
        private const float StartX = 80f;
        private const float StartY = 80f;
        private const float ColumnSpacing = 380f;
        private const float RowSpacing = 180f;
        private const float GroupSpacing = 120f;

        public static void AutoLayout(DialogueTable table)
        {
            if (table == null || table.RowCount == 0)
                return;

            Undo.RecordObject(table, "Auto Layout Dialogue Graph");

            Dictionary<int, List<int>> outgoingByRowId = BuildOutgoingMap(table);
            Dictionary<int, int> incomingCountByRowId = BuildIncomingCountMap(table, outgoingByRowId);

            List<int> rootRowIds = GetRootRowIds(table, incomingCountByRowId);
            HashSet<int> visited = new();
            Dictionary<int, int> depthByRowId = new();

            for (int i = 0; i < rootRowIds.Count; i++)
                AssignDepths(rootRowIds[i], 0, outgoingByRowId, depthByRowId, visited);

            for (int i = 0; i < table.RowCount; i++)
            {
                DialogueRow row = table.GetRow(i);
                if (row == null || visited.Contains(row.RowId))
                    continue;

                AssignDepths(row.RowId, 0, outgoingByRowId, depthByRowId, visited);
            }

            Dictionary<int, List<int>> rowsByDepth = new();

            for (int i = 0; i < table.RowCount; i++)
            {
                DialogueRow row = table.GetRow(i);
                if (row == null)
                    continue;

                int depth = depthByRowId.TryGetValue(row.RowId, out int assignedDepth) ? assignedDepth : 0;

                if (!rowsByDepth.TryGetValue(depth, out List<int> rowsAtDepth))
                {
                    rowsAtDepth = new List<int>();
                    rowsByDepth.Add(depth, rowsAtDepth);
                }

                rowsAtDepth.Add(row.RowId);
            }

            List<int> orderedDepths = new(rowsByDepth.Keys);
            orderedDepths.Sort();

            float currentY = StartY;

            for (int i = 0; i < orderedDepths.Count; i++)
            {
                int depth = orderedDepths[i];
                List<int> rowIds = rowsByDepth[depth];
                rowIds.Sort();

                for (int j = 0; j < rowIds.Count; j++)
                {
                    Vector2 position = new Vector2(
                        StartX + depth * ColumnSpacing,
                        currentY);

                    table.SetNodePosition(rowIds[j], position);
                    currentY += RowSpacing;
                }

                currentY += GroupSpacing;
            }

            EditorUtility.SetDirty(table);
            AssetDatabase.SaveAssets();
        }

        private static Dictionary<int, List<int>> BuildOutgoingMap(DialogueTable table)
        {
            Dictionary<int, List<int>> outgoingByRowId = new();

            for (int i = 0; i < table.RowCount; i++)
            {
                DialogueRow row = table.GetRow(i);
                if (row == null)
                    continue;

                List<int> targets = new();

                if (row.IsChoicePromptRow)
                {
                    int[] choiceRowIds = row.ChoiceRowIds;
                    if (choiceRowIds != null)
                    {
                        for (int j = 0; j < choiceRowIds.Length; j++)
                        {
                            if (table.HasRowId(choiceRowIds[j]))
                                targets.Add(choiceRowIds[j]);
                        }
                    }
                }
                else if (row.UsesLeadsTo && row.LeadsTo >= 0 && table.HasRowId(row.LeadsTo))
                {
                    targets.Add(row.LeadsTo);
                }

                outgoingByRowId[row.RowId] = targets;
            }

            return outgoingByRowId;
        }

        private static Dictionary<int, int> BuildIncomingCountMap(
            DialogueTable table,
            Dictionary<int, List<int>> outgoingByRowId)
        {
            Dictionary<int, int> incomingCountByRowId = new();

            for (int i = 0; i < table.RowCount; i++)
            {
                DialogueRow row = table.GetRow(i);
                if (row != null)
                    incomingCountByRowId[row.RowId] = 0;
            }

            foreach (KeyValuePair<int, List<int>> pair in outgoingByRowId)
            {
                List<int> targets = pair.Value;
                for (int i = 0; i < targets.Count; i++)
                {
                    if (incomingCountByRowId.ContainsKey(targets[i]))
                        incomingCountByRowId[targets[i]]++;
                }
            }

            return incomingCountByRowId;
        }

        private static List<int> GetRootRowIds(DialogueTable table, Dictionary<int, int> incomingCountByRowId)
        {
            List<int> rootRowIds = new();

            for (int i = 0; i < table.RowCount; i++)
            {
                DialogueRow row = table.GetRow(i);
                if (row == null)
                    continue;

                if (incomingCountByRowId.TryGetValue(row.RowId, out int incomingCount) && incomingCount == 0)
                    rootRowIds.Add(row.RowId);
            }

            rootRowIds.Sort();
            return rootRowIds;
        }

        private static void AssignDepths(
            int rowId,
            int depth,
            Dictionary<int, List<int>> outgoingByRowId,
            Dictionary<int, int> depthByRowId,
            HashSet<int> visited)
        {
            if (depthByRowId.TryGetValue(rowId, out int existingDepth))
            {
                if (depth <= existingDepth)
                    return;

                depthByRowId[rowId] = depth;
            }
            else
            {
                depthByRowId[rowId] = depth;
            }

            if (!visited.Add(rowId))
                return;

            if (!outgoingByRowId.TryGetValue(rowId, out List<int> targets))
                return;

            for (int i = 0; i < targets.Count; i++)
                AssignDepths(targets[i], depth + 1, outgoingByRowId, depthByRowId, visited);

            visited.Remove(rowId);
        }
    }
}