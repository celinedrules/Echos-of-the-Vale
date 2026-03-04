// Done
using UnityEngine;

namespace Data.ItemData
{
    [CreateAssetMenu(fileName = "List of Items - ", menuName = "Echos of the Vale/Item Data/Item List")]
    public class ItemListData : ScriptableObject
    {
        [SerializeField] protected ItemData[] itemList;

        public ItemData[] ItemList => itemList;
    }
}