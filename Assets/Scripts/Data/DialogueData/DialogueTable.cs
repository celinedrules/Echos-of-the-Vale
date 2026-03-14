using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Data.DialogueData
{
    [CreateAssetMenu(fileName = "DialogueTable", menuName = "Echos of the Vale/Dialogue Data/Dialogue Table")]
    public class DialogueTable : ScriptableObject
    {
        [Serializable]
        private class DialogueGraphNodeLayout
        {
            [SerializeField] private int rowId;
            [SerializeField] private Vector2 position;

            public int RowId => rowId;
            public Vector2 Position
            {
                get => position;
                set => position = value;
            }

            public DialogueGraphNodeLayout(int rowId, Vector2 position)
            {
                this.rowId = rowId;
                this.position = position;
            }
        }

        [Serializable]
        private class DialogueGraphEditorData
        {
            [SerializeField] private List<DialogueGraphNodeLayout> nodeLayouts = new();
            [SerializeField] private Vector2 panPosition;
            [SerializeField] private float zoomScale = 1f;
            [SerializeField] private Vector2 startNodePosition = new(60f, 60f);

            public List<DialogueGraphNodeLayout> NodeLayouts => nodeLayouts;
            public Vector2 PanPosition
            {
                get => panPosition;
                set => panPosition = value;
            }

            public float ZoomScale
            {
                get => zoomScale;
                set => zoomScale = Mathf.Max(0.01f, value);
            }

            public Vector2 StartNodePosition
            {
                get => startNodePosition;
                set => startNodePosition = value;
            }
        }

        [SerializeField] private string tableName;
        [SerializeField] private int startRowId = -1;

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

        [ListDrawerSettings(ShowFoldout = false)]
        [SerializeField] private List<DialogueRow> rows = new();

        [SerializeField, HideInInspector] private DialogueGraphEditorData graphEditorData = new();

        public string TableName => tableName;
        public IReadOnlyList<DialogueRow> Rows => rows;
        public DialogueRow FirstRow => rows.Count > 0 ? rows[0] : null;
        public int RowCount => rows.Count;

        public int StartRowId
        {
            get => startRowId;
            set => startRowId = value;
        }

        public Vector2 GraphPanPosition
        {
            get => graphEditorData.PanPosition;
            set => graphEditorData.PanPosition = value;
        }

        public float GraphZoomScale
        {
            get => graphEditorData.ZoomScale;
            set => graphEditorData.ZoomScale = value;
        }

        public Vector2 StartNodePosition
        {
            get => graphEditorData.StartNodePosition;
            set => graphEditorData.StartNodePosition = value;
        }

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

        public Vector2 GetNodePosition(int rowId, Vector2 defaultPosition)
        {
            DialogueGraphNodeLayout layout = GetNodeLayout(rowId);
            return layout != null ? layout.Position : defaultPosition;
        }

        public void SetNodePosition(int rowId, Vector2 position)
        {
            DialogueGraphNodeLayout layout = GetNodeLayout(rowId);

            if (layout != null)
            {
                layout.Position = position;
                return;
            }

            graphEditorData.NodeLayouts.Add(new DialogueGraphNodeLayout(rowId, position));
        }

        public bool HasNodePosition(int rowId) => GetNodeLayout(rowId) != null;

        public void RemoveNodePosition(int rowId)
        {
            for (int i = graphEditorData.NodeLayouts.Count - 1; i >= 0; i--)
            {
                if (graphEditorData.NodeLayouts[i].RowId == rowId)
                    graphEditorData.NodeLayouts.RemoveAt(i);
            }
        }

        public void PruneMissingNodePositions()
        {
            HashSet<int> validRowIds = new();

            for (int i = 0; i < rows.Count; i++)
                validRowIds.Add(rows[i].RowId);

            for (int i = graphEditorData.NodeLayouts.Count - 1; i >= 0; i--)
            {
                if (!validRowIds.Contains(graphEditorData.NodeLayouts[i].RowId))
                    graphEditorData.NodeLayouts.RemoveAt(i);
            }

            if (startRowId >= 0 && !validRowIds.Contains(startRowId))
                startRowId = -1;
        }

        public List<string> GetValidationMessages() => DialogueTableValidator.GetValidationMessages(this);

        private DialogueGraphNodeLayout GetNodeLayout(int rowId)
        {
            for (int i = 0; i < graphEditorData.NodeLayouts.Count; i++)
            {
                if (graphEditorData.NodeLayouts[i].RowId == rowId)
                    return graphEditorData.NodeLayouts[i];
            }

            return null;
        }

        private bool CanRenumberSafely() => DialogueTableUtility.CanRenumberSafely(rows);
        private bool HasDuplicateRowIds() => DialogueTableValidator.HasDuplicateRowIds(this);
    }
}