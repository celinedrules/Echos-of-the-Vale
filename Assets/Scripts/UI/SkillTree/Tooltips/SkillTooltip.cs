// Done
using System.Collections;
using System.Text;
using Data.SkillData;
using Managers;
using TMPro;
using UI.Common;
using UI.SkillTree.Nodes;
using UnityEngine;

namespace UI.SkillTree.Tooltips
{
    public class SkillTooltip : Tooltip
    {
        [SerializeField] private TextMeshProUGUI skillName;
        [SerializeField] private TextMeshProUGUI skillDescription;
        [SerializeField] private TextMeshProUGUI skillCooldown;
        [SerializeField] private TextMeshProUGUI skillRequirements;
        [SerializeField] private Color metCondition;
        [SerializeField] private Color notMetCondition;
        [SerializeField] private Color importantInfo;

        [SerializeField]
        private string lockedSkillText = "You've taken a different path and can't use this skill anymore.";
        
        private SkillTree.Core.SkillTree _skillTree;
        private Coroutine _textEffectRoutine;

        protected override void Awake()
        {
            base.Awake();
            _skillTree = UiManager.Instance.SkillTree;
        }

        public void ShowTooltip(bool show, RectTransform targetRect, SkillData skillData, UiTreeNode node)
        {
            if (!skillData)
                return;
            
            base.ShowTooltip(show, targetRect);

            if (!show)
                return;
            
            skillName.text = skillData.DisplayName;
            skillDescription.text = skillData.Description;
            skillCooldown.text = $"Cooldown: {skillData.UpgradeData.Cooldown} Seconds";

            if (node == null)
            {
                skillRequirements.text = "";
                return;
            }
            
            string skillLockedText = GetColoredString(lockedSkillText, importantInfo);
            string requirementsText = node.IsLocked ? skillLockedText : GetRequirements(node.SkillData.Cost, node.NeededNodes, node.ConflictingNodes);
            skillRequirements.text = requirementsText;
        }

        public void LockedSkillEffect()
        {
            Debug.Log("Locked Skill Effect");
            StopLockedSkillEffect();
            _textEffectRoutine = StartCoroutine(TextEffectRoutine(skillRequirements, 0.15f, 3));
        }
        
        public void StopLockedSkillEffect()
        {
            if (_textEffectRoutine != null)
                StopCoroutine(_textEffectRoutine);
        }
        
        private IEnumerator TextEffectRoutine(TextMeshProUGUI text, float blinkInterval, int timesToBlink)
        {
            for (int i = 0; i < timesToBlink; i++)
            {
                text.text = GetColoredString(lockedSkillText, notMetCondition);
                yield return new WaitForSeconds(blinkInterval);
                text.text = GetColoredString(lockedSkillText, importantInfo);
                yield return new WaitForSeconds(blinkInterval);
            }
        }
        
        private string GetRequirements(int skillCost, UiTreeNode[] neededNodes, UiTreeNode[] conflictingNodes)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Requirements");

            Color costColor = _skillTree.HasEnoughSkillPoints(skillCost) ? metCondition : notMetCondition;
            string costText = $" - {skillCost} skill point(s)";
            sb.AppendLine(GetColoredString(costText, costColor));

            foreach (UiTreeNode node in neededNodes)
            {
                if(!node)
                    continue;
                
                Color nodeColor = node.IsUnlocked ? metCondition : notMetCondition;
                string nodeText = $" - {node.SkillData.DisplayName}";
                sb.AppendLine(GetColoredString(nodeText, nodeColor));
            }

            if (conflictingNodes.Length <= 0)
                return sb.ToString();

            sb.AppendLine();
            sb.AppendLine(GetColoredString("Locks out", importantInfo));

            foreach (UiTreeNode node in conflictingNodes)
            {
                if(!node)
                    continue;
                
                string nodeText = $" - {node.SkillData.DisplayName}";
                sb.AppendLine(GetColoredString(nodeText, importantInfo));
            }

            return sb.ToString();
        }
    }
}