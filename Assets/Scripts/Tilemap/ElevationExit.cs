using UnityEngine;

namespace Tilemap
{
    public class ElevationExit : MonoBehaviour
    {
        [SerializeField] private Collider2D[] collisionHigh;
        [SerializeField] private Collider2D[] collisionLow;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if(!other.CompareTag("Player"))
                return;

            foreach (Collider2D col in collisionHigh)
            {
                col.enabled = false;
            }

            foreach (Collider2D col in collisionLow)
            {
                col.enabled = true;
            }

            other.GetComponentInChildren<SpriteRenderer>().sortingOrder = 8;
        }
    }
}