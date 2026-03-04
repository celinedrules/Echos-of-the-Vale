using Managers;
using Stats;
using TMPro;
using UI.Common;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using Utilities.Enums;

namespace UI.Inventory
{
    public class StatSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [FormerlySerializedAs("statType")] [SerializeField] private StatType statSlotType;
        [SerializeField] private TextMeshProUGUI statName;
        [SerializeField] private TextMeshProUGUI statValue;

        private RectTransform _rect;
        private PlayerStats _playerStats;
        
        private PlayerStats PlayerStats => 
            _playerStats ??= GameManager.Instance.Player.Stats as PlayerStats;

        private void Awake()
        {
            _rect = GetComponent<RectTransform>();
        }

        private void Start()
        {
            //statName.text = LocalizedStatStrings.GetName(statSlotType);
            statName.text = statSlotType.ToString();
        }

        private void OnValidate()
        {
            //string sName = LocalizedStatStrings.GetName(statSlotType);
            string sName= statSlotType.ToString();
            gameObject.name = $"Stat - {sName}";
            statName.text = sName;
        }
        
        public void UpdateStatValue()
        {
            if(PlayerStats == null)
                return;
            
            Stat statToUpdate = _playerStats.GetStatByType(statSlotType);
            
            if(statToUpdate == null && statSlotType != StatType.ElementalDamage)
            {
                Debug.LogWarning($"Stat {statSlotType} not found in player stats");
                return;
            }

            float value = _playerStats.GetStatValue(statSlotType);
            statValue.text = Utils.IsPercentageStat(statSlotType) ? $"{value.ToString()}%" : value.ToString();

        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            //UiManager.Instance.StatTooltip.ShowTooltip(true, _rect, statSlotType);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            //UiManager.Instance.StatTooltip.ShowTooltip(false, null);
        }
    }
}