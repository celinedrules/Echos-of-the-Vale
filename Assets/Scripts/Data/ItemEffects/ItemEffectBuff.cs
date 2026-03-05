// Done
using System;
using Player;
using Sirenix.OdinInspector;
using Stats;
using UnityEngine;

namespace Data.ItemEffects
{
    [CreateAssetMenu(fileName = "Item Effect - Buff", menuName = "Echos of the Vale/Item Data/Item Effect/Buff Effect",
        order = 0)]
    public class ItemEffectBuff : ItemEffectData
    {
        [HorizontalGroup("Wrapper", MarginLeft = 5)]
        [BoxGroup("Wrapper/Buffs to Apply")]
        [SerializeField]
        [ListDrawerSettings(ShowItemCount = false, ShowFoldout = false)]
        [LabelText("Effects")]
        private BuffEffectData[] buffsToApply;

        [BoxGroup("Wrapper/Buffs to Apply")]
        [Space(7)]
        [SerializeField] private float duration;

        [BoxGroup("Wrapper/Buffs to Apply")]
        [SerializeField] private string source = Guid.NewGuid().ToString();

        private PlayerStats _playerStats;

        public override bool CanBeUsed(PlayerController player)
        {
            _playerStats = (PlayerStats)player.Stats;

            if (!_playerStats.CanApplyBuff(source))
                return false;

            Player = player;
            return true;
        }

        public override void ExecuteEffect()
        {
            _playerStats.ApplyBuff(buffsToApply, duration, source);
            Player = null;
        }
    }
}