// Done
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities.Enums;

namespace Data.SkillData
{
    [CreateAssetMenu(fileName = "Skill Runtime Data", menuName = "Echos of the Vale/Skill Data/Skill Runtime Data", order = 0)]
    public class SkillRuntimeData : ScriptableObject
    {
        [Header("Skill Points")]
        [SerializeField] private int skillPoints;

        [Header("Unlocked Skills")]
        [SerializeField] private List<string> unlockedSkills = new();

        [Header("Skill Upgrades")]
        [SerializeField] private List<SkillUpgradeEntry> skillUpgrades = new();

        public int SkillPoints
        {
            get => skillPoints;
            set => skillPoints = value;
        }

        public List<string> UnlockedSkills => unlockedSkills;
        public List<SkillUpgradeEntry> SkillUpgrades => skillUpgrades;

        public void SetSkillUnlocked(string skillName, bool unlocked)
        {
            if (unlocked && !unlockedSkills.Contains(skillName))
                unlockedSkills.Add(skillName);
            else if (!unlocked)
                unlockedSkills.Remove(skillName);
        }

        public bool IsSkillUnlocked(string skillName) => unlockedSkills.Contains(skillName);

        public void SetSkillUpgrade(SkillType skillType, SkillUpgradeType upgradeType)
        {
            var existing = skillUpgrades.Find(e => e.skillType == skillType);
            if (existing != null)
            {
                existing.upgradeType = upgradeType;
            }
            else
            {
                skillUpgrades.Add(new SkillUpgradeEntry { skillType = skillType, upgradeType = upgradeType });
            }
        }

        public SkillUpgradeType? GetSkillUpgrade(SkillType skillType)
        {
            var entry = skillUpgrades.Find(e => e.skillType == skillType);
            return entry?.upgradeType;
        }

        public void ResetToDefaults()
        {
            skillPoints = 0;
            unlockedSkills.Clear();
            skillUpgrades.Clear();
        }

#if UNITY_EDITOR
        [ButtonGroup("Debug")]
        [Button("Clear Runtime Data", ButtonSizes.Medium)]
        private void DebugClearRuntimeData()
        {
            ResetToDefaults();
            Debug.Log("Skill runtime data cleared.");
        }

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