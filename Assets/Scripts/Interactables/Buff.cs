// Done
using Data.ItemEffects;
using Stats;
using UnityEngine;

namespace Interactables
{
    public class Buff : MonoBehaviour
    {
        [Header("Details")]
        [SerializeField] private BuffEffectData[] buffEffects;
        [SerializeField] private string buffName;
        [SerializeField] private float duration = 4.0f;

        [Header("Movement")]
        [SerializeField] private float floatSpeed = 1.0f;
        [SerializeField] private float floatRange = 0.1f;

        private PlayerStats _statsToModify;
        private Vector3 _startPosition;

        private void Awake()
        {
            _startPosition = transform.position;
        }

        private void Update()
        {
            float yOffset = Mathf.Sin(Time.time * floatSpeed) * floatRange;
            transform.position = _startPosition + new Vector3(0, yOffset, 0);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player"))
                return;

            _statsToModify = other.GetComponent<PlayerStats>();

            if (_statsToModify != null && _statsToModify.CanApplyBuff(buffName))
            {
                _statsToModify.ApplyBuff(buffEffects, duration, buffName);
                Destroy(gameObject);
            }
        }
    }
}