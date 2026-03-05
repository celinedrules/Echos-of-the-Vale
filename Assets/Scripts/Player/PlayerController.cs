using System;
using System.Collections;
using Core;
using Core.Interfaces;
using Data.EntityData;
using Data.InventoryData;
using Data.WeaponData;
using InventorySystem;
using StateMachine.States.PlayerStates;
using Stats;
using UnityEngine;

namespace Player
{
    public class PlayerController : Entity
    {
        public static event Action OnPlayerDeath;
        
        private PlayerStateFactory _factory;
        private Coroutine _queuedAttackCo;
        
        public InventoryPlayer Inventory { get; private set; }
        public PlayerInputHandler InputHandler { get; private set; }
        public EntityStatusHandler StatusHandler { get; private set; }
        
        public IdleState IdleState { get; private set; }
        public MoveState MoveState { get; private set; }
        //public DashState DashState { get; private set; }
        public BasicAttackState BasicAttackState { get; private set; }
        public CounterAttackState CounterAttackState { get; private set; }
        // public SwordThrowState SwordThrowState { get; private set; }
        public DeathState DeathState { get; private set; }
        
        // [field: SerializeField, Header("Skills")]
        // public SkillDash Dash { get; private set; }
        
        [field: SerializeField, Header("Movement Settings")]
        public float MoveSpeed { get; set; } = 5.0f;
        [field: SerializeField] public float DashSpeed { get; set; } = 20f;
        [field: SerializeField] public float DashDuration { get; set; } = 0.25f;
        
        [field: SerializeField, Header("Attack Settings")]
        public Animator SwordAnimator { get; set; }
        [field: SerializeField] public GameObject SwordParent { get; set; }
        [field: SerializeField] public SwordData ActiveSword { get; set; }
        [field: SerializeField] public Vector2[] AttackVelocity { get; set; }
        [field: SerializeField] public float AttackVelocityDuration { get; set; } = 0.1f;
        [field: SerializeField] public float ComboResetDuration { get; set; } = 1f;

        [field: SerializeField, Header("Effects")]
        public PlayerFx PlayerFx { get; set; }
        
        protected override Type RequiredDataType => typeof(PlayerData);
        
        protected override void Awake()
        {
            base.Awake();

            Stats = new PlayerStats();
            Inventory = GetComponent<InventoryPlayer>();

            SetupInventory();
            
            InputHandler = GetComponent<PlayerInputHandler>();
            Health = GetComponent<PlayerHealth>();
            StatusHandler = GetComponent<EntityStatusHandler>();
            
            if (SwordAnimator)
                SwordAnimator.keepAnimatorStateOnDisable = true;
            
            SwordAnimator.runtimeAnimatorController = ActiveSword.AnimatorController;
            
            _factory = new PlayerStateFactory(this, StateMachine);
            
            IdleState = _factory.Create<IdleState>("Idle");
            MoveState = _factory.Create<MoveState>("Move");
            //DashState = _factory.Create<DashState>("Dash");
            BasicAttackState = _factory.Create<BasicAttackState>("BasicAttack");
            CounterAttackState = _factory.Create<CounterAttackState>("CounterAttack");
            //SwordThrowState = _factory.Create<SwordThrowState>("SwordThrow");
            DeathState = _factory.Create<DeathState>("Death");
            
            // if (!Dash)
            //     Dash = GetComponentInChildren<SkillDash>();
        }

        protected override void Start()
        {
            base.Start();
            
            StateMachine.Initialize(IdleState);
            
             if (!Managers.GameManager.Instance.InventoryRuntimeData.HasValidData)
                 SetupEquipment();

            ((PlayerStats)Stats)?.LoadFromRuntimeData();
        }
        
        public void TeleportPlayer(Vector3 position) => transform.position = position;

        public void SetVisible(bool visible)
        {
            foreach (SpriteRenderer sr in GetComponentsInChildren<SpriteRenderer>())
                sr.enabled = visible;
        }
        
