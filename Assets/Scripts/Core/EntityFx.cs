// Done
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using Utilities.Enums;

namespace Core
{
    public class EntityFx : MonoBehaviour
    {
        private static readonly int FlashAmount = Shader.PropertyToID("_FlashAmount");
        
        [Header("Receive Damage")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private float flashDuration = 0.1f;

        [Header("Give Damage")]
        [SerializeField] private GameObject hitEffect;
        [FormerlySerializedAs("critiallHitEffect")] [SerializeField] private GameObject criticalHitEffect;
        [SerializeField] private Color hitEffectColor = Color.white;
        
        [Header("Element Colors")]
        [SerializeField] private Color fireColor = Color.red;
        [SerializeField] private Color iceColor = Color.cyan;
        [SerializeField] private Color lightningColor = Color.yellow;
        
        
        private Material _material;
        private Coroutine _flashRoutine;

        protected SpriteRenderer SpriteRenderer => spriteRenderer;
        
        private void Awake() => _material = spriteRenderer.material;

        public Color GetElementColor(ElementType elementType)
        {
            return elementType switch
            {
                ElementType.None => Color.white,
                ElementType.Fire => fireColor,
                ElementType.Ice => iceColor,
                ElementType.Lightning => lightningColor,
                _ => throw new ArgumentOutOfRangeException(nameof(elementType), elementType, null)
            };
        }
        
        public void PlayStatusEffect(float duration, ElementType elementType)
        {
            switch (elementType)
            {
                case ElementType.Ice:
                    StartCoroutine(PlayStatusEffectRoutine(duration, iceColor));
                    break;
                case ElementType.Fire:
                    StartCoroutine(PlayStatusEffectRoutine(duration, fireColor));
                    break;
                case ElementType.Lightning:
                    StartCoroutine(PlayStatusEffectRoutine(duration, lightningColor));
                    break;
                case ElementType.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(elementType), elementType, null);
            }
        }

        public void StopAllEffects()
        {
            StopAllCoroutines();
            spriteRenderer.color = Color.white;
            _material.SetFloat(FlashAmount, 0f);
        }

        private IEnumerator PlayStatusEffectRoutine(float duration, Color effectColor)
        {
            const float tickInterval = 0.25f;
            float timeHasPassed = 0;
            
            Color defaultColor = spriteRenderer.color;
            Color lightColor  = effectColor * 1.2f;
            Color darkColor = effectColor * 0.8f;

            //_material.SetFloat(FlashAmount, 1f);
            
            bool toggle = false;
            
            
            while (timeHasPassed < duration)
            {
                spriteRenderer.color = toggle ? lightColor : darkColor;
                
                toggle = !toggle;
                
                yield return new WaitForSeconds(tickInterval);
                timeHasPassed += tickInterval;
            }
            
            spriteRenderer.color = defaultColor;
        }
        
        public void SetElementColor(ElementType elementType)
        {
            hitEffectColor = elementType switch
            {
                ElementType.Fire => fireColor,
                ElementType.Ice => iceColor,
                ElementType.Lightning => lightningColor,
                _ => Color.white
            };
            
        }
        
        public void Flash()
        {
            if (_flashRoutine != null)
                StopCoroutine(_flashRoutine);

            _flashRoutine = StartCoroutine(FlashRoutine());
        }

        private IEnumerator FlashRoutine()
        {
            _material.SetFloat(FlashAmount, 1f);
            yield return new WaitForSeconds(flashDuration);
            _material.SetFloat(FlashAmount, 0f);
        }
        
        public void CreateHitEffect(Transform target, bool critical)
        {
            GameObject hitPrefab = critical ? criticalHitEffect : hitEffect;
            GameObject hitEffectGo = Instantiate(hitPrefab, target.position, Quaternion.identity);

            // Calculate direction based on X position difference
            // If target is to the right (diff > 0), direction is 1. If left, -1.
            float direction = Mathf.Sign(target.position.x - transform.position.x);

            Vector3 newScale = hitEffectGo.transform.localScale;
            // Force the scale to match the direction (using Abs ensures we don't flip a double negative)
            newScale.x = Mathf.Abs(newScale.x) * direction;
            hitEffectGo.transform.localScale = newScale;
        }
    }
}