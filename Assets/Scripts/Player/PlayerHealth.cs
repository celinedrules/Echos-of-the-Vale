using Core;
using UnityEngine;

namespace Player
{
    public class PlayerHealth : EntityHealth
    {
        protected override void HandleDamageKnockback(Entity attacker, int finalDamage)
        {
            // bool heavy = IsHeavyDamage(finalDamage);
            KnockbackSettings kb = normalDamageKnockback;
            // KnockbackSettings kb = heavy ? HeavyDamageKnockback : NormalDamageKnockback;
            if (attacker)
            {
                Vector2 direction = ((Vector2)transform.position - (Vector2)attacker.transform.position).normalized;
                entity.Knockback(direction * kb.power, kb.duration);
            }
        }
    }
}