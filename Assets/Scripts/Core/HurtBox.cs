using System;
using Core.Interfaces;
using UnityEngine;
using Utilities.Enums;

namespace Core
{
    public class HurtBox : MonoBehaviour
    {
        // [SerializeField] private DamageScaleData basicAttackScale;
        
        private Entity _targetEntity;
        private Entity _sourceEntity;
        
        public IDamageable Owner { get; private set; }

        private void Awake()
        {
            Owner = GetComponentInParent<IDamageable>();
            _targetEntity = GetComponentInParent<Entity>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            WeaponHitBox hitBox = other.GetComponent<WeaponHitBox>();
            
            if(!hitBox)
                return;
            
            if(Owner == null)
                return;
            
            Debug.Log("Hurtbox hit something");
            
            _sourceEntity = hitBox.Owner;
            
            if(!_sourceEntity)
                return;

            PerformAttack();
        }

        private void PerformAttack()
        {
            //AttackData attackData = _sourceEntity.Stats.GeAttackData(basicAttackScale);
            //EntityStatusHandler targetStatusHandler = _targetEntity?.GetComponent<EntityStatusHandler>();

            //int physicalDamage = attackData.PhysicalDamage;
            //int elementalDamage = attackData.ElementalDamage;
            bool tookDamage;

            //ElementType elementType = attackData.ElementType;
            
            //tookDamage = Owner.TakeDamage(physicalDamage, elementalDamage, elementType, _sourceEntity);
            tookDamage = Owner.TakeDamage(1, 1, ElementType.None, _sourceEntity);

            Debug.Log(Owner + "Took damage");
            
            // if(elementType != ElementType.None)
            //     targetStatusHandler?.ApplyEffect(elementType, attackData.EffectData);

            if (tookDamage)
            {
                Debug.Log("Took damage");
                // _sourceEntity?.OnDealtPhysicalDamage(physicalDamage);
                // _sourceEntity?.EntityFx.SetElementColor(elementType);
                // _sourceEntity?.EntityFx?.CreateHitEffect(transform, attackData.IsCritical);
                //
                // if(_targetEntity is Player.Player)
                //     FusionAudioManager.Instance.PlayAudio(PlayerAudioId.PlayerHitAttack);
                // else if (_targetEntity is Enemy.Enemy enemy)
                //     FusionAudioManager.Instance.PlayAudio(EnemyAudioId.EnemyHitAttack, enemy.AudioEmitter.Source);
            }
        }
    }
}