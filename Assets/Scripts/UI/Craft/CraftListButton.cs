// Done
using Data.ItemData;
using UnityEngine;

namespace UI
{
    public class CraftListButton : MonoBehaviour
    {
        private ItemData[] _craftRecipes;
        private CraftSlot[] _craftSlots;

        public void SetCraftsSlots(CraftSlot[] craftSlots) => _craftSlots = craftSlots;

        public void SetCraftRecipes(ItemData[] recipes) => _craftRecipes = recipes;

        public void UpdateCraftSlots()
        {
            if (_craftRecipes == null || _craftRecipes.Length == 0)
                return;

            foreach (CraftSlot slot in _craftSlots)
                slot.gameObject.SetActive(false);

            for (int i = 0; i < _craftRecipes.Length; i++)
            {
                _craftSlots[i].gameObject.SetActive(true);
                _craftSlots[i].SetupButton(_craftRecipes[i]);
            }
        }
    }
}