using Managers;
using UnityEngine;
using Utilities;

namespace StateMachine.States.EnemyStates
{
    public class IdleState : EnemyState
    {
        private Timer _idleTimer;

        public override void Enter()
        {
            base.Enter();
            Enemy.SetVelocity(Vector2.zero);
            SetupTimers();
        }

        private void SetupTimers()
        {
                _idleTimer = TimerManager.Instance.CreateTimer(Enemy.IdleTime,
                    () => { StateMachine.ChangeState(Enemy.MoveState); });
        }
        
        public override void Exit()
        {
            base.Exit();
            _idleTimer.Cancel();
        }
    }
}