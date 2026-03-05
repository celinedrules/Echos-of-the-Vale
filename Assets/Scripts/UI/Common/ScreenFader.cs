// Done
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

namespace UI.Common
{
    public class ScreenFader : Singleton<ScreenFader>
    {
        [SerializeField] private Image fadeImage;
        [SerializeField] private float fadeDuration = 0.5f;
        
        // Static flag persists across scene loads
        private static bool _shouldStartBlack;
        
        public float FadeDuration => fadeDuration;

        protected override void Awake()
        {
            base.Awake();
            SetAlpha(_shouldStartBlack ? 1f : 0f);
        }

        public void FadeOut(Action onComplete = null)
        {
            StartCoroutine(FadeRoutine(0f, 1f, onComplete));
        }

        public void FadeIn(Action onComplete = null)
        {
            StartCoroutine(FadeRoutine(1f, 0f, onComplete));
        }

        public Coroutine FadeOutCoroutine()
        {
            _shouldStartBlack = true; // Next scene should start black
            return StartCoroutine(FadeRoutine(0f, 1f, null));
        }

        public Coroutine FadeInCoroutine()
        {
            _shouldStartBlack = false; // Reset flag after fading in
            return StartCoroutine(FadeRoutine(1f, 0f, null));
        }

        private IEnumerator FadeRoutine(float from, float to, Action onComplete)
        {
            float elapsed = 0f;
            SetAlpha(from);

            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float alpha = Mathf.Lerp(from, to, elapsed / fadeDuration);
                SetAlpha(alpha);
                yield return null;
            }

            SetAlpha(to);
            onComplete?.Invoke();
        }

        private void SetAlpha(float alpha)
        {
            if (!fadeImage) 
                return;
            
            Color color = fadeImage.color;
            color.a = alpha;
            fadeImage.color = color;
        }
    }
}