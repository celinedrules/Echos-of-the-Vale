using Core;
using Player;
using Unity.VisualScripting.Antlr3.Runtime.Misc;

namespace StateMachine.States.PlayerStates
{
    public class PlayerState : EntityState
    {
        protected PlayerController Player;
        protected PlayerInputHandler Input;

        public void Initialize(PlayerController player, StateMachine stateMachine, string animBoolName)
        {
            base.Initialize(stateMachine, animBoolName);
            
            Player = player;
            Rigidbody = Player.Rigidbody;
            Animator = Player.Animator;
            //Stats =  Player.Stats;
            Input = Player.Input;
        }

        public override void Update()
        {
            base.Update();
            
            if(Input.Player.Attack.WasPressedThisFrame())
                StateMachine.ChangeState(Player.BasicAttackState);
        }

        protected void SetFacingFloats(float x, float y)
        {
            Animator.SetFloat("FacingX", x);
            Animator.SetFloat("FacingY", y);
        }

        protected void SetFacingFloatsFromDirection()
        {
            var (x, y) = Player.FacingDirection switch
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
        }
    }
}