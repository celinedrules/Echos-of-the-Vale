// Done
using UnityEngine;

namespace StateMachine.States.EnemyStates
{
    public class DeathState : EnemyState
    {
        public override void Enter()
        {
            base.Enter();
            
            Collider2D[] colliders = Enemy.GetComponentsInChildren<Collider2D>();
            
            foreach (Collider2D collider in colliders)
                collider.enabled = false;
            
            StateMachine.CanChangeState = false;
        }

        public override void Update()
        {
            base.Update();

            if (HasAnimationFinished())
                Enemy.Death();
        }
    }
}