// Done
using Core;
using UnityEngine;

namespace Enemy
{
    public class EnemyFx : EntityFx
    {
        [Header("Counter Attack Window")]
        [SerializeField] private GameObject attackAlert;
        
        public void EnableAttackAlert(bool enable) => attackAlert.SetActive(enable);
    }
}