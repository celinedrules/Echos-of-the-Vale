// Done
using Data.EntityData;
using UnityEngine;

namespace Interactables
{
    public class Merchant : Npc
    {
        private MerchantNpcData _data;
        
        protected override System.Type RequiredDataType => typeof(MerchantNpcData);
        
        protected override void Awake()
        {
            base.Awake();
            Animator?.SetBool(IsBlacksmith, false);
            _data = Data as MerchantNpcData;
            
            if(!_data)
                throw new System.ArgumentNullException(nameof(_data), "MerchantNpcData cannot be null");
            
            Merchant.SetShopData(_data.ShopInventory, _data.MinItemsAmount);
        }

        protected override void Update()
        {
            base.Update();
            
            if(Input.GetKeyDown(KeyCode.Z))
                Merchant.FillShopList();
        }

        public override void Interact()
        {
            base.Interact();
            UiManager.Merchant.SetupMerchantUi(Merchant, Inventory);
        }
    }
}