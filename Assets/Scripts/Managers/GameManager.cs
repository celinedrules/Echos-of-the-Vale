// Done
using System.Collections;
using Audio;
using Core;
using Data;
using Data.InventoryData;
using Data.QuestData;
using Data.SkillData;
using Data.StatsData;
using Data.WorldData;
using Interactables;
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
        [SerializeField] private bool resetStatsOnStart;
        [SerializeField] private LocationDatabase locationDatabase;
        [SerializeField] private WorldRuntimeData worldRuntimeData;
        [SerializeField] private InventoryRuntimeData inventoryRuntimeData;
        [SerializeField] private SkillRuntimeData skillRuntimeData;
        [SerializeField] private StatsRuntimeData statsRuntimeData;
        [SerializeField] private QuestRuntimeData questRuntimeData;
        
        private float _sessionPlayTime;
        private string _pendingWaypointId;
        private bool _isTransitioning;
        private bool _isRespawning;
        private bool _pendingPortal;
        private Vector2 _pendingPortalPosition;
        private bool _keepBackgroundMusic;

        public PlayerController Player => player ? player : FindPlayer();
        public float SessionPlayTime => _sessionPlayTime;
        public WorldRuntimeData WorldRuntimeData => worldRuntimeData;
        public InventoryRuntimeData InventoryRuntimeData => inventoryRuntimeData;
        public SkillRuntimeData SkillRuntimeData => skillRuntimeData;
        public StatsRuntimeData StatsRuntimeData => statsRuntimeData;
        public QuestRuntimeData QuestRuntimeData => questRuntimeData;

        public Vector2? LastWaypointExitPosition { get; set; }
        public float LastWaypointExitTime { get; private set; }
        public bool KeepBackgroundMusic
        {
            set => _keepBackgroundMusic = value;
        }
        
        public string CurrentLocationName =>
            locationDatabase.GetDisplayName(SceneManager.GetActiveScene().name);
        
        private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
        private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

        protected override void Awake()
        {
            base.Awake();

            FindPlayer();

            if (resetStatsOnStart)
            {
                Entity[] entities = FindObjectsByType<Entity>(FindObjectsInactive.Include, FindObjectsSortMode.None);

                foreach (Entity entity in entities)
                    entity.ResetStats();
            }
        }
        
        private void Start() => inventoryRuntimeData?.ResetToDefaults();

        private void Update()
        {
            _sessionPlayTime += Time.unscaledDeltaTime;
            TimerManager.Instance.Update(Time.deltaTime, Time.unscaledDeltaTime);
        }
        
        public void SetSessionPlayTime(float loadedTime) => _sessionPlayTime = loadedTime;

        private PlayerController FindPlayer()
        {
            if (!player)
                player = FindFirstObjectByType<PlayerController>();

            return player;
        }
        
        public void SetLastWaypointExit(Vector2 position)
        {
            LastWaypointExitPosition = position;
            LastWaypointExitTime = Time.time;
        }
        
        public void SetPendingPortal(Vector2 position)
        {
            _pendingPortal = true;
            _pendingPortalPosition = position;
        }
        
        public void RestartScene()
        {
            _isRespawning = true;
            string sceneName = SceneManager.GetActiveScene().name;
            LoadScene(sceneName);
        }
        
        public void LoadScene(string sceneName, string targetWaypointId = null)
        {
            if (_isTransitioning)
                return;

            if (sceneName == "DefaultScene")
                sceneName = defaultScene.name;
            
            if(!_keepBackgroundMusic)
                FusionAudioManager.Instance.StopTrack(AudioTrackType.Background, ScreenFader.Instance.FadeDuration);
            
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
        
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            FindPlayer();
            StartCoroutine(OnSceneLoadedRoutine());
        }
        
        private IEnumerator OnSceneLoadedRoutine()
        {
            if (_isRespawning)
            {
                Vector2? respawnPos = GetRespawnPosition();

                if (respawnPos.HasValue && player)
                    player.TeleportPlayer(respawnPos.Value);
                _isRespawning = false;
            }
            else if (_pendingPortal)
            {
                if (player && Portal.Instance)
                    player.TeleportPlayer(_pendingPortalPosition);

                _pendingPortal = false;
            }
            else
            {
                TeleportPlayerToWaypoint();
            }

            // Wait a frame for physics/position to settle
            yield return null;

            // Fade back in
            if (ScreenFader.Instance)
                yield return ScreenFader.Instance.FadeInCoroutine();

            _isTransitioning = false;
        }
        
        private void TeleportPlayerToWaypoint()
        {
            if (string.IsNullOrEmpty(_pendingWaypointId) || !player)
                return;

            Waypoint[] waypoints = FindObjectsByType<Waypoint>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (Waypoint waypoint in waypoints)
            {
                if (waypoint.WaypointId == _pendingWaypointId)
                {
                    player.TeleportPlayer(waypoint.transform.position);
                    waypoint.DisableTemporarily();
                    break;
                }
            }

            _pendingWaypointId = null;
        }

        private Vector2? GetRespawnPosition()
        {
            if (LastWaypointExitPosition.HasValue)
                return LastWaypointExitPosition;
            
            return player.transform.position;
        }
        
        public void ReturnToMainMenu() => LoadScene(mainMenu.name);
        
        public void SaveRuntimeData()
        {
            Player?.Inventory?.SaveToRuntimeData();
            ((PlayerStats)Player?.Stats)?.SaveToRuntimeData();
            UiManager.Instance.Storage?.InventoryStorage?.SaveToRuntimeData();
            
            if (SkillManager.Exists)
                SkillManager.Instance.SkillTree?.SaveToRuntimeData();
            
            if(Portal.Exists)
                Portal.Instance?.SaveToRuntimeData();
            
            if(QuestManager.Exists)
                QuestManager.Instance?.SaveToRuntimeData();
        }
        
        public void ClearAllRuntimeData()
        {
            worldRuntimeData?.ResetToDefaults();
            inventoryRuntimeData?.ResetToDefaults();
            skillRuntimeData?.ResetToDefaults();
            statsRuntimeData?.ResetToDefaults();
            questRuntimeData?.ResetToDefaults();
        }
    }
}