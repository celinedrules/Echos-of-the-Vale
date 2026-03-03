using System;
using Core.Interfaces;
using Enemy;
using Player;
using StateMachine.States.EnemyStates;
using StateMachine.States.PlayerStates;
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

            if (hitBox)
            {
                HandleWeaponHit(hitBox);
                return;
            }

            HandleContactDamage(other);
            
            // if(Owner == null)
            //     return;
            //
            // _sourceEntity = hitBox.Owner;
            //  
            // if(!_sourceEntity)
            //     return;

            //Debug.Log(gameObject.transform.root.name);
            
            // if(_targetEntity is PlayerController player)
            // {
            //     if(player.CurrentState == player.BasicAttackState)
            //     {
            //         Debug.Log("Perform Attack");
            //         PerformAttack();
            //     }
            // }
            //PerformAttack();
        }
        
        private void HandleWeaponHit(WeaponHitBox hitBox)
        {
            if (Owner == null)
                return;

            _sourceEntity = hitBox.Owner;

            if (!_sourceEntity)
                return;

            PerformAttack();
        }
        
        private void HandleContactDamage(Collider2D other)
        {
            // Only the player should take contact damage from enemies
            if (_targetEntity is not PlayerController player)
                return;

            if (Owner == null)
                return;

            // Check if the collider belongs to an enemy
            Entity contactEntity = other.GetComponentInParent<Entity>();

            if (contactEntity is not EnemyController)
                return;

            _sourceEntity = contactEntity;
            
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
            
            // if(elementType != ElementType.None)
            //     targetStatusHandler?.ApplyEffect(elementType, attackData.EffectData);

            if (tookDamage)
            {
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