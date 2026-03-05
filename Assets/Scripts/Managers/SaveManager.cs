using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Interfaces;
using SaveSystem;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utilities;

namespace Managers
{
    public class SaveManager : Singleton<SaveManager>
    {
        [SerializeField] private bool encryptData = true;
        [SerializeField] private int numberOfSaveSlots = 3;
        [ValueDropdown(nameof(GetSaveSlots))]
        [SerializeField] private int currentSaveSlot;

        private const string FileNamePrefix = "ultimaterpg";
        
        private FileDataHandler _dataHandler;
        private List<ISavable> _savables;
        private GameData _gameData;
        private string[] _fileNames;

        public GameData GameData => _gameData;
        public int NumberOfSaveSlots => numberOfSaveSlots;

        protected override void Awake()
        {
            base.Awake();
            _gameData ??= new GameData();
        }

        private IEnumerator Start()
        {
            Debug.Log(Application.persistentDataPath);
            _fileNames = new string[numberOfSaveSlots];

            for (int i = 0; i < numberOfSaveSlots; i++)
                _fileNames[i] = FileNamePrefix + i + ".json";
            
            _dataHandler = new FileDataHandler(Application.persistentDataPath, _fileNames[currentSaveSlot], encryptData);
            _savables = FindSavables();
            
            yield return new WaitForSeconds(0.01f);
        }
        
        private string GetFileName(int slot) => $"{FileNamePrefix}{slot}.json";
        
        [ButtonGroup("SaveButtons")]
        [Button("$GetLoadButtonText", ButtonSizes.Gigantic)]
        [GUIColor(0.4f, 1f, 0.4f)]
        private void LoadGame()
        {
            _gameData = _dataHandler.LoadData();

            if (_gameData == null)
            {
                Debug.Log("No save data found. Creating new save data.");
                _gameData = new GameData();
                return;
            }
            
            GameManager.Instance.SetSessionPlayTime(_gameData.playTimeSeconds);
            GameManager.Instance.Player.TeleportPlayer(_gameData.playerPosition);
            
            foreach(ISavable savable in _savables)
                savable.LoadData(_gameData);
        }
        
        public void LoadGameFromSlot(int slotIndex)
        {
            _dataHandler = new FileDataHandler(Application.persistentDataPath, GetFileName(slotIndex), encryptData);
            currentSaveSlot = slotIndex;
            
            _gameData = _dataHandler.LoadData();
            
            if (_gameData == null)
            {
                Debug.LogWarning("No save data found for this slot.");
                return;
            }
            
            string currentScene = SceneManager.GetActiveScene().name;
            string savedScene = _gameData.sceneName;
            
            if (!string.IsNullOrEmpty(savedScene) && savedScene != currentScene)
            {
                // Load the saved scene, then apply save data after it loads
                StartCoroutine(LoadSceneAndApplyData(savedScene));
            }
            else
            {
                Time.timeScale = 1f;
                // Same scene, just apply data
                ApplyLoadedData();
            }
        }
        
        private IEnumerator LoadSceneAndApplyData(string sceneName)
        {
            // Unpause before loading scene
            Time.timeScale = 1f;
            
            AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneName);
            
            while (!loadOperation.isDone)
                yield return null;
            
            // Wait a couple frames for scene objects to fully initialize
            yield return null;
            yield return null;
            
            // Re-find savables in the new scene
            _savables = FindSavables();
            
            ApplyLoadedData();
        }
        
        private void ApplyLoadedData()
        {
            // Unpause the game (in case it was paused from menu)
            Time.timeScale = 1f;
            
            GameManager.Instance.SetSessionPlayTime(_gameData.playTimeSeconds);
            
            // Get fresh player reference after scene load
            var player = GameManager.Instance.Player;
            if (player != null)
            {
                player.TeleportPlayer(_gameData.playerPosition);
            }
            else
            {
                Debug.LogError("Player not found after scene load!");
            }
            
            // Apply audio settings
            // FusionAudioManager.Instance.SetTrackVolume(AudioTrackType.Background, _gameData.bgmVolume);
            // FusionAudioManager.Instance.SetTrackVolume(AudioTrackType.PlayerFX, _gameData.sfxVolume);
            // FusionAudioManager.Instance.SetSpatialAudioVolume(_gameData.sfxVolume);

            
            foreach (ISavable savable in _savables)
                savable.LoadData(_gameData);
        }
        
        [ButtonGroup("SaveButtons")]
        [Button("$GetSaveButtonText", ButtonSizes.Gigantic)]
        public void SaveGame()
        {
            SaveGameToSlot(currentSaveSlot);
        }

        public SaveMetadata SaveGameToSlot(int slotIndex)
        {
            // Switch to the target slot
            _dataHandler = new FileDataHandler(Application.persistentDataPath, GetFileName(slotIndex), encryptData);
            
            _gameData ??= new GameData();

            // Re-find savables in case scene changed
            _savables = FindSavables();

            foreach (ISavable savable in _savables)
                savable.SaveData(ref _gameData);

            _dataHandler.SaveData(_gameData);

            currentSaveSlot = slotIndex;
        
            return _dataHandler.LoadMetadata();
        }
        
        [ButtonGroup("SaveButtons")]
        [Button("$GetDeleteButtonText", ButtonSizes.Gigantic)]
        [GUIColor(1f, 0.4f, 0.4f)]
        public void DeleteSaveGame()
        {
            if (currentSaveSlot < 0 || currentSaveSlot >= numberOfSaveSlots)
            {
                Debug.LogError($"Invalid save slot: {currentSaveSlot}");
                return;
            }
            
            _dataHandler =  new FileDataHandler(Application.persistentDataPath, GetFileName(currentSaveSlot), encryptData);
            _dataHandler.Delete();
        }
        
        private List<ISavable> FindSavables()
        {
            return FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .OfType<ISavable>().ToList();
        }
        
        private IEnumerable<ValueDropdownItem<int>> GetSaveSlots()
        {
            for (int i = 0; i < numberOfSaveSlots; i++)
                yield return new ValueDropdownItem<int>($"Slot {i}", i);
        }

        private string GetLoadButtonText() => $"Load game";// from slot: {currentSaveSlot}";
        private string GetSaveButtonText() => $"Save game";// in slot: {currentSaveSlot}";
        private string GetDeleteButtonText() => $"Delete save";// in slot: {currentSaveSlot}";
    }
}