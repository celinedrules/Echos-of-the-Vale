using Core;
using UnityEngine;
using Enemy;

namespace StateMachine.States.EnemyStates
{
    public class EnemyState : EntityState
    {
        private static readonly int FacingX = Animator.StringToHash("FacingX");
        private static readonly int FacingY = Animator.StringToHash("FacingY");
        
        protected EnemyController Enemy;
        
        public void Initialize(EnemyController enemy, StateMachine stateMachine, string animBoolName)
        {
            base.Initialize(stateMachine, animBoolName);
            
            Enemy = enemy;
            Rigidbody = enemy.Rigidbody;
            Animator = enemy.Animator;
            // Stats = enemy.Stats;
        }

        private void SetFacingFloats(float x, float y)
        {
            Animator.SetFloat(FacingX, x);
            Animator.SetFloat(FacingY, y);
        }

        private void SetFacingFloatsFromDirection()
        {
            (float x, float y) = Enemy.FacingDirection switch
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
    }
}