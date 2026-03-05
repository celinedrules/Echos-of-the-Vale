// Done
using Utilities.Enums;

namespace Core.Interfaces
{
    public interface IDamageable
    {
        public bool TakeDamage(int amount, int elementalDamage, ElementType elementType, Entity attacker);
    }
}