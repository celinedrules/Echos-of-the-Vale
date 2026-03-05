// Done
using UnityEngine;

namespace StateMachine.States.PlayerStates
{
    public class IdleState : PlayerState
    {
        public override void Enter()
        {
            base.Enter();
            Player.SetVelocity(Vector2.zero);
        }

        public override void Update()
        {
            base.Update();
            
            if (Player.IsKnockedBack)
                return;
            
            // No input → stay idle
            if (Input.MoveInput == Vector2.zero)
                return;

            // Otherwise, movement happened -> go to MoveState
            StateMachine.ChangeState(Player.MoveState);
        }
    }
}