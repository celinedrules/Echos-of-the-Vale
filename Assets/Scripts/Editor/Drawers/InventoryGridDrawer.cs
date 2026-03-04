// Done
using Data.InventoryData;
using Data.ItemData;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Editor.Drawers
{
    public class InventoryGridDrawer : OdinValueDrawer<InventoryGrid>
    {
        private static int _activePickerControlId = -1;
        private static int _activePickerIndex = -1;

        private static int _dragSourceCol = -1;
        private static int _dragSourceRow = -1;
        private static InventoryGrid _dragSourceGrid;

        private float _cachedCellSize = 60f;

        protected override void DrawPropertyLayout(GUIContent label)
        {
            InventoryGrid grid = ValueEntry.SmartValue ?? new InventoryGrid();

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("Inventory Size", GUILayout.Width(100));
                int newSize = EditorGUILayout.IntField(grid.Size);

                if (newSize != grid.Size)
                {
                    grid.Size = newSize;
                    ValueEntry.SmartValue = grid;
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            const float cellSpacing = 2f;
            const int columns = InventoryGrid.Columns;
            int rows = grid.Rows;

            Rect measureRect = EditorGUILayout.GetControlRect(false, 1);
            
            if (Event.current.type == EventType.Repaint && measureRect.width > 10)
                _cachedCellSize = (measureRect.width - (cellSpacing * (columns - 1))) / columns;

            float cellSize = _cachedCellSize;
            float rowHeight = cellSize + cellSpacing;

            for (int row = 0; row < rows; row++)
            {
                Rect rowRect = GUILayoutUtility.GetRect(10, rowHeight, GUILayout.ExpandWidth(true));

                for (int col = 0; col < columns; col++)
                {
                    if (!grid.IsValidSlot(col, row))
                        break;

                    Rect cellRect = new(rowRect.x + col * (cellSize + cellSpacing), rowRect.y, cellSize, cellSize);
                    DrawCell(cellRect, grid, col, row);
                }
            }

            EditorGUILayout.Space(5);
            Rect trashRect = GUILayoutUtility.GetRect(10, 35, GUILayout.ExpandWidth(true)).Padding(2);
            int trashId = DragAndDropUtilities.GetDragAndDropId(trashRect);

            DragAndDropUtilities.DrawDropZone(trashRect, null as Object, null, trashId);

            GUIStyle style = new(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Italic,
                normal =
                {
                    textColor = new Color(1f, 1f, 1f, 0.5f)
                }
            };
            
            GUI.Label(trashRect, "Drag items here to remove", style);

            if (Event.current.type == EventType.DragPerform && trashRect.Contains(Event.current.mousePosition))
            {
                if (_dragSourceGrid != null && _dragSourceCol >= 0 && _dragSourceRow >= 0)
                {
                    _dragSourceGrid.SetSlot(_dragSourceCol, _dragSourceRow, default);
                    _dragSourceGrid = null;
                    _dragSourceCol = -1;
                    _dragSourceRow = -1;
                    GUI.changed = true;
                    DragAndDrop.AcceptDrag();
                    Event.current.Use();
                }
            }
            
            ValueEntry.SmartValue = grid;
        }

        private void DrawCell(Rect rect, InventoryGrid grid, int col, int row)
        {
            int index = row * InventoryGrid.Columns + col;
            int controlId = GUIUtility.GetControlID(FocusType.Passive, rect);
            int dropId = DragAndDropUtilities.GetDragAndDropId(rect);

            ItemSlotData slot = grid.GetSlot(col, row);

            DragAndDropUtilities.DrawDropZone(rect, null, null, dropId);

            if (slot.Item?.ItemIcon)
            {
                Rect spriteRect = rect.Padding(4);
                spriteRect.height -= 16;
                DrawSprite(spriteRect, slot.Item.ItemIcon);
            }

            if (slot.Item != null)
            {
                Rect countRect = rect.Padding(2).AlignBottom(16);

                string maxStackText = "/ " + slot.Item.MaxStackSize;
                float maxStackWidth =
                    SirenixGUIStyles.RightAlignedGreyMiniLabel.CalcSize(new GUIContent(maxStackText)).x;

                Rect amountRect = countRect.AlignLeft(countRect.width - maxStackWidth - 2);
                Rect maxRect = countRect.AlignRight(maxStackWidth);

                GUIStyle rightAlignedStyle = new(EditorStyles.miniTextField)
                {
                    alignment = TextAnchor.MiddleRight
                };

                int newAmount = EditorGUI.IntField(amountRect, Mathf.Max(1, slot.Amount), rightAlignedStyle);
               
                if (newAmount != slot.Amount)
                {
                    slot.Amount = newAmount;
                    grid.SetSlot(col, row, slot);
                }

                GUI.Label(maxRect, maxStackText, SirenixGUIStyles.RightAlignedGreyMiniLabel);
            }

            Event evt = Event.current;
            
            if (evt.type == EventType.MouseDown && evt.clickCount == 2 && rect.Contains(evt.mousePosition))
            {
                _activePickerControlId = controlId;
                _activePickerIndex = index;
                EditorGUIUtility.ShowObjectPicker<ItemData>(slot.Item, false, "", controlId);
                evt.Use();
            }

            if (_activePickerControlId == controlId && _activePickerIndex == index)
            {
                if (evt.commandName is "ObjectSelectorUpdated" or "ObjectSelectorClosed")
                {
                    if (EditorGUIUtility.GetObjectPickerControlID() == controlId)
                    {
                        ItemData picked = EditorGUIUtility.GetObjectPickerObject() as ItemData;
                        slot.Item = picked;
                        slot.Amount = picked ? Mathf.Max(1, slot.Amount == 0 ? 1 : slot.Amount) : 0;
                        grid.SetSlot(col, row, slot);
                        GUI.changed = true;

                        if (evt.commandName == "ObjectSelectorClosed")
                        {
                            _activePickerControlId = -1;
                            _activePickerIndex = -1;
                        }
                    }
                }
            }

            ItemSlotData dropped = DragAndDropUtilities.DropZone(rect, slot);

            if (dropped != slot)
            {
                if (_dragSourceGrid != null && _dragSourceCol >= 0 && _dragSourceRow >= 0)
                {
                    if (!(_dragSourceGrid == grid && _dragSourceCol == col && _dragSourceRow == row))
                        _dragSourceGrid.SetSlot(_dragSourceCol, _dragSourceRow, default);
                }

                grid.SetSlot(col, row, dropped);
                _dragSourceGrid = null;
                _dragSourceCol = -1;
                _dragSourceRow = -1;
            }

            ItemData droppedItem = DragAndDropUtilities.DropZone(rect, slot.Item);
            
            if (droppedItem != slot.Item)
            {
                slot.Item = droppedItem;
                slot.Amount = droppedItem != null ? 1 : 0;
                grid.SetSlot(col, row, slot);
            }

            if (slot.Item)
            {
                if (evt.type == EventType.MouseDrag && rect.Contains(evt.mousePosition))
                {
                    _dragSourceGrid = grid;
                    _dragSourceCol = col;
                    _dragSourceRow = row;
                }
            }

            DragAndDropUtilities.DragZone(rect, slot, true, true);
        }

        private static void DrawSprite(Rect rect, Sprite sprite)
        {
            if (!sprite || !sprite.texture)
                return;

            Texture tex = sprite.texture;
            Rect spriteRect = sprite.textureRect;

            Rect uvRect = new(spriteRect.x / tex.width, spriteRect.y / tex.height, spriteRect.width / tex.width,
                spriteRect.height / tex.height);

            float size = Mathf.Min(rect.width, rect.height);
            Rect drawRect = rect.AlignCenter(size, size);

            GUI.DrawTextureWithTexCoords(drawRect, tex, uvRect);
        }
    }
}