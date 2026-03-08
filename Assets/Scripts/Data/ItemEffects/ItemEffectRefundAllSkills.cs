// Done
using Managers;
using UI.SkillTree.Core;
using UnityEngine;

namespace Data.ItemEffects
{
    [CreateAssetMenu(fileName = "Item Effect - Refund all skills", menuName = "Echos of the Vale/Item Data/Item Effect/Refund all skills", order = 0)]
    public class ItemEffectRefundAllSkills : ItemEffectData
    {
        public override void ExecuteEffect()
        {
            SkillTree skillTree = UiManager.Instance.SkillTree;
            skillTree.RefundAllSkills();
        }
    }
}