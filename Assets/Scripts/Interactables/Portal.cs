// Done
using System.Collections;
using Core;
using Core.Attributes;
using Data.PortalData;
using Managers;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utilities;

namespace Interactables
{
    public class Portal : Singleton<Portal>
    {
        [SerializeField, LocationDropdown] private string destinationScene;
        [SerializeField] private PortalRuntimeData runtimeData;

        [Header("Animation")]
        [SerializeField] private float animationDuration = 0.5f;
        [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] private bool keepBackgroundMusic;

        private Vector2 _defaultPosition;
        private bool _canBeTriggered = true;
        private string _currentSceneName;
        private Direction _direction = Direction.Right;
        
        private bool _pendingArrivedViaPortal;
        private bool _pendingIsReturningToOrigin;

        private Collider2D _collider;
        private SpriteRenderer _spriteRenderer;
        private Vector3 _originalScale;

        protected override void Awake()
        {
            base.Awake();

            _defaultPosition = transform.position;

            _collider = GetComponent<Collider2D>();
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            _originalScale = transform.localScale;
            _currentSceneName = SceneManager.GetActiveScene().name;

            if (runtimeData && runtimeData.ArrivedViaPortal)
            {
                runtimeData.ArrivedViaPortal = false;
                _canBeTriggered = false;

                ActivateDestinationPortal(_defaultPosition, runtimeData.PortalDirection);
                GameManager.Instance.SetPendingPortal(_defaultPosition);
            }
            else if (runtimeData.IsReturningToOrigin)
            {
                runtimeData.IsReturningToOrigin = false;
                runtimeData.OriginSceneName = null;

                ActivateReturnPortal(runtimeData.OriginPortalPosition, runtimeData.PortalDirection);
                GameManager.Instance.SetPendingPortal(runtimeData.OriginPortalPosition);
            }
            else
            {
                HidePortal();
            }
        }
        
        private void UsePortal()
        {
            bool isInDestinationScene = !string.IsNullOrEmpty(runtimeData.OriginSceneName) &&
                                        _currentSceneName != runtimeData.OriginSceneName;

            _canBeTriggered = false;

            if (isInDestinationScene)
            {
                _pendingIsReturningToOrigin = true;
                string returnScene = runtimeData.OriginSceneName;
                GameManager.Instance.SetPendingPortal(runtimeData.OriginPortalPosition);
                StartCoroutine(AnimateScaleAndLoadScene(_originalScale, Vector3.zero, returnScene));
            }
            else
            {
                _pendingArrivedViaPortal = true;
                StartCoroutine(AnimateScaleAndLoadScene(_originalScale, Vector3.zero, destinationScene));
            }
        }
        
        private void ShowPortal()
        {
            _collider.enabled = true;
            _spriteRenderer.enabled = true;
        }
        
        private void HidePortal()
        {
            _collider.enabled = false;
            _spriteRenderer.enabled = false;
            transform.localScale = Vector3.zero;
        }
        
        public void ActivatePortal(Vector2 position, Direction playerFacingDirection = Direction.Right)
        {
            transform.position = position;
            _direction = playerFacingDirection;

            float directionMultiplier = playerFacingDirection == Direction.Left ? -1 : 1;

            _originalScale = new Vector3(Mathf.Abs(_originalScale.x) * directionMultiplier, _originalScale.y,
                _originalScale.z);
            transform.localScale = Vector3.zero;

            ShowPortal();
            StartCoroutine(AnimateScale(Vector3.zero, _originalScale, () => _canBeTriggered = true));
        }
        
        private void ActivateDestinationPortal(Vector2 position, Direction playerFacingDirection)
        {
            transform.position = position;
            _direction = playerFacingDirection;

            float directionMultiplier = playerFacingDirection == Direction.Left ? -1 : 1;
            _originalScale = new Vector3(Mathf.Abs(_originalScale.x) * directionMultiplier, _originalScale.y,
                _originalScale.z);
            transform.localScale = Vector3.zero;

            ShowPortal();
            StartCoroutine(AnimateDestinationPortal(Vector3.zero, _originalScale));
        }
        
