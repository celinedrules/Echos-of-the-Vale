// Done
using UI.Common;
using UnityEngine;

namespace UI
{
    public class Save : MonoBehaviour, IUiPanel
    {
        [SerializeField] private SaveSlotParent saveSlotParent;
        [SerializeField] private int maxSaveSlots = 3;

        private CanvasGroup _canvasGroup;

        public CanvasGroup CanvasGroup => _canvasGroup;
        public bool ShowMenuButtons => false;
        public bool ShowBackground => true;
        public bool DisablePlayerInput => true;
        public bool HasTooltips => false;

        private void Start()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvasGroup.alpha = 0;
        }

        public void OnOpened()
        {
            saveSlotParent.CreateSaveSlots(maxSaveSlots);
        }
    }
}