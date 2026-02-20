using System;
using UnityEngine;

namespace Player
{
    public class PlayerInputHandler : MonoBehaviour
    {
        public Vector2 MoveInput { get; private set; }

        private PlayerInputSet _input;
        
        public PlayerInputSet.PlayerActions Player => _input.Player;

        private void Awake()
        {
            _input = new PlayerInputSet();
        }

        private void OnEnable()
        {
            _input.Enable();
            
            RegisterPlayerInputActions();
        }

        private void RegisterPlayerInputActions()
        {
            _input.Player.Movement.performed += ctx => MoveInput = ctx.ReadValue<Vector2>();
            _input.Player.Movement.canceled += _ => MoveInput = Vector2.zero;
        }

        private void OnDisable()
        {
            _input.Disable();
        }
        
        private void EnablePlayerInput()
        {
            _input.Player.Enable();
            Time.timeScale = 1f;
        }

        private void DisablePlayerInput()
        {
            _input.Player.Disable();
            Time.timeScale = 0f;
        }
    }
}