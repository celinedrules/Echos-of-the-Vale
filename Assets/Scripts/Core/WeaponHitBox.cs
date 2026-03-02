using System;
using Enemy;
using Player;
using UnityEngine;

namespace Core
{
    public class WeaponHitBox : MonoBehaviour
    {
        [field: SerializeField] public int Damage { get; set; }
        [field: SerializeField] public Entity Owner { get; set; }

        private bool _hitSomething;

        private void OnEnable()
        {
            _hitSomething = false;
        }

        // private void OnDisable()
        // {
        //     if (!_hitSomething && Owner && FusionAudioManager.Instance)
        //     {
        //         if (Owner is PlayerController)
        //             FusionAudioManager.Instance.PlayAudio(PlayerAudioId.PlayerMissAttack,
        //                 speed: Random.Range(0.95f, 1.1f));
        //         else if (Owner is EnemyController enemy)
        //             FusionAudioManager.Instance.PlayAudio(EnemyAudioId.EnemyMissAttack, enemy.AudioEmitter.Source);
        //     }
        // }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if(other.GetComponent<HurtBox>())
            {
                Debug.Log("Hitbox hit something");
                _hitSomething = true;
                
                // if(Owner is EnemyController enemy)
                //     FusionAudioManager.Instance.StopAudio(enemy.AudioEmitter.Source);
            }
        }
    }
}