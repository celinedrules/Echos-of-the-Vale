using Player;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Data.ItemEffects
{
    public class ItemEffectData : ScriptableObject
    {
        [TextArea(4, 4)]
        [SerializeField, LabelText("Description")]
        private string effectDescription;

        protected PlayerController Player;

        public string EffectDescription => effectDescription;

        public virtual bool CanBeUsed(PlayerController player) => true;

        public virtual void ExecuteEffect()
        {
        }

        public virtual void Subscribe(PlayerController player) => Player = player;

        public virtual void Unsubscribe()
        {
        }
    }
}