using System;
using Data.InventoryData;
using Player;
using UnityEngine;
using Utilities;

namespace Managers
{
    public class GameManager : Singleton<GameManager>
    {
        [SerializeField] private PlayerController player;
        [SerializeField] private InventoryRuntimeData inventoryRuntimeData;

        public PlayerController Player => player ? player : FindPlayer();
        public InventoryRuntimeData InventoryRuntimeData => inventoryRuntimeData;

        private void Update()
        {
            TimerManager.Instance.Update(Time.deltaTime, Time.unscaledDeltaTime);
        }

        private PlayerController FindPlayer()
        {
            if (!player)
                player = FindFirstObjectByType<PlayerController>();

            return player;
        }
    }
}