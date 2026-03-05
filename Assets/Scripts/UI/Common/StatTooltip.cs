// Done
using System.Collections.Generic;
using Managers;
using Stats;
using TMPro;
using UnityEngine;
using Utilities.Enums;

namespace UI.Common
{
    public class StatTooltip : Tooltip
    {
        [SerializeField] private TextMeshProUGUI statTooltipText;
        
        private PlayerStats PlayerStats =>  GameManager.Instance.Player.Stats as PlayerStats;

        protected override void Awake()
        {
            base.Awake();
            // _playerStats = GameManager.Instance.Player.Stats as PlayerStats;
        }

        public void ShowTooltip(bool show, RectTransform targetRect, StatType statType)
        {
            base.ShowTooltip(show, targetRect);
            statTooltipText.text = GetStatTextByType(statType);
        }

        public string GetStatTextByType(StatType type)
        {
            return type.ToString();
        }
        
        private Dictionary<string, object> GetArgsForStat(StatType type)
        {
            return type switch
            {
                StatType.Armor => new Dictionary<string, object>
                {
                    { "mitigation", PlayerStats.GetArmorMitigation(0) * 100 }
                },
                // Add more stats with dynamic values as needed
                _ => null
            };
        }
    }
}