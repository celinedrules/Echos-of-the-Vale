// Done
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI.Dialogue
{
    public class DialogueChoiceHandler : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
    {
        public event Action<int> OnHover;
        public event Action<int> OnClick;
        
        private int _index;

        public void Setup(int index) => _index = index;
        public void OnPointerEnter(PointerEventData eventData) => OnHover?.Invoke(_index);
        public void OnPointerClick(PointerEventData eventData) => OnClick?.Invoke(_index);
    }
}