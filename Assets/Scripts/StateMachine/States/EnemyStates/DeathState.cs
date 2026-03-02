using UnityEngine;

namespace StateMachine.States.EnemyStates
{
    public class DeathState : EnemyState
    {
        public override void Enter()
        {
            Animator.enabled = false;
            Enemy.GetComponent<Collider2D>().enabled = false;
            StateMachine.CanChangeState = false;
        }
    }
}