using System;
using System.Collections;
using Data.InventoryData;
using Player;
using Stats;
using UI.Common;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utilities;

namespace Managers
{
    public class GameManager : Singleton<GameManager>
    {
        [SerializeField] private SceneAsset defaultScene;
        [SerializeField] private SceneAsset mainMenu;
        [SerializeField] private PlayerController player;
        [SerializeField] private InventoryRuntimeData inventoryRuntimeData;
        
        private float _sessionPlayTime;
        private string _pendingWaypointId;
        private bool _isTransitioning;
        private bool _isRespawning;
        private bool _pendingPortal;
        private Vector2 _pendingPortalPosition;
        private bool _keepBackgroundMusic;

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
        
        public void LoadScene(string sceneName, string targetWaypointId = null)
        {
            if (_isTransitioning)
                return;

            if (sceneName == "DefaultScene")
                sceneName = defaultScene.name;
            
            // if(!_keepBackgroundMusic)
            //     FusionAudioManager.Instance.StopTrack(AudioTrackType.Background, ScreenFader.Instance.FadeDuration);
            
            StartCoroutine(LoadSceneRoutine(sceneName, targetWaypointId));
        }

        private IEnumerator LoadSceneRoutine(string sceneName, string targetWaypointId)
        {
            _isTransitioning = true;

            // Fade to black
            if (ScreenFader.Instance)
                yield return ScreenFader.Instance.FadeOutCoroutine();

            SaveRuntimeData();
            _pendingWaypointId = targetWaypointId;
            _keepBackgroundMusic = false;
            SceneManager.LoadScene(sceneName);
        }
        
        public void SaveRuntimeData()
        {
            // Player?.Inventory?.SaveToRuntimeData();
            // ((PlayerStats)Player?.Stats)?.SaveToRuntimeData();
            // UiManager.Instance.Storage?.InventoryStorage?.SaveToRuntimeData();
            //
            // if (SkillManager.Exists)
            //     SkillManager.Instance.SkillTree?.SaveToRuntimeData();
            //
            // if(Portal.Exists)
            //     Portal.Instance?.SaveToRuntimeData();
            //
            // if(QuestManager.Exists)
            //     QuestManager.Instance?.SaveToRuntimeData();
        }
    }
}