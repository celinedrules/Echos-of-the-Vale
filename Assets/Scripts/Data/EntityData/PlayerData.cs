// Done
using Data.InventoryData;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities.Enums;

namespace Data.EntityData
{
    [CreateAssetMenu(fileName = "Player Data - ", menuName = "Echos of the Vale/Player Data/Player Data")]
    public class PlayerData : EntityData
    {
        [HorizontalGroup("Split", 200), EnumToggleButtons]
        [SerializeField, HideLabel] private PlayerClasses playerClass;

        [VerticalGroup("Split/Meta")]
        [SerializeField, LabelText("Age"), Range(0, 100)]
        private int age;
        
        [TabGroup("Starting Inventory")]
        [SerializeField, HideLabel]
        private InventoryGrid inventory = new();

        [TabGroup("Starting Stats")]
        [SerializeField, HideLabel] private PlayerStatsData playerStatsData;

        [TabGroup("Starting Equipment")]
        [SerializeField, HideLabel] private EquipData equipData = new();

        public override EntityStatsData StatsData => playerStatsData;
        public InventoryGrid Inventory => inventory;
        public EquipData EquipData => equipData;
    }
}