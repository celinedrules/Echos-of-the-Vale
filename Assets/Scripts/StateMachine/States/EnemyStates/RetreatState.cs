using UnityEngine;

namespace StateMachine.States.EnemyStates
{
    public class RetreatState : EnemyState
    {
        private const float ArrivalThreshold = 0.2f;

        public override void Enter()
        {
            base.Enter();
            MoveTowardOrigin();
        }

        public override void Update()
        {
            base.Update();

            float distanceToOrigin = Vector2.Distance(Enemy.Rigidbody.position, Enemy.OriginalPosition);

            if (distanceToOrigin <= ArrivalThreshold)
            {
                Enemy.SetVelocity(Vector2.zero);
                StateMachine.ChangeState(Enemy.IdleState);
                return;
            }

            MoveTowardOrigin();
        }

        private void MoveTowardOrigin()
        {
            Vector2 direction = (Enemy.OriginalPosition - Enemy.Rigidbody.position).normalized;
            Enemy.SetVelocity(direction * Enemy.GetMoveSpeed());
        }

        public override void Exit()
        {
            base.Exit();
            Enemy.SetVelocity(Vector2.zero);
            Enemy.PlayerTransform = null;
        }
    }
}