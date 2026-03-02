using Core.Interfaces;
using UnityEngine;
using Utilities.Enums;

namespace Core
{
    public class EntityHealth : MonoBehaviour, IDamageable
    {
        public virtual bool TakeDamage(int amount, int elementalDamage, ElementType elementType, Entity attacker)
        {
            return false;
        }
    }
}