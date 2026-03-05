using Core;
using Core.Interfaces;
using Data.EntityData;
using InventorySystem;
using Managers;
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
            HandleNpcFlip();
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

        private void HandleNpcFlip()
        {
            if (!Player)
                return;

            Direction previousDirection = _facingDirection;

            if (Animator.transform.position.x > Player.position.x && _facingDirection == Direction.Right)
                _facingDirection = Direction.Left;
            else if (Animator.transform.position.x < Player.position.x && _facingDirection == Direction.Left)
                _facingDirection = Direction.Right;

            if (previousDirection != _facingDirection)
            {
                Vector3 scale = Animator.transform.localScale;
                scale.x = Mathf.Abs(scale.x) * (int)_facingDirection;
                Animator.transform.localScale = scale;
            }
        }

        protected virtual void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player"))
                return;

            if (other.gameObject.layer == LayerMask.NameToLayer("PlayerAttack"))
                return;

            Player = other.transform;
            Inventory = Player.GetComponent<InventoryPlayer>();
            Storage?.SetInventory(Inventory);
            Merchant?.SetInventory(Inventory);
            interactTooltip.SetActive(true);
        }

        protected virtual void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag("Player"))
                return;

            if (other.gameObject.layer == LayerMask.NameToLayer("PlayerAttack"))
                return;

            interactTooltip.SetActive(false);
        }

        public virtual void Interact()
        {
            // Debug.Log("Interact");
            // QuestManager.Instance.AddProgress(data.QuestTargetId);
            //
            // if (data.DialogueTable == null)
            // {
            //     Debug.LogWarning($"No DialogueTable assigned to {name}");
            //     return;
            // }
            //
            // DialogueRow startRow = data.DialogueTable.GetRowById(data.StartRowId);
            // if (startRow == null)
            // {
            //     Debug.LogWarning($"Row ID {data.StartRowId} not found in {data.DialogueTable.name}");
            //     return;
            // }
            //
            // UiManager.OpenDialogue();
            // UiManager.Dialogue.SetupNpcData(new DialogueNpcData(data.QuestTargetId, data.RewardType, data.Quests));
            // UiManager.Dialogue.PlayDialogue(data.DialogueTable, data.StartRowId);
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