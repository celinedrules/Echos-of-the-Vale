// Done
using Unity.VisualScripting;
using UnityEngine;

namespace UI.Common
{
    public class Tooltip : MonoBehaviour
    {
        [SerializeField] private Vector2 offset = new Vector2(300, 20);
        
        private RectTransform _rect;
        private CanvasGroup _canvasGroup;
        
        public CanvasGroup CanvasGroup => _canvasGroup;

        protected virtual void Awake()
        {
            _rect = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        public virtual void ShowTooltip(bool show, RectTransform targetRect)
        {
            if (!show)
            {
                _canvasGroup.alpha = 0;
                return;
            }

            _canvasGroup.alpha = 1;
            UpdatePosition(targetRect);
        }
        
        private void UpdatePosition(RectTransform targetRect)
        {
            float screenCenterX = Screen.width / 2f;
            float screenTop = Screen.height;
            float screenBottom = 0;
            
            Vector2 targetPosition = targetRect.position;
            targetPosition.x = targetPosition.x > screenCenterX ? targetPosition.x - offset.x : targetPosition.x + offset.x;
            float verticalHalf = _rect.sizeDelta.y / 2f;
            float topY = targetPosition.y + verticalHalf;
            float bottomY = targetPosition.y - verticalHalf;

            if (topY > screenTop)
                targetPosition.y = screenTop - verticalHalf - offset.y;
            else if (bottomY < screenBottom)
                targetPosition.y = screenBottom + verticalHalf + offset.y;
            
            _rect.position = targetPosition;
        }
        
        protected string GetColoredString(string text, Color color)
        {
            return $"<color=#{color.ToHexString()}>{text}</color>";
        }
    }
}