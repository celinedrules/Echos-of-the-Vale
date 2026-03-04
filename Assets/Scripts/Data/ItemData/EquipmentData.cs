// Done
using System;
using System.Collections;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using Utilities.Enums;

namespace Data.ItemData
{
    [CreateAssetMenu(fileName = "Equipment Data - ", menuName = "Echos of the Vale/Item Data/Equipment Item")]
    public class EquipmentData : ItemData
    {
        [VerticalGroup(LeftVerticalGroup)]
        [ValueDropdown("CustomAddStatsButton", IsUniqueList = true, DrawDropdownForListElements = false,
            DropdownTitle = "Modify Stats")]
        [ListDrawerSettings(ShowFoldout = true, DraggableItems = false)]
        [SerializeField] private ItemModifier[] modifiers;

        public ItemModifier[] Modifiers => modifiers;

#if UNITY_EDITOR
        private IEnumerable CustomAddStatsButton()
        {
            return Enum.GetValues(typeof(StatType)).Cast<StatType>()
                .Except(modifiers.Select(x => x.StatType))
                .Select(x => new ItemModifier(x))
                .AppendWith(modifiers)
                .Select(x => new ValueDropdownItem(x.StatType.ToString(), x));
        }
#endif
    }

    [Serializable, InlineProperty]
    public class ItemModifier
    {
        [SerializeField, HideInInspector] private StatType statType;

        public int Value
        {
            get => value;
            set => this.value = value;
        }

        [SerializeField, LabelText("$statType"), Range(0, 500)]
        private int value;

        public StatType StatType => statType;

        public ItemModifier()
        {
        }

        public ItemModifier(StatType type)
        {
            statType = type;
        }
    }
}