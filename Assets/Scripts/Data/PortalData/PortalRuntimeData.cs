// Done
using Core;
using UnityEngine;

namespace Data.PortalData
{
    [CreateAssetMenu(fileName = "Portal Runtime Data", menuName = "Echos of the Vale/Portal Data/Portal Runtime Data", order = 0)]
    public class PortalRuntimeData : ScriptableObject
    {
        [Header("Portal State")]
        [SerializeField] private string originSceneName;
        [SerializeField] private Vector2 originPortalPosition;
        [SerializeField] private Direction portalDirection = Direction.Right;
        [SerializeField] private bool arrivedViaPortal;
        [SerializeField] private bool isReturningToOrigin;

        public string OriginSceneName
        {
            get => originSceneName;
            set => originSceneName = value;
        }

        public Vector2 OriginPortalPosition
        {
            get => originPortalPosition;
            set => originPortalPosition = value;
        }

        public Direction PortalDirection
        {
            get => portalDirection;
            set => portalDirection = value;
        }

        public bool ArrivedViaPortal
        {
            get => arrivedViaPortal;
            set => arrivedViaPortal = value;
        }

        public bool IsReturningToOrigin
        {
            get => isReturningToOrigin;
            set => isReturningToOrigin = value;
        }

        public void ResetToDefaults()
        {
            originSceneName = null;
            originPortalPosition = Vector2.zero;
            portalDirection = Direction.Right;
            arrivedViaPortal = false;
            isReturningToOrigin = false;
        }

#if UNITY_EDITOR
        private void OnEnable()
        {
            UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void OnDisable()
        {
            UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        private void OnPlayModeStateChanged(UnityEditor.PlayModeStateChange state)
        {
            if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
            {
                ResetToDefaults();
            }
        }
#endif
    }
}