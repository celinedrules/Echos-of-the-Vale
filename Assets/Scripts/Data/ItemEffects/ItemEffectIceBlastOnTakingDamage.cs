// Done
using System;
using Core;
using Core.Interfaces;
using Data.DamageData;
using Player;
using UnityEngine;
using Utilities.Enums;

namespace Data.ItemEffects
{
    [CreateAssetMenu(fileName = "Item Effect - Ice Blast on Taking Damage",
        menuName = "Echos of the Vale/Item Data/Item Effect/Ice Blast", order = 0)]
    public class ItemEffectIceBlastOnTakingDamage : ItemEffectData
    {
        [SerializeField] private ElementalEffectData effectData;
        [SerializeField] private int iceDamage;
        [SerializeField] private LayerMask enemyLayerMask;
        [SerializeField] private float healthPercentTrigger = 0.25f;
        [SerializeField] private float cooldown;
        
        [Header("Effects Objects")]
        [SerializeField] private GameObject iceBlastEffects;
        [SerializeField] private GameObject onHitEffects;

        [NonSerialized] private float _lastTimeUsed;


        public override void ExecuteEffect()
        {
            bool noCooldown = Time.time >= _lastTimeUsed + cooldown;
            bool reachedThreshold = Player.Health.GetHealthPercentage() <= healthPercentTrigger;

            if (noCooldown && reachedThreshold)
            {
                Player.PlayerFx.CreateEffectOf(iceBlastEffects, Player.transform);
                _lastTimeUsed = Time.time;
                DamageEnemiesWithIce();
            }
        }

        private void DamageEnemiesWithIce()
        {
            Collider2D[] enemies = Physics2D.OverlapCircleAll(Player.transform.position, 1.5f, enemyLayerMask);

            foreach (Collider2D enemy in enemies)
            {
                IDamageable damageable = enemy.GetComponent<IDamageable>();

                if (damageable == null)
                    continue;

                bool enemyWasHit = damageable.TakeDamage(0, iceDamage, ElementType.Ice, Player);

                EntityStatusHandler statusHandler = enemy.GetComponent<EntityStatusHandler>();
                statusHandler?.ApplyEffect(ElementType.Ice, effectData);

                if (enemyWasHit)
                    Player.PlayerFx.CreateEffectOf(onHitEffects, enemy.transform);
            }
        }

        public override void Subscribe(PlayerController player)
        {
            Debug.Log("LastTimeUsed: " + _lastTimeUsed + " - Cooldown: " + cooldown);
            base.Subscribe(player);
            _lastTimeUsed -= cooldown;
            Player.Health.OnTakingDamage += ExecuteEffect;
        }

        public override void Unsubscribe()
        {
            base.Unsubscribe();
            Player.Health.OnTakingDamage -= ExecuteEffect;
            Player = null;
        }
    }
}