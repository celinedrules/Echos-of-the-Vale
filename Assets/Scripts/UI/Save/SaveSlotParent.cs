// Done
using Managers;
using SaveSystem;
using UnityEngine;

namespace UI
{
    public class SaveSlotParent : MonoBehaviour
    {
        [SerializeField] private GameObject saveSlotPrefab;

        private const string FileNamePrefix = "ultimaterpg";

        public void CreateSaveSlots(int numberOfSlots)
        {
            foreach (Transform child in transform)
                Destroy(child.gameObject);
            
            for (int i = 0; i < numberOfSlots; i++)
            {
                // Load metadata for this slot
                FileDataHandler dataHandler = new(Application.persistentDataPath, $"{FileNamePrefix}{i}.json", false);
                SaveMetadata metadata = dataHandler.LoadMetadata();
            
                if(!UiManager.Instance.IsSaving && metadata == null)
                    continue;
                
                SaveSlot slot = Instantiate(saveSlotPrefab, transform).GetComponent<SaveSlot>();
                slot.SetSaveSlotData(i, metadata);
            }
        }
    }
}