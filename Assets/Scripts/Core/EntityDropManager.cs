// Done
using System.Collections.Generic;
using System.Linq;
using Data.ItemData;
using Interactables;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Core
{
    public class EntityDropManager : MonoBehaviour
    {
        [SerializeField] private GameObject itemDropPrefab;
        [SerializeField] private ItemListData dropData;

        [Header("Drop Restrictions")]
        [SerializeField] private int maxRarity = 1200;
        [SerializeField] private int maxItems = 3;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.X))
                DropItems();
        }

        public virtual void DropItems()
        {
            if (!dropData)
            {
                Debug.Log($"You need to assign drop data to {gameObject.name}");
                return;
            }

            List<ItemData> itemsToDrop = RollDrops();
            int amountToDrop = Mathf.Min(itemsToDrop.Count, maxItems);

            for (int i = 0; i < amountToDrop; i++)
                CreateItemDrop(itemsToDrop[i]);
        }

        public void CreateItemDrop(ItemData itemToDrop)
        {
            GameObject droppedItem = Instantiate(itemDropPrefab, transform.position, Quaternion.identity);
            droppedItem.GetComponent<ItemPickup>().SetupItem(itemToDrop);
        }

        public List<ItemData> RollDrops()
        {
            List<ItemData> possibleDrops = new();
            List<ItemData> finalDrops = new();
            float maxRarity = this.maxRarity;

            foreach (ItemData item in dropData.ItemList)
            {
                float dropChance = item.GetDropChance();

                if (Random.Range(0, 100) <= dropChance)
                    possibleDrops.Add(item);
            }

            possibleDrops = possibleDrops.OrderByDescending(i => i.Rarity).ToList();

            foreach (ItemData item in possibleDrops)
            {
                if (maxRarity > item.Rarity)
                {
                    finalDrops.Add(item);
                    maxRarity -= item.Rarity;
                }
            }

            return finalDrops;
        }
    }
}