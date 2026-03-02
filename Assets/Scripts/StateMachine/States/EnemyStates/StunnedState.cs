using Managers;
using Utilities;

namespace StateMachine.States.EnemyStates
{
    public class StunnedState : EnemyState
    {
        private Timer _stunnedTimer;
        
        public bool KnockBack { get; set; }

        public override void Enter()
        {
            base.Enter();
        }

        private void SetupTimers()
        {
            _stunnedTimer = TimerManager.Instance.CreateTimer(Enemy.StunnedFrameDuration,
                () => { StateMachine.ChangeState(Enemy.IdleState); });
        }

        public override void Exit()
        {
            base.Exit();
            
            _stunnedTimer.Cancel();
            KnockBack = false;
        }
    }
}