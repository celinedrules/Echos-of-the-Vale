using Data.ItemData;
using InventorySystem;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Editor.Drawers
{
    public class InventoryItemDrawer<TInventoryItem> : OdinValueDrawer<TInventoryItem>
        where TInventoryItem : InventoryItem
    {
        // private bool IsInsideQuestData()
        // {
        //     // Walk up the property tree to find the root object type
        //     var root = Property.Tree.UnitySerializedObject;
        //     return root != null && root.targetObject is QuestData;
        // }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            Rect rect = EditorGUILayout.GetControlRect(label != null, 45);

            if (label != null)
                rect.xMin = EditorGUI.PrefixLabel(rect.AlignCenterY(15), label).xMin;
            else
                rect = EditorGUI.IndentedRect(rect);

            InventoryItem item = ValueEntry.SmartValue;
            Texture texture = null;

            if (item?.ItemData)
            {
                texture = GUIHelper.GetAssetThumbnail(item.ItemData.ItemIcon, typeof(ItemData), true);
                GUI.Label(rect.AddXMin(50).AlignMiddle(16), EditorGUI.showMixedValue ? "-" : item.ItemData.ItemName);
            }

            // Get the result from the object field
            ItemData newItemData = SirenixEditorFields.UnityPreviewObjectField(
                rect.AlignLeft(45),
                item?.ItemData,
                texture,
                typeof(ItemData)
            ) as ItemData;

            // Handle the conversion: if the selected ItemData changed, update the InventoryItem
            ItemData currentItemData = item?.ItemData;

            if (newItemData != currentItemData)
            {
                if (newItemData != null)
                {
                    int previousStack = item?.StackSize ?? 1;
                    ValueEntry.SmartValue = (TInventoryItem)new InventoryItem(newItemData);
                    ValueEntry.SmartValue.StackSize = previousStack;
                }
                else
                {
                    ValueEntry.SmartValue = default;
                }
            }

            // Only show stack size when inside a QuestData ScriptableObject
            // if (item?.ItemData != null && IsInsideQuestData())
            // {
            //     Rect stackRect = EditorGUILayout.GetControlRect(false, 18);
            //     stackRect = EditorGUI.IndentedRect(stackRect);
            //     stackRect.xMin += 50;
            //
            //     int newStack = EditorGUI.IntField(stackRect, "Amount", item.StackSize);
            //     if (newStack != item.StackSize)
            //     {
            //         item.StackSize = Mathf.Max(1, newStack);
            //         ValueEntry.Values.ForceMarkDirty();
            //     }
            // }
        }
    }
}