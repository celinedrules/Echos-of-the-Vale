// Done
using Core;
using Managers;
using Player;
using UnityEngine;

namespace StateMachine.States.PlayerStates
{
    public class PlayerState : EntityState
    {
        private static readonly int FacingX = Animator.StringToHash("FacingX");
        private static readonly int FacingY = Animator.StringToHash("FacingY");
        protected PlayerController Player;
        protected PlayerInputHandler Input;

        public void Initialize(PlayerController player, StateMachine stateMachine, string animBoolName)
        {
            base.Initialize(stateMachine, animBoolName);
            
            Player = player;
            Rigidbody = Player.Rigidbody;
            Animator = Player.Animator;
            //Stats =  Player.Stats;
            Input = Player.InputHandler;
        }

        public override void Update()
        {
            base.Update();
            
            if(Input.Player.Attack.WasPressedThisFrame())
                StateMachine.ChangeState(Player.BasicAttackState);
            
            if(Input.Player.CounterAttack.WasPressedThisFrame() && StateMachine.CurrentState != Player.CounterAttackState)
                StateMachine.ChangeState(Player.CounterAttackState);

            if (Input.Player.Dash.WasPressedThisFrame() && CanDash())
            {
                SkillManager.Instance.Dash.SetSkillOnCooldown();
                StateMachine.ChangeState(Player.DashState);
            }
        }

        private void SetFacingFloats(float x, float y)
        {
            Animator.SetFloat(FacingX, x);
            Animator.SetFloat(FacingY, y);

            if (!Player.SwordAnimator)
                return;
            
            Player.SwordAnimator.SetFloat(FacingX, x);
            Player.SwordAnimator.SetFloat(FacingY, y);
        }

        private void SetFacingFloatsFromDirection()
        {
            (float x, float y) = Player.FacingDirection switch
            {
                Direction.Up    => ( 0f,  1f),
                Direction.Down  => ( 0f, -1f),
                Direction.Left  => (-1f,  0f),
                Direction.Right => ( 1f,  0f),
                _ => (0f, -1f)
            };
            SetFacingFloats(x, y);
        }
        
        protected override void UpdateAnimationParams()
        {
            base.UpdateAnimationParams();
            SetFacingFloatsFromDirection();
        }
        
        private bool CanDash()
        {
            if (!SkillManager.Instance.Dash.CanUseSkill())
            {
                Debug.Log("Can't use skill");
                return false;
            }

            // if (Player.HitWall)
            //     return false;

            return StateMachine.CurrentState != Player.DashState;
            // return StateMachine.CurrentState != Player.DashState &&
            //        StateMachine.CurrentState != Player.DomainExpansionState;
        }
    }
}