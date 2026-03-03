using Managers;
using UnityEngine;
using Utilities;

namespace StateMachine.States.EnemyStates
{
    public class AttackState : EnemyState
    {
        private Timer _stunTimer;

        public override void Enter()
        {
            base.Enter();
            Enemy.SetVelocity(Vector2.zero);
            Enemy.IsAttacking = true;
            SyncAttackSpeed();
        }

        public override void Update()
        {
            base.Update();
            
            TriggerOnFrame(Enemy.StartStunnedFrame, EnableStun);
            
            if(!HasAnimationFinished())
                return;
            
            HandleStateChange();
        }

        public override void Exit()
        {
            base.Exit();
            Enemy.IsAttacking = false;
            _stunTimer?.Cancel();
        }

        private void EnableStun()
        {
            Enemy.CanBeStunned = true;
            _stunTimer = TimerManager.Instance.CreateTimer(Enemy.StunnedFrameDuration, DisableStun);
        }

        private void DisableStun()
        {
            Enemy.CanBeStunned = false;
        }

        private void HandleStateChange()
        {
            StateMachine.ChangeState(Enemy.BattleState);
        }
    }
}