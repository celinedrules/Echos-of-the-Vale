using UnityEngine;

namespace UI.Common
{
    public class SimplePanel : MonoBehaviour, IUiPanel
    {
        private CanvasGroup _canvasGroup;
        
        public CanvasGroup CanvasGroup => _canvasGroup;
        public bool ShowMenuButtons => false;
        public bool ShowBackground => false;
        public bool DisablePlayerInput => false;
        public bool HasTooltips => false;
        
        public void OnOpened()
        {
            
        }

        private void Start() => _canvasGroup = GetComponent<CanvasGroup>();
    }
}