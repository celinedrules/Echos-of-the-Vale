using Data.ItemData;
using InventorySystem;
using Managers;
using UnityEngine;

namespace Interactables
{
    public class ItemPickup : MonoBehaviour
    {
        [SerializeField] private string pickupId;
        [SerializeField] private Vector2 dropForce = new Vector2(3.0f, 10.0f);
        [SerializeField] private ItemData itemData;
        [SerializeField, HideInInspector] private int lastKnownInstanceId;
        
        private SpriteRenderer _spriteRenderer;
        //private Rigidbody2D _rigidbody;
        private Collider2D _collider;
        private bool _isCollected;
        
        public string PickupId => pickupId;
        
        private void Awake()
        {
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            //_rigidbody = GetComponent<Rigidbody2D>();
            _collider = GetComponent<Collider2D>();
        }

        private void Start()
        {
            // Check if already collected in runtime data
            // if (!string.IsNullOrEmpty(pickupId) && GameManager.Instance.WorldRuntimeData.IsPickupCollected(pickupId))
            //     Destroy(gameObject);
        }
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            // Only run in edit mode, not during play
            if (Application.isPlaying)
                return;
            
            if(!_spriteRenderer)
                _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            
            if(itemData)
                SetupVisuals();
            
            int currentInstanceId = GetInstanceID();
            
            // Generate new ID if empty OR if this was duplicated (different instance ID)
            if (string.IsNullOrEmpty(pickupId) || (lastKnownInstanceId != currentInstanceId && lastKnownInstanceId != 0))
            {
                pickupId = System.Guid.NewGuid().ToString();
                UnityEditor.EditorUtility.SetDirty(this);
            }
            
            lastKnownInstanceId = currentInstanceId;
        }
#endif
        
        public void SetupItem(ItemData newItemData)
        {
            itemData = newItemData;
            SetupVisuals();

            if(GameManager.Instance.Player.TryGetComponent(out Collider2D playerCollider))
                Physics2D.IgnoreCollision(_collider, playerCollider);

            float dropForceX = Random.Range(-dropForce.x, dropForce.x);
            //_rigidbody.linearVelocity = new Vector2(dropForceX, dropForce.y);
            _collider.isTrigger = false;

            pickupId = "";
        }
        
        private void SetupVisuals()
        {
            _spriteRenderer.sprite = itemData.ItemIcon;
            gameObject.name = "Object Item Pickup - " + itemData.ItemName;
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if(!other.CompareTag("Player"))
                return;
        
            InventoryPlayer inventory = other.GetComponent<InventoryPlayer>(); 
        
            if(!inventory)
                return;

            InventoryItem itemToAdd = new(itemData);
        
            if (!inventory.CanAddItem(itemToAdd))
                return;
        
            inventory.AddItem(itemToAdd);
            _isCollected = true;
            
            //QuestManager.Instance.CheckImmediateDeliverQuests();
            
            // Mark as collected in runtime data immediately
            //GameManager.Instance.WorldRuntimeData.MarkPickupCollected(pickupId);
            
            Destroy(gameObject);
        }
        
        // public void SaveData(ref GameData gameData)
        // {
        //     if (string.IsNullOrEmpty(pickupId))
        //         return;
        //
        //     if (_isCollected && !gameData.collectedPickups.ContainsKey(pickupId))
        //         gameData.collectedPickups.Add(pickupId, true);
        // }
        
        // public void LoadData(GameData gameData)
        // {
        //     if (string.IsNullOrEmpty(pickupId))
        //         return;
        //
        //     if (gameData.collectedPickups.ContainsKey(pickupId))
        //         Destroy(gameObject);
        // }
    }
}