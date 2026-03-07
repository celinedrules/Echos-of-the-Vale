// Done
using System.Linq;
using Core.Interfaces;
using Data.SkillData;
using Managers;
using SaveSystem;
using Sirenix.OdinInspector;
using SkillSystem.Core;
using TMPro;
using UI.Common;
using UI.SkillTree.Connections;
using UI.SkillTree.Nodes;
using UnityEngine;
using Utilities.Enums;

namespace UI.SkillTree.Core
{
    public class SkillTree : MonoBehaviour, IUiPanel, ISavable
    {
        [SerializeField] private TextMeshProUGUI skillPointsText;
        [SerializeField] private TreeConnectHandler[] parentNodes;

        private SkillRuntimeData RuntimeData => GameManager.Instance.SkillRuntimeData;
        private SkillManager _skillManager;
        private CanvasGroup _canvasGroup;

        public CanvasGroup CanvasGroup => _canvasGroup;
        public bool ShowMenuButtons => true;
        public bool ShowBackground => true;
        public bool DisablePlayerInput => true;
        public bool HasTooltips => true;

        private void OnEnable() => _skillManager = SkillManager.Instance;

        private void Start()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvasGroup.alpha = 0;
            UpdateAllConnections();

            _skillManager.OnSkillPointsChanged += OnSkillPointsUpdated;

            LoadFromRuntimeData();
        }

        private void OnSkillPointsUpdated(int totalPoints)
        {
            skillPointsText.text = totalPoints.ToString();
            UpdateAllNodeAvailability();
        }

        public bool HasEnoughSkillPoints(int amount) => _skillManager.SkillPoints >= amount;

        [ContextMenu("Reset Skill Tree")]
        [Button("Reset Skill Tree")]
        public void RefundAllSkills()
        {
            UiTreeNode[] skillNodes = GetComponentsInChildren<UiTreeNode>();

            foreach (UiTreeNode node in skillNodes)
                node.Refund();
        }

        private void UpdateAllNodeAvailability()
        {
            UiTreeNode[] allNodes = GetComponentsInChildren<UiTreeNode>();

            foreach (UiTreeNode node in allNodes)
                node.UpdateConnectionAvailability();
        }

        [ContextMenu("Update All Connections")]
        [Button("Update All Connections")]
        public void UpdateAllConnections()
        {
            foreach (TreeConnectHandler handler in parentNodes)
            {
                handler.UpdateAllConnections();
            }
        }

        private void OnDisable()
        {
            if (_skillManager != null)
                _skillManager.OnSkillPointsChanged -= OnSkillPointsUpdated;
        }

        public void OnOpened()
        {
        }
        
        public void SaveToRuntimeData()
        {
            var runtimeData = RuntimeData;
            if (runtimeData == null) return;

            runtimeData.SkillPoints = _skillManager.SkillPoints;
            runtimeData.UnlockedSkills.Clear();
            runtimeData.SkillUpgrades.Clear();

            UiTreeNode[] allNodes = GetComponentsInChildren<UiTreeNode>();

            foreach (UiTreeNode node in allNodes)
            {
                if (!node.SkillData) continue;

                if (node.IsUnlocked)
                    runtimeData.SetSkillUnlocked(node.SkillData.DisplayName, true);
            }

            foreach (SkillBase skill in _skillManager.AllSkills)
                runtimeData.SetSkillUpgrade(skill.SkillType, skill.SkillUpgradeType);
        }

        private void LoadFromRuntimeData()
        {
            SkillRuntimeData runtimeData = RuntimeData;
            if (!runtimeData || runtimeData.UnlockedSkills.Count == 0) 
                return;

            _skillManager.SetSkillPoints(runtimeData.SkillPoints);

            UiTreeNode[] allNodes = GetComponentsInChildren<UiTreeNode>();

            foreach (UiTreeNode node in allNodes)
            {
                if (!node.SkillData) 
                    continue;

                if (runtimeData.IsSkillUnlocked(node.SkillData.DisplayName))
                    node.UnlockWithSaveData();
            }

            foreach (SkillBase skill in _skillManager.AllSkills)
            {
                SkillUpgradeType? upgradeType = runtimeData.GetSkillUpgrade(skill.SkillType);
                
                if (upgradeType == null) 
                    continue;

                UiTreeNode upgradeNode = allNodes.FirstOrDefault(
                    node => node.SkillData?.UpgradeData?.UpgradeType == upgradeType);

                if (upgradeNode)
                    skill.UpgradeSkill(upgradeNode.SkillData);
            }
        }

        public void SaveData(ref GameData gameData)
        {
            SaveToRuntimeData();
            
            gameData.skillPoints = _skillManager.SkillPoints;
            gameData.skillTree.Clear();
            gameData.skillUpgrades.Clear();

            UiTreeNode[] allNodes = GetComponentsInChildren<UiTreeNode>();

            foreach (UiTreeNode node in allNodes)
            {
                if (!node.SkillData)
                    continue;
                
                string skillName = node.SkillData.DisplayName;
                gameData.skillTree[skillName] = node.IsUnlocked;
            }

            foreach (SkillBase skill in _skillManager.AllSkills)
                gameData.skillUpgrades[skill.SkillType] = skill.SkillUpgradeType;
        }

        public void LoadData(GameData gameData)
        {
            _skillManager.SetSkillPoints(gameData.skillPoints);

            UiTreeNode[] allNodes = GetComponentsInChildren<UiTreeNode>();

            foreach (UiTreeNode node in allNodes)
            {
                if (!node.SkillData)
                    continue;

                string skillName = node.SkillData.DisplayName;

                if (gameData.skillTree.TryGetValue(skillName, out bool unlocked) && unlocked)
                    node.UnlockWithSaveData();
            }

            foreach (SkillBase skill in _skillManager.AllSkills)
            {
                if (!gameData.skillUpgrades.TryGetValue(skill.SkillType, out SkillUpgradeType upgradeType))
                    continue;
                
                UiTreeNode upgradeNode =
                    allNodes.FirstOrDefault(node => node.SkillData.UpgradeData.UpgradeType == upgradeType);

                if (upgradeNode)
                    skill.UpgradeSkill(upgradeNode.SkillData);
            }
            
            SaveToRuntimeData();
        }
    }
}