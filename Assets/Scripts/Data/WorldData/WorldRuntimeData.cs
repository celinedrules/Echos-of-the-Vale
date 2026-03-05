// Done
using System.Collections.Generic;
using UnityEngine;

namespace Data.WorldData
{
    [CreateAssetMenu(fileName = "World Runtime Data", menuName = "Echos of the Vale/World/World Runtime Data", order = 0)]
    public class WorldRuntimeData : ScriptableObject
    {
        [SerializeField] private List<string> collectedPickupIds = new();
        [SerializeField] private List<string> openedChestIds = new();

        public IReadOnlyList<string> CollectedPickupIds => collectedPickupIds;
        public IReadOnlyList<string> OpenedChestIds => openedChestIds;

        public void MarkPickupCollected(string pickupId)
        {
            if (string.IsNullOrEmpty(pickupId))
                return;

            if (!collectedPickupIds.Contains(pickupId))
                collectedPickupIds.Add(pickupId);
        }

        public bool IsPickupCollected(string pickupId)
        {
            return !string.IsNullOrEmpty(pickupId) && collectedPickupIds.Contains(pickupId);
        }

        public void MarkChestOpened(string chestId)
        {
            if (string.IsNullOrEmpty(chestId))
                return;

            if (!openedChestIds.Contains(chestId))
                openedChestIds.Add(chestId);
        }

        public bool IsChestOpened(string chestId)
        {
            return !string.IsNullOrEmpty(chestId) && openedChestIds.Contains(chestId);
        }

        public void ResetToDefaults()
        {
            collectedPickupIds.Clear();
            openedChestIds.Clear();
        }

#if UNITY_EDITOR
        private void OnEnable()
        {
            UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void OnDisable()
        {
            UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        private void OnPlayModeStateChanged(UnityEditor.PlayModeStateChange state)
        {
            if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
            {
                ResetToDefaults();
            }
        }
#endif
    }
}