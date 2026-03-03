using Managers;
using Player;
using UnityEngine;
using Utilities;

namespace StateMachine.States.PlayerStates 
{
    public class CounterAttackState : PlayerState
    {
        private static readonly int CounterAttackTrigger = Animator.StringToHash("CounterAttackTrigger");

        private Timer _counterAttackTimer;
        private PlayerCounterAttack _playerCounterAttack;
        private bool _countered;

        public override void Enter()
        {
            Debug.Log("CounterAttackState entered");
            base.Enter();
            
            if(!_playerCounterAttack)
                _playerCounterAttack = Player.GetComponentInChildren<PlayerCounterAttack>();

            _playerCounterAttack.OnCounterSuccess += HandleCounterSuccess;
            _playerCounterAttack.SetCounterWindow(true);
            Player.SwordParent.SetActive(true);
            if (Player.SwordAnimator)
            {
                Player.SwordAnimator.SetBool("Counter", true);
                //Player.SwordAnimator.enabled = false;
            }
            //Player.SwordAnimator.playbackTime = 0f;
            //Player.SwordAnimator.Play("Attack", 0, 0f);
            
            _countered = false;
            
            _counterAttackTimer = TimerManager.Instance.CreateTimer(_playerCounterAttack.CounterDuration, () =>
            {
                if (!_countered || HasAnimationFinished())
                {
                    StateMachine.ChangeState(Player.IdleState);
                    Player.SwordAnimator.SetBool("Counter", false);
                }
            });
        }

        public override void Update()
        {
            base.Update();
            //Player.SetVelocity(0, Rigidbody.linearVelocity.y);
            
            if (_countered && HasAnimationFinished())
                StateMachine.ChangeState(Player.IdleState);
        }

        public override void Exit()
        {
            base.Exit();
            _playerCounterAttack.OnCounterSuccess -= HandleCounterSuccess;
            _playerCounterAttack.SetCounterWindow(false);
            if (Player.SwordAnimator)
            {
                Player.SwordAnimator.SetBool("Counter", false);
                //Player.SwordAnimator.enabled = true;
            }
            Player.SwordParent.SetActive(false);
        }

        private void HandleCounterSuccess()
        {
            _countered = true;
            _counterAttackTimer?.Cancel();
            if (Player.SwordAnimator)
            {
                Debug.Log("Enable Counter Attack Animator");
                Player.SwordAnimator.enabled = true;
                Player.SwordAnimator.SetBool("Counter", false);
                Player.SwordAnimator.Play("Attack", 0, 0f);
            }
            Animator.SetTrigger(CounterAttackTrigger);
        }
    }
}