        private void ActivateReturnPortal(Vector2 position, Direction playerFacingDirection)
        {
            transform.position = position;
            _direction = playerFacingDirection;

            float directionMultiplier = playerFacingDirection == Direction.Left ? -1 : 1;
            _originalScale = new Vector3(Mathf.Abs(_originalScale.x) * directionMultiplier, _originalScale.y,
                _originalScale.z);
            transform.localScale = Vector3.zero;
            _spriteRenderer.enabled = true;
            _collider.enabled = false;

            StartCoroutine(AnimateReturnPortal(Vector3.zero, _originalScale));
        }

        private IEnumerator AnimateDestinationPortal(Vector3 from, Vector3 to)
        {
            yield return null;

            GameManager.Instance.Player?.SetVisible(false);

            float elapsed = 0f;

            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                float t = scaleCurve.Evaluate(elapsed / animationDuration);
                transform.localScale = Vector3.LerpUnclamped(from, to, t);
                yield return null;
            }

            transform.localScale = to;
            GameManager.Instance.Player?.SetVisible(true);
            _canBeTriggered = true;
        }

        private IEnumerator AnimateReturnPortal(Vector3 from, Vector3 to)
        {
            yield return null;

            GameManager.Instance.Player?.SetVisible(false);

            float elapsed = 0f;

            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                float t = scaleCurve.Evaluate(elapsed / animationDuration);
                transform.localScale = Vector3.LerpUnclamped(from, to, t);
                yield return null;
            }

            transform.localScale = to;

            GameManager.Instance.Player?.SetVisible(true);

            yield return new WaitForSeconds(0.2f);

            elapsed = 0f;

            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                float t = scaleCurve.Evaluate(elapsed / animationDuration);
                transform.localScale = Vector3.LerpUnclamped(to, from, t);
                yield return null;
            }

            transform.localScale = from;
            HidePortal();
        }
        
        private IEnumerator AnimateScale(Vector3 from, Vector3 to, System.Action onComplete = null)
        {
            float elapsed = 0f;

            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                float t = scaleCurve.Evaluate(elapsed / animationDuration);
                transform.localScale = Vector3.LerpUnclamped(from, to, t);
                yield return null;
            }

            transform.localScale = to;
            onComplete?.Invoke();
        }

        private IEnumerator AnimateScaleAndLoadScene(Vector3 from, Vector3 to, string sceneName)
        {
            GameManager.Instance.Player?.SetVisible(false);

            float elapsed = 0f;

            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                float t = scaleCurve.Evaluate(elapsed / animationDuration);
                transform.localScale = Vector3.LerpUnclamped(from, to, t);
                yield return null;
            }

            transform.localScale = to;
            GameManager.Instance.SaveRuntimeData();
            GameManager.Instance.KeepBackgroundMusic = keepBackgroundMusic;
            GameManager.Instance.LoadScene(sceneName);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player"))
                return;

            if (other.gameObject.layer == LayerMask.NameToLayer("PlayerAttack"))
                return;

            if (!_canBeTriggered)
                return;

            UsePortal();
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag("Player"))
                return;

            if (other.gameObject.layer == LayerMask.NameToLayer("PlayerAttack"))
                return;

            _canBeTriggered = true;
        }

        public void SaveToRuntimeData()
        {
            if (_pendingArrivedViaPortal)
            {
                runtimeData.OriginSceneName = _currentSceneName;
                runtimeData.OriginPortalPosition = transform.position;
                runtimeData.PortalDirection = _direction;
                runtimeData.ArrivedViaPortal = true;
                runtimeData.IsReturningToOrigin = false;
            }
            else if (_pendingIsReturningToOrigin)
            {
                runtimeData.IsReturningToOrigin = true;
                runtimeData.ArrivedViaPortal = false;
            }

            _pendingArrivedViaPortal = false;
            _pendingIsReturningToOrigin = false;
        }
    }
}