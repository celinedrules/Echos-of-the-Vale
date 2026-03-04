using Managers;
using UI.Common;
using UnityEngine;

namespace UI
{
    public class MainMenu : MonoBehaviour, IUiPanel
    {
        private const string DefaultScene = "DefaultScene";
        private CanvasGroup _canvasGroup;

        private void Start()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            UiManager.Instance.OpenMainMenu();
            //FusionAudioManager.Instance.PlayAudio(BackgroundAudioId.MainMenu);
        }

        public void Play()
        {
            //FusionAudioManager.Instance.PlayAudio(UiAudioId.ButtonClicked);
            GameManager.Instance.LoadScene(DefaultScene);
        }

        public void Quit() => Application.Quit();

        public CanvasGroup CanvasGroup => _canvasGroup;
        public bool ShowMenuButtons => false;
        public bool ShowBackground => true;
        public bool DisablePlayerInput => true;
        public bool HasTooltips => false;

        public void OnOpened()
        {
        }
    }
}