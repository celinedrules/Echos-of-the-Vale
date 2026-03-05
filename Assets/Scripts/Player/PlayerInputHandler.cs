using Managers;
using UnityEngine;

namespace Player
{
    public class PlayerInputHandler : MonoBehaviour
    {
        public Vector2 MoveInput { get; private set; }

        private PlayerInputSet _input;
        private UiManager _uiManager;
        
        public PlayerInputSet.PlayerActions Player => _input.Player;

        private void Awake()
        {
            _input = new PlayerInputSet();
        }

        private void OnEnable()
        {
            _input.Enable();
            
            RegisterPlayerInputActions();
            RegisterUiInputActions();
            InitializeUiManager();
        }

        private void InitializeUiManager()
        {
            _uiManager = UiManager.Instance;
            _uiManager.OnUiOpened += DisablePlayerInput;
            _uiManager.OnUiClosed += EnablePlayerInput;
        }
        
        private void RegisterUiInputActions()
        {
            _input.UI.Cancel.performed += _ => UiManager.Instance.ToggleMenu();
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