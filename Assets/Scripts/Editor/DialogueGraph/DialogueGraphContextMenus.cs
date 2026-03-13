using UnityEngine.UIElements;
using Utilities.Enums;

namespace Editor.DialogueGraph
{
    public static class DialogueGraphContextMenus
    {
        public static void BuildCanvasMenu(DialogueGraphWindow window, ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("Add/Line",
                _ => window.CreateRowFromMenu(DialogueRowKind.Line),
                window.HasSelectedTable
                    ? DropdownMenuAction.Status.Normal
                    : DropdownMenuAction.Status.Disabled);

            evt.menu.AppendAction("Add/Choice Prompt",
                _ => window.CreateRowFromMenu(DialogueRowKind.ChoicePrompt),
                window.HasSelectedTable
                    ? DropdownMenuAction.Status.Normal
                    : DropdownMenuAction.Status.Disabled);

            evt.menu.AppendAction("Add/Choice Response",
                _ => window.CreateRowFromMenu(DialogueRowKind.ChoiceResponse),
                window.HasSelectedTable
                    ? DropdownMenuAction.Status.Normal
                    : DropdownMenuAction.Status.Disabled);

            evt.menu.AppendSeparator();

            evt.menu.AppendAction("Auto Layout",
                _ => window.AutoLayoutFromMenu(),
                window.HasSelectedTable
                    ? DropdownMenuAction.Status.Normal
                    : DropdownMenuAction.Status.Disabled);

            evt.menu.AppendSeparator();

            evt.menu.AppendAction("Duplicate Selected",
                _ => window.DuplicateSelectedRowFromMenu(),
                window.HasSelectedRow
                    ? DropdownMenuAction.Status.Normal
                    : DropdownMenuAction.Status.Disabled);

            evt.menu.AppendAction("Duplicate Selected (Reset Links)",
                _ => window.DuplicateSelectedRowResetLinksFromMenu(),
                window.HasSelectedRow
                    ? DropdownMenuAction.Status.Normal
                    : DropdownMenuAction.Status.Disabled);

            evt.menu.AppendAction("Delete Selected",
                _ => window.DeleteSelectedRowFromMenu(),
                window.HasSelectedRow
                    ? DropdownMenuAction.Status.Normal
                    : DropdownMenuAction.Status.Disabled);
        }

        public static void BuildNodeMenu(DialogueGraphWindow window, ContextualMenuPopulateEvent evt, int rowId, int rowIndex)
        {
            window.SelectRow(rowId, rowIndex);

            evt.menu.AppendAction("Add/Line",
                _ => window.CreateRowFromMenu(DialogueRowKind.Line),
                window.HasSelectedTable
                    ? DropdownMenuAction.Status.Normal
                    : DropdownMenuAction.Status.Disabled);

            evt.menu.AppendAction("Add/Choice Prompt",
                _ => window.CreateRowFromMenu(DialogueRowKind.ChoicePrompt),
                window.HasSelectedTable
                    ? DropdownMenuAction.Status.Normal
                    : DropdownMenuAction.Status.Disabled);

            evt.menu.AppendAction("Add/Choice Response",
                _ => window.CreateRowFromMenu(DialogueRowKind.ChoiceResponse),
                window.HasSelectedTable
                    ? DropdownMenuAction.Status.Normal
                    : DropdownMenuAction.Status.Disabled);

            evt.menu.AppendSeparator();

            evt.menu.AppendAction("Auto Layout",
                _ => window.AutoLayoutFromMenu(),
                window.HasSelectedTable
                    ? DropdownMenuAction.Status.Normal
                    : DropdownMenuAction.Status.Disabled);

            evt.menu.AppendSeparator();

            evt.menu.AppendAction("Duplicate Selected",
                _ => window.DuplicateSelectedRowFromMenu(),
                window.HasSelectedRow
                    ? DropdownMenuAction.Status.Normal
                    : DropdownMenuAction.Status.Disabled);

            evt.menu.AppendAction("Duplicate Selected (Reset Links)",
                _ => window.DuplicateSelectedRowResetLinksFromMenu(),
                window.HasSelectedRow
                    ? DropdownMenuAction.Status.Normal
                    : DropdownMenuAction.Status.Disabled);

            evt.menu.AppendAction("Delete Selected",
                _ => window.DeleteSelectedRowFromMenu(),
                window.HasSelectedRow
                    ? DropdownMenuAction.Status.Normal
                    : DropdownMenuAction.Status.Disabled);
        }
    }
}