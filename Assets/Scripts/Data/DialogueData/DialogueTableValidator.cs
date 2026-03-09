using System.Collections.Generic;
using Utilities.Enums;

namespace Data.DialogueData
{
    public static class DialogueTableValidator
    {
        public static List<string> GetValidationMessages(DialogueTable table)
        {
            List<string> messages = new();

            if (table == null)
            {
                messages.Add("Dialogue table is null.");
                return messages;
            }

            IReadOnlyList<DialogueRow> rows = table.Rows;
            if (rows == null || rows.Count == 0)
            {
                messages.Add("The dialogue table has no rows.");
                return messages;
            }

            Dictionary<int, int> rowIdCounts = new();
            HashSet<int> referencedChoiceRowIds = new();

            for (int i = 0; i < rows.Count; i++)
            {
                DialogueRow row = rows[i];
                int rowId = row.RowId;

                if (rowIdCounts.ContainsKey(rowId))
                    rowIdCounts[rowId]++;
                else
                    rowIdCounts[rowId] = 1;

                if (row.RowKind == DialogueRowKind.ChoicePrompt && row.ChoiceRowIds != null)
                {
                    for (int j = 0; j < row.ChoiceRowIds.Length; j++)
                        referencedChoiceRowIds.Add(row.ChoiceRowIds[j]);
                }
            }

            foreach (KeyValuePair<int, int> pair in rowIdCounts)
            {
                if (pair.Value > 1)
                    messages.Add($"Duplicate Row Id found: {pair.Key} appears {pair.Value} times.");
            }

            for (int i = 0; i < rows.Count; i++)
            {
                DialogueRow row = rows[i];
                int rowId = row.RowId;

                if (row.LeadsTo == rowId && row.LeadsTo >= 0)
                    messages.Add($"Row {rowId} leads to itself.");

                if (row.UsesLeadsTo && row.LeadsTo >= 0 && table.GetRowById(row.LeadsTo) == null)
                    messages.Add($"Row {rowId} has Leads To = {row.LeadsTo}, but that row does not exist.");

                if (row.RowKind == DialogueRowKind.ChoicePrompt)
                {
                    if (row.ChoiceRowIds == null || row.ChoiceRowIds.Length == 0)
                    {
                        messages.Add($"ChoicePrompt row {rowId} does not define any Choice Row Ids.");
                    }
                    else
                    {
                        for (int j = 0; j < row.ChoiceRowIds.Length; j++)
                        {
                            int choiceRowId = row.ChoiceRowIds[j];

                            if (choiceRowId == rowId)
                                messages.Add($"ChoicePrompt row {rowId} references itself as a choice.");

                            DialogueRow choiceRow = table.GetRowById(choiceRowId);
                            if (choiceRow == null)
                            {
                                messages.Add($"ChoicePrompt row {rowId} references missing choice row {choiceRowId}.");
                                continue;
                            }

                            if (choiceRow.RowKind != DialogueRowKind.ChoiceResponse)
                                messages.Add($"ChoicePrompt row {rowId} references row {choiceRowId}, which is not a ChoiceResponse row.");
                        }
                    }
                }

                if (row.RowKind == DialogueRowKind.ChoiceResponse && !referencedChoiceRowIds.Contains(rowId))
                    messages.Add($"ChoiceResponse row {rowId} is not referenced by any ChoicePrompt row.");
            }

            return messages;
        }

        public static bool HasDuplicateRowIds(DialogueTable table)
        {
            if (table == null || table.Rows == null)
                return false;

            HashSet<int> seenIds = new();

            for (int i = 0; i < table.Rows.Count; i++)
            {
                if (!seenIds.Add(table.Rows[i].RowId))
                    return true;
            }

            return false;
        }
    }
}