using Managers;
using Player;
using UnityEngine;

namespace Data.ItemEffects
{
    [CreateAssetMenu(fileName = "Item Effect - Heal", menuName = "Echos of the Vale/Item Data/Item Effect/Heal Effect")]
    public class ItemEffectHeal : ItemEffectData
    {
        [SerializeField] private float healPercent = 0.1f;
        
        public override void ExecuteEffect()
        {
            // PlayerController player = GameManager.Instance.Player;
            // float healAmount = player.Stats.GetMaxHealth() * healPercent;
            // player.Health.IncreaseHealth(Mathf.RoundToInt(healAmount));
        }
    }
}