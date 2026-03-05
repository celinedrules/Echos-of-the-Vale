using Core;
using Core.Interfaces;
using Managers;
using SaveSystem;
using UnityEngine;
using Utilities.Enums;

namespace Interactables
{
    public class Chest : MonoBehaviour, IDamageable, ISavable
    {
        private static readonly int Open = Animator.StringToHash("Open");

        [Header("Open Details")]
        [SerializeField] private string chestId;
        [SerializeField] private bool canDropItems = true;
        [SerializeField] private Vector2 knockback;

        private Animator _animator;
        private Rigidbody2D _rBody;
        private EntityFx _entityFx;
        private EntityDropManager _dropManager;
        private bool _isOpened;

        private void Awake()
        {
            _animator = GetComponentInChildren<Animator>();
            _rBody = GetComponent<Rigidbody2D>();
            _entityFx = GetComponentInChildren<EntityFx>();
            _dropManager = GetComponent<EntityDropManager>();
        }

        // private void Start()
        // {
        //     // Check if already opened in runtime data
        //     if (GameManager.Instance.WorldRuntimeData.IsChestOpened(chestId))
        //     {
        //         OpenChest(false);
        //     }
        // }

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(chestId))
                chestId = System.Guid.NewGuid().ToString();
        }

        public bool TakeDamage(int amount, int elementalDamage, ElementType elementType, Entity attacker)
        {
            if (!canDropItems)
                return false;

            OpenChest(true);
            
            // Mark as opened in runtime data immediately
            //GameManager.Instance.WorldRuntimeData.MarkChestOpened(chestId);
            
            return true;
        }

        private void OpenChest(bool dropItems)
        {
            canDropItems = false;
            _isOpened = true;

            if (dropItems)
            {
                _animator.SetBool(Open, true);
                _dropManager?.DropItems();
                _entityFx?.Flash();
                _rBody.linearVelocity = knockback;
            }
            else
            {
                _animator.Play("ChestOpening", 0, 1f);
            }
        }

        public void SaveData(ref GameData gameData)
        {
            if (string.IsNullOrEmpty(chestId))
                return;

            // gameData.openedChests ??= new SerializableDictionary<string, bool>();
            //
            // if (_isOpened && !gameData.openedChests.ContainsKey(chestId))
            //     gameData.openedChests.Add(chestId, true);
        }

        public void LoadData(GameData gameData)
        {
            if (string.IsNullOrEmpty(chestId))
                return;

            // gameData.openedChests ??= new SerializableDictionary<string, bool>();
            //
            // if (gameData.openedChests.ContainsKey(chestId))
            //     OpenChest(false);
        }
    }
}