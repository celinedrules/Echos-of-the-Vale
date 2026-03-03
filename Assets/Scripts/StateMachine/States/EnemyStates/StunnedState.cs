using Managers;
using UnityEngine;
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
            // Enemy.EnemyFx.EnableAttackAlert(false);
            Enemy.CanBeStunned = false;

            //SetupTimers();

            if (KnockBack)
            {
                Vector2 direction = ((Vector2)Enemy.transform.position - (Vector2)Enemy.PlayerTransform.position).normalized;
                Vector2 knockbackPower = direction * Enemy.StunnedVelocity;
                Enemy.Knockback(knockbackPower, Enemy.StunnedFrameDuration);
            }
        }

        public override void Update()
        {
            base.Update();
            
            if(HasAnimationFinished())
                StateMachine.ChangeState(Enemy.IdleState);
                
        }

        // private void SetupTimers()
        // {
        //     _stunnedTimer = TimerManager.Instance.CreateTimer(Enemy.StunnedFrameDuration,
        //         () => { StateMachine.ChangeState(Enemy.IdleState); });
        // }

        public override void Exit()
        {
            base.Exit();
            
            //_stunnedTimer?.Cancel();
            KnockBack = false;
        }
    }
}