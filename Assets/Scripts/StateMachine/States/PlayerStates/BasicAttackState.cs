using Core;
using Managers;
using UnityEngine;
using Utilities;

namespace StateMachine.States.PlayerStates
{
    public class BasicAttackState : PlayerState
    {
        private static readonly int BasicAttackIndex = Animator.StringToHash("BasicAttackIndex");

        private const int StartComboIndex = 1;
        private const int MaxCombo = 3;
        private int _comboIndex = 1;

        private Direction _attackDirection;

        private bool _comboAttachedQueued;

        private Timer _attackVelocityTimer;
        private Timer _resetComboTimer;
        
        public override void Enter()
        {
            base.Enter();

            _comboAttachedQueued = false;
            
            SyncAttackSpeed();
            
            _attackDirection = Input.MoveInput.x != 0 ? (Direction)Input.MoveInput.x : Player.FacingDirection;
            
            Animator.SetInteger(BasicAttackIndex, _comboIndex);
            
            if (Player.swordParent)
                Player.swordParent.SetActive(true);
            
            if(Player.swordAnimator)
            {
                Player.swordAnimator.Play("Attack", 0, 0f);
            }

            ApplyAttackVelocity();
            SetupTimers();
        }
        
        public override void Update()
        {
            base.Update();

            if (Input.Player.Attack.WasPressedThisFrame())
                QueueNextAttack();

            if (!HasAnimationFinished())
                return;
            
            HandleStateChange();
        }
        
        public override void Exit()
        {
            base.Exit();
            
            if (Player.swordParent)
                Player.swordParent.SetActive(false);

            SetNextComboIndex();

            _attackVelocityTimer?.Cancel();
        }
        
        private void SetupTimers()
        {
            _attackVelocityTimer = TimerManager.Instance.CreateTimer(Player.AttackVelocityDuration, OnAttackVelocityEnd);

            if (_resetComboTimer != null && !_resetComboTimer.IsCompleted)
                _resetComboTimer.Restart();
            else
                _resetComboTimer = TimerManager.Instance.CreateTimer(Player.ComboResetDuration, ResetComboIndex);
        }
        
        private void HandleStateChange()
        {
            if (_comboAttachedQueued)
            {
                Animator.SetBool(AnimBoolName, false);
                Player.EnterAttackStateWithDelay();
            }
            else
            {
                StateMachine.ChangeState(Player.IdleState);
            }
        }
        
        private void ApplyAttackVelocity()
        {
            Vector2 attackVelocity = Player.AttackVelocity[_comboIndex - 1];
            Player.SetVelocity(attackVelocity.x * (int)_attackDirection, attackVelocity.y);
        }

        private void OnAttackVelocityEnd()
        {
            Rigidbody.linearVelocity = new Vector2(0, Rigidbody.linearVelocity.y);
        }

        private void SetNextComboIndex()
        {
            _comboIndex++;

            if (_comboIndex > MaxCombo)
                ResetComboIndex();
        }

        private void ResetComboIndex()
        {
            _comboIndex = StartComboIndex;
        }

        private void QueueNextAttack()
        {
            if(_comboIndex < MaxCombo)
                _comboAttachedQueued = true;
        }
    }
}