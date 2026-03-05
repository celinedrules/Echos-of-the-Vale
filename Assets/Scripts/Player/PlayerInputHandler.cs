using Managers;
using UnityEngine;

namespace Player
{
    public class PlayerInputHandler : MonoBehaviour
    {
        public Vector2 MoveInput { get; private set; }
        public Vector2 MousePosition { get; private set; }

        private PlayerInputSet _input;
        private UiManager _uiManager;
        
        public PlayerInputSet.PlayerActions Player => _input.Player;

        private void Awake() => _input = new PlayerInputSet();

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
            // _input.UI.DialogueInteraction.performed += _ =>
            // {
            //     if(UiManager.Instance.IsActivePanelOfType<Dialogue>())
            //         UiManager.Instance.Dialogue.DialogueInteraction();
            // };
            // _input.UI.DialogueNavigation.performed += ctx =>
            // {
            //     int direction = Mathf.RoundToInt(ctx.ReadValue<float>());
            //
            //     if (UiManager.Instance.IsActivePanelOfType<Dialogue>())
            //         UiManager.Instance.Dialogue.NavigateChoices(direction);
            // };
        }
        
        private void RegisterPlayerInputActions()
        {
            //_input.Player.Mouse.performed += ctx => MousePosition = ctx.ReadValue<Vector2>();
            _input.Player.Movement.performed += ctx => MoveInput = ctx.ReadValue<Vector2>();
            _input.Player.Movement.canceled += _ => MoveInput = Vector2.zero;
            // _input.Player.Spell.performed += _ => SkillManager.Instance.Shard.TryUseSkill();
            // _input.Player.Spell.performed += _ => SkillManager.Instance.TimeEcho.TryUseSkill();
            // _input.Player.Interact.performed += _ => GameManager.Instance.Player.TryInteract();
            // _input.Player.QuickItemSlot1.performed += _ => GameManager.Instance.Player.Inventory.TryUseQuickItem(1);
            // _input.Player.QuickItemSlot2.performed += _ => GameManager.Instance.Player.Inventory.TryUseQuickItem(2);
        }

        private void OnDisable()
        {
            _input.Disable();
            
            if(!_uiManager)
                return;
            
            _uiManager.OnUiOpened -= DisablePlayerInput;
            _uiManager.OnUiClosed -= EnablePlayerInput;
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