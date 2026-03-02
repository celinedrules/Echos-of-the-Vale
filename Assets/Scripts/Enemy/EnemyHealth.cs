using Core;
using Utilities.Enums;

namespace Enemy
{
    public class EnemyHealth : EntityHealth
    {
        public override bool TakeDamage(int amount, int elementalDamage, ElementType elementType, Entity attacker)
        {
            return false;
        }
    }
}