using System.Collections.Generic;
using InventorySystem;
using Managers;
using Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities.Enums;

namespace UI.Hud
{
    public class Hud : MonoBehaviour
    {
        [SerializeField] private RectTransform healthRect;
        [SerializeField] private Slider healthSlider;
        [SerializeField] private TextMeshProUGUI healthText;

        [Header("Quick Item Slots")]
        [SerializeField] private float yOffsetQuickItemParent = 150.0f;
        [SerializeField] private Transform quickItemOptionsParent;

        private const float BaseHealthBarSize = 150f;
        private const float HealthBarGrowthRate = 0.25f; // 25% of health increase
        private int _initialMaxHealth;
        private PlayerController _player;
        private InventoryPlayer _inventory;
        // private SkillSlot[] _skillSlots;
        private QuickItemSlotOption[] _quickItemOptions;
        private QuickItemSlot[] _quickItemSlots;
        
        private void Start()
        {
            _quickItemSlots = GetComponentsInChildren<QuickItemSlot>();
            
            _player = GameManager.Instance.Player;
            _initialMaxHealth = _player.Stats.GetMaxHealth();
            _player.Health.OnHealthUpdate += UpdateHealthBar;

            UpdateHealthBar();
            
            //_skillSlots = GetComponentsInChildren<SkillSlot>(true);
            
            _inventory = _player.Inventory;
            _inventory.OnInventoryChanged += UpdateQuickSlots;
            _inventory.OnQuickSlotUse += PlayQuickSlotFeedback;
        }
        
        public void PlayQuickSlotFeedback(int slotNumber) => _quickItemSlots[slotNumber].SimilulateButtonFeedback();
        
        public void UpdateQuickSlots()
        {
            InventoryItem[] quickItems = _inventory.QuickItems;
            
            for (int i = 0; i < quickItems.Length; i++)
                _quickItemSlots[i].UpdateQuickSlot(quickItems[i]);
        }
        
        
        public void OpenQuickItemOptions(QuickItemSlot quickItemSlot, RectTransform targetRect)
        {
            if(_quickItemOptions == null || _quickItemOptions.Length == 0)
                _quickItemOptions = quickItemOptionsParent.GetComponentsInChildren<QuickItemSlotOption>(true);
            
            List<InventoryItem> consumables = _inventory.ItemList.FindAll(i => i.ItemData.ItemType == ItemType.Consumable);
            
            if(consumables.Count == 0)
                return;
        
            for (int i = 0; i < _quickItemOptions.Length; i++)
            {
                if (i < consumables.Count)
                {
                    _quickItemOptions[i].gameObject.SetActive(true);
                    _quickItemOptions[i].SetupOption(quickItemSlot, consumables[i]);
                }
                else
                {
                    _quickItemOptions[i].gameObject.SetActive(false);
                }
            }
            
            quickItemOptionsParent.position = targetRect.position + Vector3.up * yOffsetQuickItemParent;
            //UiManager.Instance.OpenQuickItemSlotOptions();
        }

        
        public void HideQuickItemOptions() => UiManager.Instance.TryCloseActiveUi();

        // public SkillSlot GetSkillSlot(SkillType skillType)
        // {
        //     _skillSlots ??= GetComponentsInChildren<SkillSlot>(true);
        //
        //     foreach (SkillSlot slot in _skillSlots)
        //     {
        //         if (slot.SkillType == skillType)
        //         {
        //             slot.gameObject.SetActive(true);
        //             return slot;
        //         }
        //     }
        //
        //     return null;
        // }
        
        public void UpdateHealthBar()
        {
            int currentHealth = _player.Health.CurrentHealth;
            int maxHealth = _player.Stats.GetMaxHealth();
            
            int healthIncrease = maxHealth - _initialMaxHealth;
            float targetWidth = BaseHealthBarSize + (healthIncrease * HealthBarGrowthRate);
            float sizeDifference = Mathf.Abs(targetWidth - healthRect.sizeDelta.x);
            
            if (sizeDifference > 0.1f)
                healthRect.sizeDelta = new Vector2(targetWidth, healthRect.sizeDelta.y);
            
            healthText.text = $"{currentHealth}/{maxHealth}";
            healthSlider.value = _player.Health.GetHealthPercentage();
        }
    }
}