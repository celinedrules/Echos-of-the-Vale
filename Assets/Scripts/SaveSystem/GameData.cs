// Done
using System;
using UnityEngine;
using Utilities.Enums;

namespace SaveSystem
{
    [Serializable]
    public class GameData
    {
        public string characterName;
        public string location;
        public string sceneName;
        public string lastSaved;
        public float playTimeSeconds;
        public int gold;
        public int crystals;
        
        public SerializableDictionary<string, int> inventory;
        public SerializableDictionary<string, int> storageItems;
        public SerializableDictionary<string, int> storageMaterials;
        public SerializableDictionary<string, ItemType> equipedItems;

        public int skillPoints;
        public SerializableDictionary<string, bool> skillTree;
        public SerializableDictionary<SkillType, SkillUpgradeType> skillUpgrades;

        public Vector2 savedCheckpoint;
        public Vector2 playerPosition;
        public SerializableDictionary<string, bool> activatedCheckpoints;

        public SerializableDictionary<string, bool> collectedPickups;
        public SerializableDictionary<string, bool> openedChests;

        public SerializableDictionary<string, bool> completedQuests; // Save ID, Complete Status
        public SerializableDictionary<string, int> activeQuests; // Save ID, Progress
        
        // Options settings
        public float bgmVolume = 1f;
        public float sfxVolume = 1f;
        public bool showHealthBar = true;
        
        public GameData()
        {
            inventory = new SerializableDictionary<string, int>();
            storageItems = new SerializableDictionary<string, int>();
            storageMaterials = new SerializableDictionary<string, int>();
            equipedItems = new SerializableDictionary<string, ItemType>();
            skillTree = new SerializableDictionary<string, bool>();
            skillUpgrades = new SerializableDictionary<SkillType, SkillUpgradeType>();
            activatedCheckpoints = new SerializableDictionary<string, bool>();
            collectedPickups = new SerializableDictionary<string, bool>();
            openedChests = new SerializableDictionary<string, bool>();
            completedQuests = new SerializableDictionary<string, bool>();
            activeQuests = new SerializableDictionary<string, int>();
        }
    }
}