using SkillSystem.Core;
using UnityEngine;

namespace SkillSystem.Skills.Movement
{
    public class SkillDash : SkillBase 
    {
        private static readonly int DashAnimationSpeed = Animator.StringToHash("DashAnimationSpeed");

        [SerializeField] private float dashAnimationSpeed = 2f;
        [SerializeField] private float defaultAnimationSpeed = 1f;

        public void OnStartEffect()
        {
            Debug.Log("Starting dash effect");
            if (!Player || !Player.Animator)
                return;

            Player.Animator.SetFloat(DashAnimationSpeed, dashAnimationSpeed);
        }

        public void OnEndEffect()
        {
            if (!Player || !Player.Animator)
                return;

            Player.Animator.SetFloat(DashAnimationSpeed, defaultAnimationSpeed);
        }
    }
}