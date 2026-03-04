// Done
using UI.Common;
using UnityEngine;

namespace UI
{
    public class GameOver : MonoBehaviour, IUiPanel
    {
        private CanvasGroup _canvasGroup;

        private void Start()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }
        
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