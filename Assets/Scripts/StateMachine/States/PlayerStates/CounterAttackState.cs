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
            
            _countered = false;
            
            _counterAttackTimer = TimerManager.Instance.CreateTimer(_playerCounterAttack.CounterDuration, () =>
            {
                if (!_countered || HasAnimationFinished())
                    StateMachine.ChangeState(Player.IdleState);
            });
        }

        public override void Update()
        {
            base.Update();
            //Player.SetVelocity(0, Rigidbody.linearVelocity.y);
        }

        public override void Exit()
        {
            base.Exit();
            _playerCounterAttack.OnCounterSuccess -= HandleCounterSuccess;
            _playerCounterAttack.SetCounterWindow(false);
            Player.SwordParent.SetActive(false);
        }

        private void HandleCounterSuccess()
        {
            _countered = true;
            Animator.SetTrigger(CounterAttackTrigger);
            StateMachine.ChangeState(Player.IdleState);
        }
    }
}