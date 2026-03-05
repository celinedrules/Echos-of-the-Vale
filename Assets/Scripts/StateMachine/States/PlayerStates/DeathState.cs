// Done
using Managers;

namespace StateMachine.States.PlayerStates
{
    public class DeathState : PlayerState
    {
        private bool _enteredThisFrame;

        public override void Enter()
        {
            base.Enter();

            _enteredThisFrame = true;
            Input.Player.Disable();
        }

        public override void Update()
        {
            base.Update();

            if (_enteredThisFrame)
            {
                _enteredThisFrame = false;
                return;
            }

            if (HasAnimationFinished())
            {
                UiManager.Instance.OpenGameOver();
            }
        }
    }
}