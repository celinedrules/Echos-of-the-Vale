using System;
using System.Collections;
using UnityEngine;

namespace UI.Dialogue
{
    public class DialogueTypewriter : MonoBehaviour
    {
        [SerializeField] private DialogueView view;
        [SerializeField] private float textSpeed = 0.1f;

        private Coroutine _typingRoutine;
        private string _fullText = string.Empty;
        private Action _onTypingComplete;

        public bool IsTyping => _typingRoutine != null;

        public void StartTyping(string text, Action onTypingComplete)
        {
            StopTyping();

            _fullText = text ?? string.Empty;
            _onTypingComplete = onTypingComplete;
            _typingRoutine = StartCoroutine(TypeTextCoroutine(_fullText));
        }

        public void CompleteInstantly()
        {
            if (!IsTyping)
                return;

            StopCoroutine(_typingRoutine);
            _typingRoutine = null;

            view.SetDialogueText(_fullText);
            NotifyTypingComplete();
        }

        private void StopTyping()
        {
            if (_typingRoutine == null)
                return;

            StopCoroutine(_typingRoutine);
            _typingRoutine = null;
        }

        private IEnumerator TypeTextCoroutine(string text)
        {
            view.ClearDialogueText();

            string currentText = string.Empty;

            foreach (char letter in text)
            {
                currentText += letter;
                view.SetDialogueText(currentText);
                yield return new WaitForSecondsRealtime(textSpeed);
            }

            _typingRoutine = null;
            NotifyTypingComplete();
        }

        private void NotifyTypingComplete()
        {
            Action callback = _onTypingComplete;
            _onTypingComplete = null;
            callback?.Invoke();
        }
    }
}