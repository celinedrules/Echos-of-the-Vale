using UnityEngine;

namespace StateMachine.States.PlayerStates
{
    public class DeathState : PlayerState
    {
        private bool _enteredtThisFrame;

        public override void Enter()
        {
            base.Enter();

            _enteredtThisFrame = true;
            Input.Player.Disable();
        }

        public override void Update()
        {
            base.Update();

            if (_enteredtThisFrame)
            {
                _enteredtThisFrame = false;
                return;
            }

            if (HasAnimationFinished())
            {
                Debug.Log("GAME OVER!");
                //UiManager.Instance.OpenGameOver();
            }
        }
    }
}