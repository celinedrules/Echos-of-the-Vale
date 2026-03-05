using System.Collections;
using Core;
using UnityEngine;

namespace Player
{
    public class PlayerFx : EntityFx
    {
        [Header("Image Echo Graphics")]
        [Range(0.01f, 0.2f)]
        [SerializeField] private float imageEchoInterval = 0.05f;
        [SerializeField] private GameObject imageEchoPrefab;

        private PlayerController _player;
        private Coroutine _imageEchoRoutine;

        private void Awake()
        {
            _player = GetComponentInParent<PlayerController>();
        }

        public void PlayImageEchoEffect(float duration)
        {
            if (_imageEchoRoutine != null)
                StopCoroutine(_imageEchoRoutine);

            _imageEchoRoutine = StartCoroutine(ImageEchoEffectRoutine(duration));
        }

        private IEnumerator ImageEchoEffectRoutine(float duration)
        {
            float timeTracker = 0;

            while (timeTracker < duration)
            {
                CreateImageEcho();

                yield return new WaitForSeconds(imageEchoInterval);
                timeTracker += imageEchoInterval;
            }
        }

        private void CreateImageEcho()
        {
            GameObject imageEcho = Instantiate(imageEchoPrefab, transform.position, Quaternion.identity);
            imageEcho.GetComponentInChildren<SpriteRenderer>().sprite = SpriteRenderer.sprite;

            imageEcho.transform.localScale = new Vector3(
                (float)_player.FacingDirection,
                imageEcho.transform.localScale.y,
                imageEcho.transform.localScale.z
            );
        }

        public void CreateEffectOf(GameObject effect, Transform target)
        {
            Instantiate(effect, target.position, Quaternion.identity);
        }
    }
}