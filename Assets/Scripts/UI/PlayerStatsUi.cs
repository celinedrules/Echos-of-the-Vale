// Done
using InventorySystem;
using Managers;
using UI.Inventory;
using UnityEngine;

namespace UI
{
    public class PlayerStatsUi : MonoBehaviour
    {
        private StatSlot[] _statSlots;
        private InventoryPlayer _inventory;
        
        private void Awake() => _statSlots = GetComponentsInChildren<StatSlot>();

        private void Start()
        {
            _inventory = GameManager.Instance.Player.GetComponent<InventoryPlayer>();
            _inventory.OnInventoryChanged += UpdateStatsUi;
            
            UpdateStatsUi();
        }

        private void UpdateStatsUi()
        {
            foreach (StatSlot slot in _statSlots)
                slot.UpdateStatValue();
        }
    }
}