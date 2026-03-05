using System;
using System.IO;
using Managers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SaveSystem
{
    public class FileDataHandler
    {
        private string _fullPath;
        private string _metaPath;
        private bool _encryptData;
        private string _codeWord = "secretKey123";
    
        public FileDataHandler(string dataDirPath, string dataFileName, bool encryptData)
        {
            _fullPath = Path.Combine(dataDirPath, dataFileName);
            _metaPath = Path.Combine(dataDirPath, Path.GetFileNameWithoutExtension(dataFileName) + ".meta.json");
            _encryptData = encryptData;
        }

        public void SaveData(GameData gameData)
        {
            try
            {
                string directoryPath = Path.GetDirectoryName(_fullPath);
    
                if (!string.IsNullOrEmpty(directoryPath))
                    Directory.CreateDirectory(directoryPath);

                // Update metadata fields before saving
                gameData.lastSaved = DateTime.Now.ToString("o"); // ISO 8601 format
                gameData.playTimeSeconds = GameManager.Instance.SessionPlayTime;
                
                gameData.characterName = GameManager.Instance.Player.EntityName;
                gameData.location = GameManager.Instance.CurrentLocationName;
                gameData.sceneName = SceneManager.GetActiveScene().name;
                gameData.playerPosition = GameManager.Instance.Player.transform.position;
                gameData.crystals = 42;
                
                // Save options settings
                // gameData.bgmVolume = FusionAudioManager.Instance.GetTrackTargetVolume(AudioTrackType.Background);
                // gameData.sfxVolume = FusionAudioManager.Instance.GetTrackTargetVolume(AudioTrackType.PlayerFX);
                gameData.showHealthBar = GameManager.Instance.Player?.Health?.MiniHealthBarActive ?? true;


                string dataToSave = JsonUtility.ToJson(gameData, true);
            
                if (_encryptData)
                    dataToSave = EncryptDecrypt(dataToSave);
            
                using (FileStream stream = new(_fullPath, FileMode.Create))
                {
                    using (StreamWriter writer = new(stream))
                    {
                        writer.Write(dataToSave);
                    }
                }
                
                // Save metadata file (never encrypted for quick reads)
                SaveMetadata(gameData);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error saving game data: {ex.Message}");
            }
        }

        private void SaveMetadata(GameData gameData)
        {
            var metadata = new SaveMetadata
            {
                characterName = gameData.characterName,
                location = gameData.location,
                lastSaved = gameData.lastSaved,
                playTimeSeconds = gameData.playTimeSeconds,
                gold = gameData.gold,
                crystals = gameData.crystals
            };
            
            string metaJson = JsonUtility.ToJson(metadata, true);
            
            using (FileStream stream = new(_metaPath, FileMode.Create))
            {
                using (StreamWriter writer = new(stream))
                {
                    writer.Write(metaJson);
                }
            }
        }

        public SaveMetadata LoadMetadata()
        {
            if (!File.Exists(_metaPath))
                return null;

            try
            {
                using (FileStream stream = new(_metaPath, FileMode.Open))
                {
                    using (StreamReader reader = new(stream))
                    {
                        string json = reader.ReadToEnd();
                        return JsonUtility.FromJson<SaveMetadata>(json);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error loading metadata: {ex.Message}");
                return null;
            }
        }

        public GameData LoadData()
        {
            GameData gameData = null;

            if (File.Exists(_fullPath))
            {
                try
                {
                    string dataToLoad;

                    using (FileStream stream = new FileStream(_fullPath, FileMode.Open))
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            dataToLoad = reader.ReadToEnd();
                        }
                    }
                    
                    if(_encryptData)
                        dataToLoad = EncryptDecrypt(dataToLoad);
                    
                    gameData = JsonUtility.FromJson<GameData>(dataToLoad);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error loading game data: {ex.Message}");
                }
            }
            
            return gameData;
        }

        public void Delete()
        {
            if (File.Exists(_fullPath))
                File.Delete(_fullPath);
            
            if (File.Exists(_metaPath))
                File.Delete(_metaPath);
        }

        private string EncryptDecrypt(string data)
        {
            char[] modifiedData = new char[data.Length];

            for (int i = 0; i < data.Length; i++)
                modifiedData[i] = (char)(data[i] ^ _codeWord[i % _codeWord.Length]);
        
            return new string(modifiedData);
        }
    }
}