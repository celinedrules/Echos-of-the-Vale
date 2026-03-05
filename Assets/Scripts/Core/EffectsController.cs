// Done
using System.Collections;
using UnityEngine;

namespace Core
{
    public class EffectsController : MonoBehaviour
    {
        [Header("Auto Destroy")]
        [SerializeField] private bool autoDestroy = true;
        [SerializeField] private float destroyTime = 1.0f;

        [Header("Random Rotation")]
        [SerializeField] private bool randomRotation = true;
        [SerializeField] private float minRotation;
        [SerializeField] private float maxRotation = 360.0f;

        [Header("Random Position")]
        [SerializeField] private bool randomPosition = true;
        [SerializeField] private float xMinOffset = -0.3f;
        [SerializeField] private float xMaxOffset = 0.3f;
        [Space]
        [SerializeField] private float yMinOffset = -0.3f;
        [SerializeField] private float yMaxOffset = 0.3f;

        [Header("Fade Effect")]
        [SerializeField] private bool canFade;
        [SerializeField] private float fadeSpeed = 1.0f;

        private SpriteRenderer _spriteRenderer;

        private void Awake()
        {
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        private void Start()
        {
            if (canFade)
                StartCoroutine(FadeEffect());

            ApplyRandomPosition();
            ApplyRandomRotation();

            if (autoDestroy)
                Destroy(gameObject, destroyTime);
        }

        private IEnumerator FadeEffect()
        {
            Color targetColor = Color.white;

            while (targetColor.a > 0.0f)
            {
                targetColor.a -= fadeSpeed * Time.deltaTime;
                _spriteRenderer.color = targetColor;
                yield return null;
            }

            _spriteRenderer.color = targetColor;
        }

        private void ApplyRandomPosition()
        {
            if (!randomPosition)
                return;

            float xOffset = Random.Range(xMinOffset, xMaxOffset);
            float yOffset = Random.Range(yMinOffset, yMaxOffset);

            transform.position = new Vector3(transform.position.x + xOffset, transform.position.y + yOffset,
                transform.position.z);
        }

        private void ApplyRandomRotation()
        {
            if (!randomRotation)
                return;

            transform.rotation = Quaternion.Euler(0, 0, Random.Range(minRotation, maxRotation));
        }
    }
}