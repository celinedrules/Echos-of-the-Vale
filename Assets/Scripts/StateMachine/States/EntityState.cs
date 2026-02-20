using System;
using System.Collections.Generic;
using UnityEngine;

namespace StateMachine.States
{
    public abstract class EntityState
    {
        //private static readonly int AttackSpeedMultiplier = Animator.StringToHash("AttackSpeedMultiplier");
        
        protected StateMachine StateMachine;
        protected string AnimBoolName;
        protected Animator Animator;
        protected Rigidbody2D Rigidbody;
        //protected EntityStats EntityStats;

        private readonly HashSet<Action> _triggeredActions = new();

        protected void Initialize(StateMachine stateMachine, string animBoolName)
        {
            StateMachine = stateMachine;
            AnimBoolName = animBoolName;
        }

        public virtual void Enter()
        {
            _triggeredActions.Clear();
            Animator.SetBool(AnimBoolName, true);
        }

        public virtual void Update() => UpdateAnimationParams();
        public virtual void Exit()
        {
            Animator.SetBool(AnimBoolName, false);
        }

        protected virtual void UpdateAnimationParams() { }

        public void SyncAttackSpeed()
        {
            // float attackSpeed = Stats.OffenseStats.AttackSpeed.Value;
            // Animator.SetFloat(AttackSpeedMultiplier, attackSpeed);
        }
        
        protected bool HasAnimationFinished()
        {
            AnimatorStateInfo stateInfo = Animator.GetCurrentAnimatorStateInfo(0);
            return stateInfo.normalizedTime >= 1f && !Animator.IsInTransition(0);
        }
        
        protected void TriggerOnFrame(int frameIndex, System.Action action)
        {
            if (_triggeredActions.Contains(action))
                return;

            AnimatorClipInfo[] clipInfo = Animator.GetCurrentAnimatorClipInfo(0);
            if (clipInfo.Length == 0) return;

            AnimationClip clip = clipInfo[0].clip;
            AnimatorStateInfo stateInfo = Animator.GetCurrentAnimatorStateInfo(0);

            float totalFrames = clip.length * clip.frameRate;
            float targetNormalizedTime = frameIndex / totalFrames;

            if (stateInfo.normalizedTime >= targetNormalizedTime)
            {
                action?.Invoke();
                _triggeredActions.Add(action);
            }
        }
    }
}