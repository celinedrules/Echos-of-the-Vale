// Done
using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace UI.SkillTree.Connections
{
    [Serializable]
    public class TreeConnection : MonoBehaviour
    {
        [InlineProperty, HideLabel]
        [OnValueChanged(nameof(UpdateLayout), true)]
        [SerializeField] private TreeConnectDetails connectionDetails;

        [OnValueChanged(nameof(UpdateLayout))]
        [SerializeField] private RectTransform rotationPoint;

        [OnValueChanged(nameof(UpdateLayout))]
        [SerializeField] private RectTransform connectionLength;

        [OnValueChanged(nameof(UpdateLayout))]
        [SerializeField] private RectTransform childNodeConnectionPoint;

        [SerializeField] private Image connectionImage;
        
        public TreeConnectDetails ConnectionDetails => connectionDetails;

        private RectTransform _parentNodeRect;
        private RectTransform _childNodeRect;

        private float _lastAngle = float.NaN;
        private float _lastDistance = float.NaN;

        private bool _warnedDifferentParents;

        private void Awake()
        {
            CacheRects();

            if (connectionDetails != null && connectionDetails.ChildNode != null)
                connectionDetails.ChildNode.SetConnectionImage(connectionImage);
        }

        private void OnEnable()
        {
            CacheRects();
        }

        private void OnValidate()
        {
            CacheRects();
            _warnedDifferentParents = false;
            _lastAngle = float.NaN;
            _lastDistance = float.NaN;
        }

        public void InitializeAfterWiring()
        {
            CacheRects();
            _warnedDifferentParents = false;
            _lastAngle = float.NaN;
            _lastDistance = float.NaN;

            if (connectionDetails != null && connectionDetails.ChildNode != null)
                connectionDetails.ChildNode.SetConnectionImage(connectionImage);

            UpdateConnection();
        }
        
        public void UpdateLayout()
        {
            UpdateConnection();
        }

        private void CacheRects()
        {
            _parentNodeRect = transform.parent as RectTransform;

            if (connectionDetails != null && connectionDetails.ChildNode != null)
                _childNodeRect = connectionDetails.ChildNode.transform as RectTransform;
            else
                _childNodeRect = null;
        }

        private void UpdateConnection()
        {
            if (connectionDetails == null || connectionDetails.ChildNode == null)
                return;

            // Refresh cache if something got re-wired / destroyed.
            if (_parentNodeRect == null || _childNodeRect == null)
                CacheRects();

            if (_parentNodeRect == null || _childNodeRect == null)
                return;

            // Both nodes should have the same parent in the hierarchy for this math.
            if (_parentNodeRect.parent != _childNodeRect.parent)
            {
#if UNITY_EDITOR
                if (!_warnedDifferentParents)
                {
                    _warnedDifferentParents = true;
                    Debug.LogWarning(
                        $"Parent and child nodes must share the same parent in hierarchy: {_parentNodeRect.name} and {_childNodeRect.name}",
                        this);
                }
#endif
                return;
            }

            Vector2 direction = _childNodeRect.anchoredPosition - _parentNodeRect.anchoredPosition;
            float distance = direction.magnitude;

            // Avoid Atan2 for zero-length direction
            if (distance < 0.001f)
                return;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // Apply rotation only if it changed enough
            if (rotationPoint != null && (!Mathf.Approximately(angle, _lastAngle)))
            {
                rotationPoint.localRotation = Quaternion.Euler(0f, 0f, angle);
                _lastAngle = angle;
            }

            // Apply length only if it changed enough
            if (connectionLength != null && (!Mathf.Approximately(distance, _lastDistance)))
            {
                Vector2 size = connectionLength.sizeDelta;
                size.x = distance;
                connectionLength.sizeDelta = size;
                _lastDistance = distance;
            }

            connectionDetails.Length = distance;
        }

        public Vector2 GetConnectionPoint(RectTransform rect)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rect.parent as RectTransform,
                childNodeConnectionPoint.position,
                null,
                out Vector2 localPosition);

            return localPosition;
        }

        public void UpdateConnectionColor(TreeConnectHandler targetChild, bool unlocked)
        {
            if (connectionDetails != null && connectionDetails.ChildNode == targetChild && connectionImage != null)
                connectionImage.color = unlocked ? Color.white : GetOriginalColor();
        }

        private Color GetOriginalColor()
        {
            return new Color(0.55f, 0.55f, 0.55f, 1f);
        }
    }
}