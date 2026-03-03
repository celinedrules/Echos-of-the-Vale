using Managers;
using Player;
using UnityEngine;
using Utilities;

namespace StateMachine.States.PlayerStates 
{
    public class CounterAttackState : PlayerState
    {
        private static readonly int CounterAttackTrigger = Animator.StringToHash("CounterAttackTrigger");
        private static readonly int CounterAttackPerformedState = Animator.StringToHash("PlayerCounterAttackPerformed");
        private static readonly int CounterPrep = Animator.StringToHash("CounterPrep");
        private static readonly int Counter = Animator.StringToHash("Counter");
        private static readonly int Attack = Animator.StringToHash("Attack");

        private Timer _counterAttackTimer;
        private PlayerCounterAttack _playerCounterAttack;
        private bool _countered;

        public override void Enter()
        {
            base.Enter();
            
            if(!_playerCounterAttack)
                _playerCounterAttack = Player.GetComponentInChildren<PlayerCounterAttack>();

            _playerCounterAttack.OnCounterSuccess += HandleCounterSuccess;
            _playerCounterAttack.SetCounterWindow(true);
            Player.SwordParent.SetActive(true);
         
            if (Player.SwordAnimator)
            {
                Player.SwordAnimator.Play(CounterPrep, 0, 0f);
                Player.SwordAnimator.SetBool(Counter, true);
            }
            
            _countered = false;
            
            _counterAttackTimer = TimerManager.Instance.CreateTimer(_playerCounterAttack.CounterDuration, () =>
            {
                if (!_countered || HasAnimationFinished())
                {
                    StateMachine.ChangeState(Player.IdleState);
                    Player.SwordAnimator.SetBool(Counter, false);
                }
            });
        }

        public override void Update()
        {
            base.Update();

            UpdateAnimationParams();

            if (_countered && HasAnimationFinished(CounterAttackPerformedState))
                StateMachine.ChangeState(Player.IdleState);
        }

        public override void Exit()
        {
            base.Exit();
            _playerCounterAttack.OnCounterSuccess -= HandleCounterSuccess;
            _playerCounterAttack.SetCounterWindow(false);
            
            if (Player.SwordAnimator)
                Player.SwordAnimator.SetBool(Counter, false);

            Player.SwordParent.SetActive(false);
        }

        private void HandleCounterSuccess()
        {
            _countered = true;
            _counterAttackTimer?.Cancel();
            
            if (Player.SwordAnimator)
            {
                Player.SwordAnimator.SetBool(Counter, false);
                Player.SwordAnimator.Play(Attack, 0, 0f);
            }

            Animator.SetTrigger(CounterAttackTrigger);
        }
    }
}