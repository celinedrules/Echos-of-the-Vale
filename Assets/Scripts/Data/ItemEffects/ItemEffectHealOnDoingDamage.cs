// Done
using Player;
using UnityEngine;

namespace Data.ItemEffects
{
    [CreateAssetMenu(fileName = "Item Effect - Heal On Doing Physical Damage",
        menuName = "Echos of the Vale/Item Data/Item Effect/Heal On Doing Damage", order = 0)]
    public class ItemEffectHealOnDoingDamage : ItemEffectData
    {
        [SerializeField] private float percentHealOnAttack = .2f;

        public override void Subscribe(PlayerController player)
        {
            base.Subscribe(player);
            Player.OnDoingPhysicalDamage += HealOnDoingDamage;
        }

        public override void Unsubscribe()
        {
            base.Unsubscribe();
            Player.OnDoingPhysicalDamage -= HealOnDoingDamage;
            Player = null;
        }

        private void HealOnDoingDamage(int damage) =>
            Player.Health.IncreaseHealth(Mathf.RoundToInt(damage * percentHealOnAttack));
    }
}