// Done
using Core.Attributes;
using Managers;
using UnityEngine;

namespace Interactables
{
    public class Waypoint : MonoBehaviour
    {
        [SerializeField] private string waypointId;
        [SerializeField] private string waypointName;
        [SerializeField, LocationDropdown] private string transferToScene;
        [SerializeField, WaypointDropdown("transferToScene")] private string targetWaypointId;
        [SerializeField] private bool canBeTriggered = true;

        public string WaypointId => waypointId;
        public string TargetWaypointId => targetWaypointId;

        private void OnValidate()
        {
            // Auto-generate ID if empty
            if (string.IsNullOrEmpty(waypointId))
                waypointId = System.Guid.NewGuid().ToString()[..8];

            gameObject.name = "Waypoint - " + waypointName;
        }

        public void DisableTemporarily()
        {
            canBeTriggered = false;
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player"))
                return;

            if (other.gameObject.layer == LayerMask.NameToLayer("PlayerAttack"))
                return;

            if (!canBeTriggered)
                return;

            GameManager.Instance.LoadScene(transferToScene, targetWaypointId);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag("Player"))
                return;
        
            if (other.gameObject.layer == LayerMask.NameToLayer("PlayerAttack"))
                return;

            canBeTriggered = true;
            GameManager.Instance?.SetLastWaypointExit(transform.position);
        }
    }
}