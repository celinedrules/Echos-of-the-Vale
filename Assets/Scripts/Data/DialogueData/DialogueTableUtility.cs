using System.Collections.Generic;

namespace Data.DialogueData
{
    public static class DialogueTableUtility
    {
        public static void SortRowsByRowId(List<DialogueRow> rows)
        {
            if (rows == null)
                return;

            rows.Sort((a, b) => a.RowId.CompareTo(b.RowId));
        }

        public static void FixDuplicateRowIds(List<DialogueRow> rows)
        {
            if (rows == null)
                return;

            HashSet<int> usedIds = new();
            int nextAvailableId = 0;

            for (int i = 0; i < rows.Count; i++)
            {
                DialogueRow row = rows[i];

                if (usedIds.Add(row.RowId))
                    continue;

                while (usedIds.Contains(nextAvailableId))
                    nextAvailableId++;

                row.SetRowId(nextAvailableId);
                usedIds.Add(nextAvailableId);
                nextAvailableId++;
            }
        }

        public static bool CanRenumberSafely(IReadOnlyList<DialogueRow> rows)
        {
            if (rows == null)
                return false;

            HashSet<int> seenIds = new();

            for (int i = 0; i < rows.Count; i++)
            {
                if (!seenIds.Add(rows[i].RowId))
                    return false;
            }

            return true;
        }

        public static bool RenumberRowsSequentiallyAndRemapReferences(List<DialogueRow> rows)
        {
            if (rows == null || !CanRenumberSafely(rows))
                return false;

            Dictionary<int, int> oldToNewIds = new();

            for (int i = 0; i < rows.Count; i++)
                oldToNewIds.Add(rows[i].RowId, i);

            for (int i = 0; i < rows.Count; i++)
                rows[i].SetRowId(i);

            for (int i = 0; i < rows.Count; i++)
            {
                DialogueRow row = rows[i];

                if (row.LeadsTo >= 0 && oldToNewIds.TryGetValue(row.LeadsTo, out int remappedLeadsTo))
                    row.SetLeadsTo(remappedLeadsTo);

                int[] choiceRowIds = row.ChoiceRowIds;
                if (choiceRowIds == null || choiceRowIds.Length == 0)
                    continue;

                int[] remappedChoiceIds = new int[choiceRowIds.Length];
                for (int j = 0; j < choiceRowIds.Length; j++)
                {
                    int currentChoiceId = choiceRowIds[j];
                    remappedChoiceIds[j] = oldToNewIds.TryGetValue(currentChoiceId, out int remappedChoiceId)
                        ? remappedChoiceId
                        : currentChoiceId;
                }

                row.SetChoiceRowIds(remappedChoiceIds);
            }

            SortRowsByRowId(rows);
            return true;
        }

        public static void AutoOrganize(List<DialogueRow> rows)
        {
            FixDuplicateRowIds(rows);
            SortRowsByRowId(rows);
        }
    }
}