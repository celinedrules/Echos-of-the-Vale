using UnityEngine;

namespace StateMachine.States.EnemyStates
{
    public class DeathState : EnemyState
    {
        public override void Enter()
        {
            base.Enter();
            Enemy.GetComponent<Collider2D>().enabled = false;
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