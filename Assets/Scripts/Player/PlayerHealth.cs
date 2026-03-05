// Done
using Core;
using UnityEngine;
using Utilities.Enums;

namespace Player
{
    public class PlayerHealth : EntityHealth
    {
        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.N))
                Die();
            
            if(Input.GetKeyDown(KeyCode.M))
                TakeDamage(10, 0, ElementType.None, null);
        }

        protected override void HandleDamageKnockback(Entity attacker, int finalDamage)
        {
            bool heavy = IsHeavyDamage(finalDamage);
            
            KnockbackSettings kb = heavy ? heavyDamageKnockback : normalDamageKnockback;
            
            if (attacker)
            {
                Vector2 direction = ((Vector2)transform.position - (Vector2)attacker.transform.position).normalized;
                entity.Knockback(direction * kb.power, kb.duration);
            }
        }
    }
}