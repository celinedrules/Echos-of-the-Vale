using System;
using UI;
using UI.Common;
using UI.Dialogue;
using UI.Hud;
using UI.Inventory;
using UI.Quests;
using UnityEngine;
using Utilities;

namespace Managers
{
    public class UiManager : Singleton<UiManager>
    {
        public event Action OnUiOpened;
        public event Action OnUiClosed;

        [SerializeField] private CanvasGroup panelBackground;
        [SerializeField] private Hud hud;

        [Header("Menu Buttons")]
        [SerializeField] private CanvasGroup menuButtons;
        [SerializeField] private PanelMenuButton inventoryMenuButton;
        
        [Header("Panels")]
        [SerializeField] private MainMenu mainMenu;
        [SerializeField] private GameOver gameOver;
        [SerializeField] private Save save;
        [SerializeField] private Options options;
        [SerializeField] private Inventory inventory;
        [SerializeField] private Storage storage;
        [SerializeField] private Craft craft;
        [SerializeField] private Merchant merchant;
        // [SerializeField] private SkillTree skillTree;
        [SerializeField] private Quest quest;
        [SerializeField] private ActiveQuest activeQuest;
        [SerializeField] private SimplePanel quickItemSlotOptions;
        [SerializeField] private Dialogue dialogue;
        
        [Header("Tooltips")]
        // [SerializeField] private SkillTooltip skillTooltip;
        [SerializeField] private ItemTooltip itemTooltip;
        [SerializeField] private StatTooltip statTooltip;
        
        [Header("Notifications")]
        [SerializeField] private NotificationPopup notificationPopup;
        
        private CanvasGroup _activePanel;
        private IUiPanel _activeUiPanel;
        private bool _activePanelHasTooltips;

        public bool IsSaving { get; private set; }
        
        public MainMenu MainMenu => mainMenu;
        public GameOver GameOver => gameOver;
        public Save Save => save;
        public Options Options => options;
        public Hud Hud => hud;
        public Merchant Merchant => merchant;
        public Craft Craft => craft;
        public Storage Storage => storage;
        // public SkillTree SkillTree => skillTree;
        // public SkillTooltip SkillTooltip => skillTooltip;
        public ItemTooltip ItemTooltip => itemTooltip;
        public StatTooltip StatTooltip => statTooltip;
        public Quest Quest => quest;
        public  ActiveQuest ActiveQuest => activeQuest;
        public Dialogue Dialogue => dialogue;
        
        public bool IsActivePanelOfType<T>() where T : IUiPanel => _activeUiPanel is T;
        
        public void LoadLastSave()
        {
            Debug.Log("LoadLastSave");
        }
        
        public void OpenSave(bool isSaving)
        {
            IsSaving = isSaving;
            
            if(isSaving)
                ScreenshotManager.Instance.CaptureScreenshot();
            
            OpenPanel(save);
        }
        
        public void OpenMainMenu() => OpenPanel(mainMenu);
        public void OpenGameOver() => OpenPanel(gameOver);
        public void OpenOptions() => OpenPanel(options);
        //public void OpenSkillTree() => OpenPanel(skillTree);
        public void OpenInventory() => OpenPanel(inventory);
        public void OpenStorage() => OpenPanel(storage);
        public void OpenCraft() => OpenPanel(craft);
        public void OpenMerchant() => OpenPanel(merchant);
        // public void OpenQuickItemSlotOptions() => OpenPanel(quickItemSlotOptions);
        public void OpenQuest() => OpenPanel(quest);
        public void OpenActiveQuest() => OpenPanel(activeQuest);
        public void OpenDialogue() => OpenPanel(dialogue);
        
        public void ToggleMenu()
        {
            if (_activePanel)
            {
                TryCloseActiveUi();
            }
            else
            {
                inventoryMenuButton.SetAsActive();
                OpenInventory();
            }
        }

        public void TryCloseActiveUi()
        {
            if (!_activePanel)
                return;

            CloseActivePanel();
            OnUiClosed?.Invoke();
        }
        
        private void OpenPanel(IUiPanel panel)
        {
            if (_activePanel == panel.CanvasGroup)
                return;

            CloseActivePanel();
            _activePanel = panel.CanvasGroup;
            _activeUiPanel = panel;
            SetPanelVisible(_activePanel, true);
            SetMenuButtonsVisible(panel.ShowMenuButtons);
            _activePanelHasTooltips = panel.HasTooltips;

            panelBackground.alpha = panel.ShowBackground ? 1 : 0;

            panel.OnOpened();

            if (panel.DisablePlayerInput)
                OnUiOpened?.Invoke();
        }

        private void CloseActivePanel()
        {
            if (!_activePanel)
                return;

            SetPanelVisible(_activePanel, false);
            SetMenuButtonsVisible(false);

            panelBackground.alpha = 0;
            HideAllTooltips();

            _activePanel = null;
            _activeUiPanel = null;
        }
        
        private void SetMenuButtonsVisible(bool visible)
        {
            if (menuButtons)
            {
                menuButtons.alpha = visible ? 1 : 0;
                menuButtons.interactable = visible;
                menuButtons.blocksRaycasts = visible;
            }
        }

        private static void SetPanelVisible(CanvasGroup panel, bool visible)
        {
            panel.alpha = visible ? 1 : 0;
            panel.blocksRaycasts = visible;
            panel.interactable = visible;
        }

        private void HideAllTooltips()
        {
            if(_activePanelHasTooltips)
            {
                // HideTooltip(skillTooltip.CanvasGroup);
                HideTooltip(statTooltip.CanvasGroup);
                HideTooltip(itemTooltip.CanvasGroup);
            }
        }
        
        private static void HideTooltip(CanvasGroup tooltip) => tooltip.alpha = 0;

        public void ShowNotification(string message, Action onComplete = null) =>
            notificationPopup?.Show(message, onComplete);
    }
}