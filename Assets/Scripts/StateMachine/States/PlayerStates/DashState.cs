using Core;
using Managers;
using UnityEngine;
using Utilities;

namespace StateMachine.States.PlayerStates
{
    public class DashState : PlayerState
    {
        private Direction _dashDirection;
        protected Timer  _dashTimer;

        public override void Enter()
        {
            base.Enter();

            SkillManager.Instance.Dash.OnStartEffect();
            Player.PlayerFx?.PlayImageEchoEffect(Player.DashDuration);

            _dashDirection = Player.FacingDirection;

            Player.Health.CanTakeDamage = false;

            SetupTimers();
        }

        public override void Update()
        {
            Vector2 dashVelocity = _dashDirection switch
            {
                Direction.Up => Vector2.up,
                Direction.Down => Vector2.down,
                Direction.Left => Vector2.left,
                Direction.Right => Vector2.right,
                _ => Vector2.zero
            };

            Player.SetVelocity(dashVelocity * Player.DashSpeed);
        }

        public override void Exit()
        {
            base.Exit();

            SkillManager.Instance.Dash.OnEndEffect();

            _dashTimer?.Cancel();
            Player.Health.CanTakeDamage = true;
            Player.SetVelocity(Vector2.zero);
        }

        private void SetupTimers()
        {
            _dashTimer = TimerManager.Instance.CreateTimer(Player.DashDuration, () =>
            {
                StateMachine.ChangeState(Player.IdleState);
            });
        }
    }
}