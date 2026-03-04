// Done
using Data.ItemEffects;
using InventorySystem;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using Utilities.Enums;

namespace Data.ItemData
{
    [CreateAssetMenu(fileName = "Material Data - ", menuName = "Echos of the Vale/Item Data/Material Data")]
    public class ItemData : ScriptableObject
    {
        protected const string LeftVerticalGroup = "Split/Left";
        private const string MerchantDetailsGroup = "Split/Left/Merchant Details";
        private const string ItemDetailsVerticalGroup = "Split/Left/Item Details/Split/Right";
        
        [HideInInspector] public string saveId;

        [VerticalGroup(LeftVerticalGroup)]
        [HorizontalGroup(LeftVerticalGroup + "/Item Details/Split", 64, LabelWidth = 67)]
        [SerializeField, HideLabel, PreviewField(64, ObjectFieldAlignment.Left)]
        private Sprite itemIcon;
        
        [BoxGroup(LeftVerticalGroup + "/Item Details")]
        [VerticalGroup(ItemDetailsVerticalGroup)]
        [SerializeField, LabelText("Name")] private string itemName;
        
        [VerticalGroup(ItemDetailsVerticalGroup)]
        [SerializeField, LabelText("Type")] private ItemType itemType;
        
        [VerticalGroup(ItemDetailsVerticalGroup)]
        [SerializeField, LabelText("Max Size")] private int maxStackSize = 1;
        
        [BoxGroup(MerchantDetailsGroup)]
        [SerializeField] private int itemPrice;

        [BoxGroup(MerchantDetailsGroup)]
        [SerializeField, LabelText("Min Size")] private int minStackSizeAtShop = 1;
        
        [BoxGroup(MerchantDetailsGroup)]
        [SerializeField, LabelText("Max Size")] private int maxStackSizeAtShop = 1;

        [HorizontalGroup("Split", 0.5f, MarginLeft = 5, LabelWidth = 130)]
        [BoxGroup("Split/Right/Drop Details")]
        [Range(0, 1000)]
        [SerializeField] private int rarity = 100;

        [BoxGroup("Split/Right/Drop Details")]
        [Range(0, 100)]
        [SerializeField] private float dropChance;
        
        [BoxGroup("Split/Right/Drop Details")]
        [Range(0, 100)]
        [SerializeField] private float maxDropChance = 65.0f;
        
        [BoxGroup("Split/Right/Item Effects")]
        [SerializeField] private  ItemEffectData itemEffect;

        [BoxGroup("Split/Right/Item Effects")]
        [ShowInInspector, ShowIf("@itemEffect != null")]
        [InlineEditor(InlineEditorObjectFieldModes.Hidden)]
        private ItemEffectData ItemEffectDrawer
        {
            get => itemEffect;
            set => itemEffect = value;
        }

        [VerticalGroup("Split/Right")]
        [ListDrawerSettings(ShowFoldout =  false)]
        [SerializeField] private InventoryItem[] craftRecipe;

        public string SaveId => saveId;
        public int ItemPrice => itemPrice;
        public int MinStackSizeAtShop => minStackSizeAtShop;
        public int MaxStackSizeAtShop => maxStackSizeAtShop;
        public int Rarity => rarity;
        public float DropChance => dropChance;
        public float MaxDropChance => maxDropChance;
        public string ItemName => itemName;
        public Sprite ItemIcon => itemIcon;
        public ItemType ItemType => itemType;
        public int MaxStackSize => maxStackSize;
        public ItemEffectData ItemEffect => itemEffect;
        public InventoryItem[] CraftRecipe => craftRecipe;

        private void OnValidate()
        {
            dropChance = GetDropChance();

#if UNITY_EDITOR
            string path = AssetDatabase.GetAssetPath(this);
            saveId = AssetDatabase.AssetPathToGUID(path);
#endif
        }

        public float GetDropChance()
        {
            float maxRarity = 1000.0f;
            float chance = (maxRarity - rarity + 1) / maxRarity * 100;
            
            return Mathf.Min(chance, maxDropChance);
        }
    }
}