using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Tilemap
{
    public class ElevationEntry : MonoBehaviour
    {
        [SerializeField] private Collider2D[] collisionHigh;
        [SerializeField] private Collider2D[] collisionLow;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if(!other.CompareTag("Player"))
                return;

            foreach (Collider2D col in collisionHigh)
            {
                col.enabled = true;
            }

            foreach (Collider2D col in collisionLow)
            {
                col.enabled = false;
            }

            other.GetComponentInChildren<SpriteRenderer>().sortingOrder = 15;
        }
    }
}