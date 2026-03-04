using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class SaveSlot : MonoBehaviour, IPointerDownHandler
    {
        [SerializeField] private GameObject newSaveSlot;
        [SerializeField] private GameObject existingSaveSlot;
        [SerializeField] private TextMeshProUGUI saveSlotNumber;
        [SerializeField] private TextMeshProUGUI saveSlotLocation;
        [SerializeField] private TextMeshProUGUI saveDateTime;
        [SerializeField] private TextMeshProUGUI savePlayTime;
        [SerializeField] private TextMeshProUGUI characterName;
        [SerializeField] private TextMeshProUGUI gold;
        [SerializeField] private TextMeshProUGUI crystals;
        [SerializeField] private Image screenshot;

        private int _slotIndex;
        protected int SlotIndex => _slotIndex;

        // public void SetSaveSlotData(int slotIndex, SaveMetadata metadata)
        // {
        //     _slotIndex = slotIndex;
        //     saveSlotNumber.text = $"Slot: {slotIndex + 1}";
        //     
        //     bool exists = metadata != null;
        //     existingSaveSlot.SetActive(exists);
        //     newSaveSlot.SetActive(!exists);
        //     
        //     if (exists)
        //         UpdateUi(metadata);
        // }
    
        public void OnPointerDown(PointerEventData eventData)
        {
            // if(UiManager.Instance.IsSaving)
            // {
            //     SaveMetadata metadata = SaveManager.Instance.SaveGameToSlot(_slotIndex);
            //     ScreenshotManager.Instance.SaveScreenshot(_slotIndex);
            //     UpdateUi(metadata);
            //     UiManager.Instance.ShowNotification("Game Saved!", () =>
            //     {
            //         UiManager.Instance.OpenOptions();
            //     });
            // }
            // else
            // {
            //     SaveManager.Instance.LoadGameFromSlot(_slotIndex);
            //     UiManager.Instance.ShowNotification("Game Loaded!", () =>
            //     {
            //         UiManager.Instance.TryCloseActiveUi();
            //     });
            // }
        }

        // private void UpdateUi(SaveMetadata metadata)
        // {
        //     existingSaveSlot.SetActive(true);
        //     newSaveSlot.SetActive(false);
        //     
        //     saveSlotLocation.text = metadata.location;
        //     saveDateTime.text = metadata.FormattedDateTime;
        //     savePlayTime.text = metadata.FormattedPlayTime;
        //     characterName.text = metadata.characterName;
        //     gold.text = metadata.gold.ToString();
        //     crystals.text = metadata.crystals.ToString();
        //     
        //     Sprite sc = ScreenshotManager.Instance.LoadScreenshot(_slotIndex);
        //     if (sc != null)
        //         screenshot.sprite = sc;
        // }
    }
}