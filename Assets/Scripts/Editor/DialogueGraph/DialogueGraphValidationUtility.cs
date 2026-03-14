using System.Collections.Generic;
using System.Text.RegularExpressions;
using Data.DialogueData;

namespace Editor.DialogueGraph
{
    public static class DialogueGraphValidationUtility
    {
        public static HashSet<int> GetInvalidRowIds(DialogueTable table)
        {
            HashSet<int> invalidRowIds = new();

            if (table == null)
                return invalidRowIds;

            List<string> messages = table.GetValidationMessages();
            if (messages == null)
                return invalidRowIds;

            for (int i = 0; i < messages.Count; i++)
            {
                int rowId = TryExtractRowId(messages[i]);
                if (rowId >= 0)
                    invalidRowIds.Add(rowId);
            }

            return invalidRowIds;
        }

        public static Dictionary<int, List<string>> GetValidationMessagesByRowId(DialogueTable table)
        {
            Dictionary<int, List<string>> messagesByRowId = new();

            if (table == null)
                return messagesByRowId;

            List<string> messages = table.GetValidationMessages();
            if (messages == null)
                return messagesByRowId;

            for (int i = 0; i < messages.Count; i++)
            {
                string message = messages[i];
                int rowId = TryExtractRowId(message);

                if (rowId < 0)
                    continue;

                if (!messagesByRowId.TryGetValue(rowId, out List<string> rowMessages))
                {
                    rowMessages = new List<string>();
                    messagesByRowId.Add(rowId, rowMessages);
                }

                rowMessages.Add(message);
            }

            return messagesByRowId;
        }

        public static bool HasStartNodeIssue(DialogueTable table)
        {
            return table != null && table.StartRowId < 0;
        }

        public static string GetStartNodeValidationMessage(DialogueTable table)
        {
            if (!HasStartNodeIssue(table))
                return null;

            return "Start node is not connected.";
        }

        public static int TryExtractRowId(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return -1;

            Match match = Regex.Match(message, @"Row\s+(\d+)", RegexOptions.IgnoreCase);
            if (match.Success && int.TryParse(match.Groups[1].Value, out int rowId))
                return rowId;

            match = Regex.Match(message, @"row\s+(\d+)", RegexOptions.IgnoreCase);
            if (match.Success && int.TryParse(match.Groups[1].Value, out rowId))
                return rowId;

            return -1;
        }
    }
}