// Done
using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Stats.Groups
{
    [Serializable, InlineProperty, HideLabel]
    [GUIColor(1, 1, 1)]
    public class MajorStats
    {
        [SerializeField]
        [FoldoutGroup("Strength"), Indent(1)]
        private Stat strength = new(0);

        [SerializeField]
        [FoldoutGroup("Agility"), Indent(1)]
        private Stat agility = new(0);

        [SerializeField]
        [FoldoutGroup("Intelligence"), Indent(1)]
        private Stat intelligence = new(0);

        [SerializeField]
        [FoldoutGroup("Vitality"), Indent(1)]
        private Stat vitality  = new(0);

        public Stat Strength { get => strength; set => strength = value; }
        public Stat Agility { get => agility; set => agility = value; }
        public Stat Intelligence { get => intelligence; set => intelligence = value; }
        public Stat Vitality { get => vitality; set => vitality = value; }
        
        public void Reset()
        {
            Strength.Value = 0;
            Agility.Value = 0;
            Intelligence.Value = 0;
            Vitality.Value = 0;
        }
    }
}