        private void SetupEquipment()
        {
            PlayerData playerData = entityData as PlayerData;

            if (!playerData)
                return;

            TryEquipStartingItem(playerData.EquipData.Weapon);
            TryEquipStartingItem(playerData.EquipData.Armor);
            TryEquipStartingItem(playerData.EquipData.Accessory1);
            TryEquipStartingItem(playerData.EquipData.Accessory2);
        }

        private void TryEquipStartingItem(InventoryItem sourceItem)
        {
            if (sourceItem?.ItemData == null)
                return;

            InventoryItem item = new InventoryItem(sourceItem.ItemData);
            Inventory.AddItem(item);
            Inventory.TryEquipItem(item);
        }
        
        private void SetupInventory()
        {
            PlayerData playerData = entityData as PlayerData;

            if (!playerData)
                return;

            Inventory.ItemList.Clear();

            foreach (InventoryItem quickItem in Inventory.QuickItems)
                quickItem.ItemData = null;

            foreach (ItemSlotData slotData in playerData.Inventory.Slots)
            {
                if (slotData.Item)
                {
                    InventoryItem item = new InventoryItem(slotData.Item)
                    {
                        StackSize = slotData.Amount
                    };

                    Inventory.AddItem(item);
                }
            }
        }
        
        public void SetVelocity(float velocityX, float velocityY)
        {
            if (IsKnockedBack)
                return;

            Rigidbody.linearVelocity = new Vector2(velocityX, velocityY);
            //HandleFlip(velocityX);
        }
        
        public void EnterAttackStateWithDelay()
        {
            if (_queuedAttackCo != null)
                StopCoroutine(_queuedAttackCo);

            _queuedAttackCo = StartCoroutine(EnterAttackStateWithDelayCo());
        }

        private IEnumerator EnterAttackStateWithDelayCo()
        {
            yield return new WaitForEndOfFrame();
            StateMachine.ChangeState(BasicAttackState);
        }
        
        public void EquipSword(SwordData sword)
        {
            ActiveSword = sword;
            
            if(SwordAnimator && sword)
                SwordAnimator.runtimeAnimatorController = sword.AnimatorController;
        }
        
        protected override IEnumerator SlowDownRoutine(float duration, float slowdownFactor)
        {
            float originalMoveSpeed = MoveSpeed;
            float originalAnimatorSpeed = Animator.speed;
            Vector2[] originalAttackVelocity = new Vector2[AttackVelocity.Length];
            Array.Copy(AttackVelocity, originalAttackVelocity, AttackVelocity.Length);

            float speedMultiplier = 1 - slowdownFactor;

            MoveSpeed *= speedMultiplier;
            Animator.speed *= speedMultiplier;

            for (int i = 0; i < AttackVelocity.Length; i++)
                AttackVelocity[i] *= speedMultiplier;

            yield return new WaitForSeconds(duration);

            MoveSpeed = originalMoveSpeed;
            Animator.speed = originalAnimatorSpeed;

            for (int i = 0; i < AttackVelocity.Length; i++)
                AttackVelocity[i] = originalAttackVelocity[i];
        }

        public override void EntityDeath()
        {
            base.EntityDeath();
            OnPlayerDeath?.Invoke();
            StateMachine.ChangeState(DeathState);
        }
        
        public void TryInteract()
        {
            Transform closest = null;
            float closestDistance = Mathf.Infinity;
            Collider2D[] objectsAround = Physics2D.OverlapCircleAll(transform.position, 1.0f, LayerMask.GetMask("Npc"));

            foreach (Collider2D objectAround in objectsAround)
            {
                IInteractable interactable = objectAround.GetComponent<IInteractable>();

                if (interactable == null)
                    continue;

                float distance = Vector2.Distance(transform.position, objectAround.transform.position);

                if (distance < closestDistance)
                {
                    closest = objectAround.transform;
                    closestDistance = distance;
                }
            }

            if (!closest)
                return;

            closest.GetComponent<IInteractable>().Interact();
        }
    }
}