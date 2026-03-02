using Managers;
using Utilities;

namespace StateMachine.States.EnemyStates
{
    public class AttackState : EnemyState
    {
        private Timer _stunTimer;

        public override void Enter()
        {
            base.Enter();
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