// Done
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Data.ItemData
{
    [CreateAssetMenu(fileName = "Item Database", menuName = "Echos of the Vale/Item Data/Item Database")]
    public class ItemDatabase : ItemListData
    {
        private Dictionary<string, ItemData> _itemLookup;

        public ItemData GetItemData(string saveId)
        {
            if (_itemLookup == null)
                BuildLookup();

            return _itemLookup.GetValueOrDefault(saveId);
        }

        private void BuildLookup()
        {
            _itemLookup = new Dictionary<string, ItemData>();

            foreach (ItemData item in ItemList)
            {
                if (item && !string.IsNullOrEmpty(item.SaveId))
                    _itemLookup[item.SaveId] = item;
            }
        }

        private void OnEnable() => _itemLookup = null;

#if UNITY_EDITOR
        [ContextMenu("Auto-fill will all ItemData")]
        [PropertyOrder(-1)]
        [Button("Auto-fill will all ItemData", ButtonSizes.Gigantic)]
        public void CollectItemsData()
        {
            string[] guids = AssetDatabase.FindAssets("t:ItemData");

            itemList = guids
                .Select(guid => AssetDatabase.LoadAssetAtPath<ItemData>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(itemData => itemData).ToArray();

            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
#endif
    }
}