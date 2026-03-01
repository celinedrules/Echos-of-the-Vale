using Managers;
using UnityEngine;
using Utilities;

namespace StateMachine.States.EnemyStates
{
    public class MoveState : EnemyState
    {
        private Timer _moveTimer;
        private bool _firstFrame;

        public override void Enter()
        {
            base.Enter();
            _firstFrame = true;
            SetupTimers();

            Vector2 direction = Enemy.GetRandomDirection();

            // If near the boundary, point back toward center
            Vector2 toCenter = Enemy.OriginalPosition - Enemy.Rigidbody.position;
            if (toCenter.magnitude >= Enemy.RandomMoveRadius * 0.7f)
            {
                direction = toCenter.normalized;
            }

            Enemy.SetVelocity(direction * Enemy.GetMoveSpeed());
        }

        private void SetupTimers()
        {
            _moveTimer = TimerManager.Instance.CreateTimer(Enemy.MoveTime,
                () => { StateMachine.ChangeState(Enemy.IdleState); });
        }

        public override void Update()
        {
            base.Update();

            if (_firstFrame)
            {
                _firstFrame = false;
                return;
            }

            Vector2 currentPos = Enemy.Rigidbody.position;
            float distance = Vector2.Distance(currentPos, Enemy.OriginalPosition);

            if (distance >= Enemy.RandomMoveRadius)
            {
                StateMachine.ChangeState(Enemy.IdleState);
            }
        }

        public override void Exit()
        {
            base.Exit();
            _moveTimer.Cancel();
        }
    }
}