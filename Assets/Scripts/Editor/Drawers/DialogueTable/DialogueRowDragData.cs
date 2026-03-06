// Done
namespace Editor.Drawers.DialogueTable
{
    /// <summary>
    /// Carries the row ID during a drag-and-drop operation from the dialogue table grid.
    /// </summary>
    internal class DialogueRowDragData
    {
        public const string DragId = "DialogueRowDrag";
        public int RowId { get; }
        public int RowIndex { get; }

        public DialogueRowDragData(int rowId, int rowIndex)
        {
            RowId = rowId;
            RowIndex = rowIndex;
        }
    }
}