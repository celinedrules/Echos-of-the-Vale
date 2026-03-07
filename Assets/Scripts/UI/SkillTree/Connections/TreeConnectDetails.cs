// Done
using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace UI.SkillTree.Connections
{
    [Serializable]
    public class TreeConnectDetails
    {
        [SerializeField] private TreeConnectHandler childNode;

        [ReadOnly] // Make it read-only so you can see the auto-calculated value
        [SerializeField] private float length = 100f;


        public float Length
        {
            get => length;
            set => length = value;
        }

        public TreeConnectHandler ChildNode
        {
            get => childNode;
            set => childNode = value;
        }
    }
}