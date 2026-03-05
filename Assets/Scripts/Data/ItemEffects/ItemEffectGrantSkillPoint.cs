using UnityEngine;

namespace Data.ItemEffects
{
    [CreateAssetMenu(fileName = "Item Effect - Grant Skill Points", menuName = "Echos of the Vale/Item Data/Item Effect/Grant Skill Points", order = 0)]
    public class ItemEffectGrantSkillPoint : ItemEffectData
    {
        [SerializeField] private int pointsToAdd = 1;

        public override void ExecuteEffect()
        {
            //SkillManager.Instance.AddSkillPoints(pointsToAdd);
        }
    }
}