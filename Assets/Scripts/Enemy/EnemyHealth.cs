using Core;
using Player;
using Utilities.Enums;

namespace Enemy
{
    public class EnemyHealth : EntityHealth
    {
        private EnemyController _enemy;

        private void Awake() => _enemy = GetComponent<EnemyController>();
        
        public override bool TakeDamage(int amount, int elementalDamage, ElementType elementType, Entity attacker)
        {
            if(!CanTakeDamage)
                return false;

            bool tookDamage = base.TakeDamage(amount, elementalDamage, elementType, attacker);
            
            if(!tookDamage)
                return false;

            if (attacker is PlayerController player)
            {
                _enemy.PlayerTransform = player.transform;
                _enemy.TryEnterBattleState();
            }
            
            return true;
        }

        protected override void Die()
        {
            base.Die();
            //QuestManager.Instance.AddProgress(_enemy.QuestTargetId);
        }
    }
}