// Done
using Core;
using Core.Interfaces;
using Data.DamageData;
using Stats;
using UnityEngine;
using Utilities.Enums;

namespace SkillSystem.Core
{
    public class SkillObjectBase : MonoBehaviour
    {
        [SerializeField] private GameObject onHitEffects;
        [SerializeField] protected LayerMask enemyLayerMask;
        [SerializeField] protected Transform targetCheck;
        [SerializeField] protected float targetCheckRadius = 1.0f;

        protected Rigidbody2D Rigidbody;
        protected Animator Animator;
        protected EntityStats PlayerStats;
        protected DamageScaleData DamageScaleData;
        protected ElementType UsedElement;
        protected bool TargetWasHit;
        protected Transform LastTarget;

        protected virtual void Awake()
        {
            Rigidbody = GetComponent<Rigidbody2D>();
            Animator = GetComponentInChildren<Animator>();
        }
        
        protected Transform FindClosestTarget()
        {
            Transform target = null;
            float closestDistance = Mathf.Infinity;

            foreach (Collider2D enemy in GetEnemiesAround(transform, 10))
            {
                float distance = Vector2.Distance(transform.position, enemy.transform.position);

                if (distance < closestDistance)
                {
                    target = enemy.transform;
                    closestDistance = distance;
                }
            }
            
            return target;
        }
        
        protected Collider2D[] GetEnemiesAround(Transform t, float radius)
        {
            return Physics2D.OverlapCircleAll(t.position, radius, enemyLayerMask);
        }
        
        protected void DamageEnemiesInRadius(Transform t, float radius)
        {
            foreach (Collider2D target in GetEnemiesAround(t, radius))
            {
                IDamageable damageable = target.GetComponent<IDamageable>(); 
                
                if(damageable == null)
                    continue;
                Debug.Log($"Damageing {target.name}");
                AttackData attackData = PlayerStats.GeAttackData(DamageScaleData);
                EntityStatusHandler targetStatusHandler = target.GetComponent<EntityStatusHandler>();
                ElementType elementType = attackData.ElementType;
                
                int physicalDamage = attackData.PhysicalDamage;
                int elementalDamage = attackData.ElementalDamage;
                
                UsedElement = elementType;

                // Not sure about null
                TargetWasHit = damageable.TakeDamage(physicalDamage, elementalDamage, elementType, null);
                
                if(elementType != ElementType.None)
                    targetStatusHandler.ApplyEffect(elementType, attackData.EffectData);

                if (TargetWasHit)
                {
                    LastTarget = target.transform;
                    Instantiate(onHitEffects, target.transform.position, Quaternion.identity);
                }
            }
        }
        
        protected virtual void OnDrawGizmos()
        {
            if(targetCheck == null)
                targetCheck = transform;
            
            Gizmos.DrawWireSphere(targetCheck.position, targetCheckRadius);
        }
    }
}