using Core;
using Core.Interfaces;
using Data.DialogueData;
using Data.EntityData;
using InventorySystem;
using Managers;
using Player;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Interactables
{
    public class Npc : MonoBehaviour, IInteractable
    {
        protected static readonly int IsBlacksmith = Animator.StringToHash("IsBlacksmith");

        [Header("Npc Data")]
        [PropertyOrder(-10)]
        [ValidateInput(nameof(ValidateNpcData), "Wrong NpcData type for this NPC!")]
        [LabelText("$DataLabel")]
        [SerializeField] private NpcData data;

        [Header("Tooltip")]
        [SerializeField] private GameObject interactTooltip;
        [SerializeField] private float floatSpeed = 8.0f;
        [SerializeField] private float floatRange = 0.1f;

        protected Transform Player;
        protected InventoryPlayer Inventory;
        protected InventoryStorage Storage;
        protected InventoryMerchant Merchant;
        protected UiManager UiManager;
        protected Animator Animator;

        private Direction _facingDirection = Direction.Right;
        private Vector3 _startPosition;
        private bool _playerInRange;

        public NpcData Data => data;
        
        // public bool HasQuests => data.Quests != null && data.Quests.Length > 0;
        // public QuestData[] Quests => data.Quests;

        protected virtual System.Type RequiredDataType => typeof(NpcData);

        protected virtual void Awake()
        {
            UiManager = UiManager.Instance;
            Storage = GetComponent<InventoryStorage>();
            Merchant = GetComponent<InventoryMerchant>();
            Animator = GetComponentInChildren<Animator>();
            _startPosition = interactTooltip.transform.position;
            interactTooltip.SetActive(false);
        }

        protected virtual void Update()
        {
            HandleTooltipFloat();
        }

        private void HandleTooltipFloat()
        {
            if (interactTooltip.activeSelf)
            {
                float yOffset = Mathf.Sin(Time.time * floatSpeed) * floatRange;
                interactTooltip.transform.position = _startPosition + new Vector3(0, yOffset, 0);
            }
        }

        protected virtual void OnTriggerEnter2D(Collider2D other)
        {
            if (!IsValidInteractionCollider(other))
                return;

            if (!CachePlayerReferences(other))
                return;

            _playerInRange = true;
            RegisterWithPlayer();
            interactTooltip.SetActive(true);
        }

        protected virtual void OnTriggerExit2D(Collider2D other)
        {
            if (!IsValidInteractionCollider(other))
                return;

            UnregisterFromPlayer();
            _playerInRange = false;
            Player = null;
            Inventory = null;
            interactTooltip.SetActive(false);
        }

        private bool IsValidInteractionCollider(Collider2D other)
        {
            return other.GetComponentInChildren<PlayerInteractionCollider>() != null;
        }

        protected bool CachePlayerReferences(Collider2D other)
        {
            InventoryPlayer inventory = other.GetComponentInParent<InventoryPlayer>();

            if (!inventory)
                return false;

            Inventory = inventory;
            Player = inventory.transform;
            Storage?.SetInventory(Inventory);
            Merchant?.SetInventory(Inventory);

            return true;
        }

        private void RegisterWithPlayer()
        {
            PlayerController playerController = Player ? Player.GetComponent<PlayerController>() : null;
            playerController?.RegisterInteractable(this);
        }

        private void UnregisterFromPlayer()
        {
            PlayerController playerController = Player ? Player.GetComponent<PlayerController>() : null;
            playerController?.UnregisterInteractable(this);
        }

        protected bool CanInteract()
        {
            return _playerInRange && Player && Inventory;
        }

        public virtual void Interact()
        {
            if (!CanInteract())
                return;

            // QuestManager.Instance.AddProgress(data.QuestTargetId);
            
            if (data.DialogueTable == null)
            {
                Debug.LogWarning($"No DialogueTable assigned to {name}");
                return;
            }

            DialogueRow startRow = data.DialogueTable.GetRowById(data.StartRowId);
            if (startRow == null)
            {
                Debug.LogWarning($"Row ID {data.StartRowId} not found in {data.DialogueTable.name}");
                return;
            }

            UiManager.OpenDialogue();
            UiManager.Dialogue.SetupNpcData(new DialogueNpcData(data.QuestTargetId, data.RewardType, data.Quests));
            UiManager.Dialogue.PlayDialogue(data.DialogueTable, data.StartRowId);
        }

        private string DataLabel => RequiredDataType.Name.Replace("NpcData", "") is { Length: > 0 } label
            ? $"{label} Data"
            : "Npc Data";

        private bool ValidateNpcData(NpcData value, ref string errorMessage)
        {
            if (!value)
            {
                errorMessage = "NpcData cannot be null";
                return false;
            }

            if (RequiredDataType.IsInstanceOfType(value))
                return true;

            errorMessage = $"Expected {RequiredDataType.Name} but got {value.GetType().Name}";
            return false;
        }
    }
}