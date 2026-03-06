using System;
using System.Collections.Generic;
using UnityEngine;

namespace Data.QuestData
{
    [CreateAssetMenu(fileName = "Quest Runtime Data", menuName = "Echos of the Vale/Quest Data/Quest Runtime Data")]
    public class QuestRuntimeData : ScriptableObject
    {
        [Serializable]
        public class QuestEntry
        {
            public string saveId;
            public int currentAmount;
        }

        [Header("Active Quests")]
        [SerializeField] private List<QuestEntry> activeQuests = new();

        [Header("Completed Quests")]
        [SerializeField] private List<string> completedQuestIds = new();

        public IReadOnlyList<QuestEntry> ActiveQuests => activeQuests;
        public IReadOnlyList<string> CompletedQuestIds => completedQuestIds;

        public void SetActiveQuests(List<QuestEntry> quests) => activeQuests = new List<QuestEntry>(quests);
        public void SetCompletedQuests(List<string> questIds) => completedQuestIds = new List<string>(questIds);

        public void ResetToDefaults()
        {
            activeQuests.Clear();
            completedQuestIds.Clear();
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