// Done
using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace UI.Common
{
    public class NotificationPopup : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private float displayDuration = 2f;

        private Coroutine _hideCoroutine;

        public void Show(string message, Action onComplete = null)
        {
            messageText.text = message;
            gameObject.SetActive(true);

            if (_hideCoroutine != null)
                StopCoroutine(_hideCoroutine);

            _hideCoroutine = StartCoroutine(HideAfterDelay(onComplete));
        }

        private IEnumerator HideAfterDelay(Action onComplete)
        {
            yield return new WaitForSecondsRealtime(displayDuration);
            gameObject.SetActive(false);
            onComplete?.Invoke();
        }
    }
}