// Done
using System.Collections;
using Data.DamageData;
using UnityEngine;
using Utilities.Enums;

namespace Core
{
    public class EntityStatusHandler : MonoBehaviour
    {
        private Entity _entity;
        private EntityFx _entityFx;
        private EntityHealth _entityHealth;
        private ElementType _currentElementType = ElementType.None;
        private Coroutine _shockRoutine;
        
        [Header("Shock Effect Details")]
        [SerializeField] private GameObject lightningStrikeEffect;
        [SerializeField] private float currentCharge;
        [SerializeField] private float maximumCharge = 1.0f;
        
        private void Awake()
        {
            _entity = GetComponent<Entity>();
            _entityHealth = GetComponent<EntityHealth>();
            _entityFx = GetComponentInChildren<EntityFx>();
        }

        public void RemoveAllNegativeEffects()
        {
            StopAllCoroutines();
            _currentElementType = ElementType.None;
            _entityFx.StopAllEffects();
        }
        
        public void ApplyEffect(ElementType elementType, ElementalEffectData effectData)
        {
            if(elementType == ElementType.Ice && CanBeApplied(ElementType.Ice))
                ApplyEffect(elementType, effectData.ChillDuration, effectData.ChillSlowMultiplier);
            
            if(ElementType.Fire == elementType && CanBeApplied(ElementType.Fire))
                ApplyEffect(elementType, effectData.BurnDuration, effectData.BurnDamage);
            
            if(ElementType.Lightning == elementType && CanBeApplied(ElementType.Lightning))
                ApplyEffect(elementType, effectData.ShockDuration, effectData.ShockDamage, effectData.ShockCharge);
        }

        private void ApplyEffect(ElementType elementType, float duration, float effectStrength, float charge = 0)
        {
            float resistance = _entity.Stats.GetElementalResistance(elementType);

            if (elementType == ElementType.Ice)
            {
                float finalDuration = duration * (1 - resistance);
                StartCoroutine(IceEffectRoutine(finalDuration, effectStrength));
            }
            else if (elementType == ElementType.Fire)
            {
                int finalDamage = Mathf.RoundToInt(effectStrength * (1 - resistance));
                StartCoroutine(FireEffectRoutine(duration, finalDamage));
            }
            else if (elementType == ElementType.Lightning)
            {
                float lightningResistance = _entity.Stats.GetElementalResistance(ElementType.Lightning);
                float finalCharge = charge * (1 - lightningResistance);
                currentCharge += finalCharge;

                if (currentCharge >= maximumCharge)
                {
                    _entity.Stun(true);
                    
                    Instantiate(lightningStrikeEffect, transform.position, Quaternion.identity);
                    int finalDamage = Mathf.RoundToInt(effectStrength);
                    _entityHealth.ReduceHealth(finalDamage);
                    StopShockEffect();
                    return;
                }
                
                if (_shockRoutine != null)
                    StopCoroutine(_shockRoutine);
                
                _shockRoutine = StartCoroutine(ShockEffectRoutine(duration));
            }
        }

        private void StopShockEffect()
        {
            _currentElementType = ElementType.None;
            currentCharge = 0;
            _entityFx.StopAllEffects();
        }
        
        private IEnumerator ShockEffectRoutine(float duration)
        {
            _currentElementType = ElementType.Lightning;
            _entityFx.PlayStatusEffect(duration, _currentElementType);
            yield return new WaitForSeconds(duration);
            StopShockEffect();
        }
        
        private IEnumerator FireEffectRoutine(float duration, float totalDamage)
        {
            _currentElementType = ElementType.Fire;
            _entityFx.PlayStatusEffect(duration, _currentElementType);

            int ticksPerSecond = 2;
            int tickCount = Mathf.RoundToInt(ticksPerSecond * duration);

            int damagePerTick = Mathf.RoundToInt(totalDamage / tickCount);
            float tickInterval = 1.0f / ticksPerSecond;

            for (int i = 0; i < tickCount; i++)
            {
                _entityHealth.ReduceHealth(damagePerTick);
                yield return new WaitForSeconds(tickInterval);
            }

            _currentElementType = ElementType.None;
        }

        private IEnumerator IceEffectRoutine(float duration, float effectMultiplier)
        {
            _entity.SlowDownEntity(duration, effectMultiplier);
            _currentElementType = ElementType.Ice;
            _entityFx.PlayStatusEffect(duration, _currentElementType);

            yield return new WaitForSeconds(duration);

            _currentElementType = ElementType.None;
        }

        private bool CanBeApplied(ElementType elementType)
        {
            if (elementType == ElementType.Lightning && _currentElementType == ElementType.Lightning)
                return true;
            
            return _currentElementType == ElementType.None;
        }
    }
}