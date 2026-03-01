using UnityEngine;

namespace StateMachine.States.EnemyStates
{
    public class MoveState : EnemyState
    {
        public override void Update()
        {
            base.Update();
            
            Enemy.SetVelocity(new Vector2(Enemy.GetMoveSpeed(), Enemy.GetMoveSpeed()));
        }
    }
}