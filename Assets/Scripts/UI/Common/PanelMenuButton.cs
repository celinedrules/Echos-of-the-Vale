// Done
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Common
{
    public class PanelMenuButton : MonoBehaviour
    {
        private static PanelMenuButton _activeButton;
        
        [SerializeField] private Color activeColor = Color.white;
        [SerializeField] private Color inactiveColor = new Color(0.5f, 0.5f, 0.5f, 0.6f);
        
        private Image _image;
        private TextMeshProUGUI _text;

        private void Awake()
        {
            _image = GetComponent<Image>();
            _text = GetComponentInChildren<TextMeshProUGUI>();
            SetColor(inactiveColor);
        }
        
        public void SetAsActive()
        {
            if (_activeButton != null)
                _activeButton.SetColor(inactiveColor);
            
            _activeButton = this;
            SetColor(activeColor);
        }

        private void SetColor(Color color)
        {
            if (_image != null) _image.color = color;
            if (_text != null) _text.color = color;
        }

        public static void ClearActive()
        {
            if (_activeButton != null)
            {
                _activeButton.SetColor(_activeButton.inactiveColor);
                _activeButton = null;
            }
        }
    }
}