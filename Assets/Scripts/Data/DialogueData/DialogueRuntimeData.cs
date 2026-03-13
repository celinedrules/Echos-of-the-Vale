using System;
using System.Collections.Generic;
using UnityEngine;

namespace Data.DialogueData
{
    [CreateAssetMenu(fileName = "Dialogue Runtime Data", menuName = "Echos of the Vale/Dialogue Data/Dialogue Runtime Data")]
    public class DialogueRuntimeData : ScriptableObject
    {
        [Serializable]
        public class NpcDialogueEntry
        {
            public string npcName;
            public int startRowId;
        }

        [SerializeField] private List<NpcDialogueEntry> npcDialogueEntries = new();

        public IReadOnlyList<NpcDialogueEntry> NpcDialogueEntries => npcDialogueEntries;

        public int GetStartRowId(string npcName, int defaultStartRowId)
        {
            for (int i = 0; i < npcDialogueEntries.Count; i++)
            {
                if (npcDialogueEntries[i].npcName == npcName)
                    return npcDialogueEntries[i].startRowId;
            }

            return defaultStartRowId;
        }

        public void SetStartRowId(string npcName, int newStartRowId)
        {
            for (int i = 0; i < npcDialogueEntries.Count; i++)
            {
                if (npcDialogueEntries[i].npcName == npcName)
                {
                    npcDialogueEntries[i].startRowId = newStartRowId;
                    return;
                }
            }

            npcDialogueEntries.Add(new NpcDialogueEntry
            {
                npcName = npcName,
                startRowId = newStartRowId
            });
        }

        public bool HasEntry(string npcName)
        {
            for (int i = 0; i < npcDialogueEntries.Count; i++)
            {
                if (npcDialogueEntries[i].npcName == npcName)
                    return true;
            }

            return false;
        }

        public void SetEntries(List<NpcDialogueEntry> entries) => npcDialogueEntries = new List<NpcDialogueEntry>(entries);

        public void ResetToDefaults()
        {
            npcDialogueEntries.Clear();
        }

#if UNITY_EDITOR
        private void OnEnable() => UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        private void OnDisable() => UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;

        private void OnPlayModeStateChanged(UnityEditor.PlayModeStateChange state)
        {
            if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
                ResetToDefaults();
        }
#endif
    }
}