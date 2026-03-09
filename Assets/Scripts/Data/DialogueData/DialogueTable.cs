using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Data.DialogueData
{
    [CreateAssetMenu(fileName = "DialogueTable", menuName = "Echos of the Vale/Dialogue Data/Dialogue Table")]
    public class DialogueTable : ScriptableObject
    {
        [SerializeField] private string tableName;

        [TitleGroup("Tools")]
        [InfoBox("Renumber IDs + Remap References is disabled while duplicate Row IDs exist. Fix duplicates first.", InfoMessageType.Warning, nameof(HasDuplicateRowIds))]
        [HorizontalGroup("Tools/Buttons")]
        [Button("Auto Organize", ButtonSizes.Medium), GUIColor(0.35f, 0.85f, 1f)]
        private void AutoOrganizeRows() => DialogueTableUtility.AutoOrganize(rows);

        [HorizontalGroup("Tools/Buttons")]
        [Button("Sort By Row Id", ButtonSizes.Medium), GUIColor(0.4f, 0.8f, 1f)]
        private void SortRowsByRowId() => DialogueTableUtility.SortRowsByRowId(rows);

        [HorizontalGroup("Tools/Buttons")]
        [EnableIf(nameof(CanRenumberSafely))]
        [Button("Renumber IDs + Remap References", ButtonSizes.Medium), GUIColor(0.6f, 1f, 0.6f)]
        private void RenumberRowsSequentiallyAndRemapReferences()
        {
            if (DialogueTableUtility.RenumberRowsSequentiallyAndRemapReferences(rows))
                return;

            Debug.LogWarning($"Cannot safely renumber dialogue table '{tableName}' while duplicate Row IDs exist. Fix duplicates first.");
        }

        [HorizontalGroup("Tools/Buttons")]
        [Button("Fix Duplicate IDs", ButtonSizes.Medium), GUIColor(1f, 0.85f, 0.4f)]
        private void FixDuplicateRowIds() => DialogueTableUtility.FixDuplicateRowIds(rows);

        [ListDrawerSettings(Expanded = true)]
        [SerializeField] private List<DialogueRow> rows = new();

        public string TableName => tableName;
        public IReadOnlyList<DialogueRow> Rows => rows;
        public DialogueRow FirstRow => rows.Count > 0 ? rows[0] : null;
        public int RowCount => rows.Count;

        public DialogueRow GetRow(int index)
        {
            if (index < 0 || index >= rows.Count)
                return null;

            return rows[index];
        }

        public DialogueRow GetRowById(int rowId)
        {
            for (int i = 0; i < rows.Count; i++)
            {
                if (rows[i].RowId == rowId)
                    return rows[i];
            }

            return null;
        }

        public bool HasRowId(int rowId) => GetRowById(rowId) != null;

        public List<string> GetValidationMessages() => DialogueTableValidator.GetValidationMessages(this);

        private bool CanRenumberSafely() => DialogueTableUtility.CanRenumberSafely(rows);
        private bool HasDuplicateRowIds() => DialogueTableValidator.HasDuplicateRowIds(this);
    }
}