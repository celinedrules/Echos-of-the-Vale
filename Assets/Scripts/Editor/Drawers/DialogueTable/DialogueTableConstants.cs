// Done
using UnityEngine;

namespace Editor.Drawers.DialogueTable
{
    /// <summary>
    /// Shared constants for the dialogue table editor UI.
    /// </summary>
    internal static class DialogueTableConstants
    {
        // Column widths
        public const float ColId = 40f;
        public const float ColSpeaker = 110f;
        public const float ColAction = 120f;
        public const float ColLeadsTo = 100f;
        public const float ColSkip = 55f;
        public const float ColChoiceAnswer = 180f;
        public const float ColAudio = 90f;
        public const float ScrollBarWidth = 16f;
        public const float SplitterHeight = 5f;
        public const float ToolbarHeight = 20f;
        public const float RowHeight = 20f;

        // Colors
        public static readonly Color HeaderBg = new(0.18f, 0.18f, 0.18f, 1f);
        public static readonly Color RowEven = new(0.24f, 0.24f, 0.24f, 1f);
        public static readonly Color RowOdd = new(0.27f, 0.27f, 0.27f, 1f);
        //public static readonly Color RowSelected = new(0.9f, 0.55f, 0f, 0.35f);
        public static readonly Color RowSelected = new(0.173f, 0.365f, .529f, 1f);
        public static readonly Color RowHovered = new(1f, 1f, 1f, 0.07f);
        //public static readonly Color SelectedText = new(1f, 0.7f, 0.2f, 1f);
        public static readonly Color SelectedText = new(1f, .498f, 0f, 1f);
        public static readonly Color SplitterBg = new(0.12f, 0.12f, 0.12f, 1f);

        /// <summary>
        /// Computes the flexible Text column width based on the available total width.
        /// </summary>
        public static float GetTextColumnWidth(float totalWidth)
        {
            return Mathf.Max(80, totalWidth - ColId - ColSpeaker - ColAction -  ColLeadsTo
                                 - ColSkip - ColChoiceAnswer - ColAudio - ScrollBarWidth);
        }
    }
}