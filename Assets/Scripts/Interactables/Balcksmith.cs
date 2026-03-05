// Done
using Data.EntityData;

namespace Interactables
{
    public class Blacksmith : Npc
    {
        private BlacksmithNpcData _data;
        
        protected override System.Type RequiredDataType => typeof(BlacksmithNpcData);
        
        protected override void Awake()
        {
            base.Awake();
            Animator.SetBool(IsBlacksmith, true);
            _data = Data as BlacksmithNpcData;
        }

        public override void Interact()
        {
            base.Interact();
            UiManager.Storage.SetupStorage(Storage);
            UiManager.Craft.SetupCraftUi(Storage, _data);
        }
    }
}