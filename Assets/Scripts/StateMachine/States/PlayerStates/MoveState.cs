using UnityEngine;

namespace StateMachine.States.PlayerStates
{
    public class MoveState : PlayerState
    {
        public override void Update()
        {
            base.Update();
            
            if(Input.MoveInput == Vector2.zero)
                StateMachine.ChangeState(Player.IdleState);
            
            Player.SetVelocity(new Vector2(Input.MoveInput.x * Player.MoveSpeed, Input.MoveInput.y * Player.MoveSpeed));
        }
    }
}