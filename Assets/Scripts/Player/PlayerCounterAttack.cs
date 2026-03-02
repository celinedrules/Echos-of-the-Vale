using System;
using Core;
using Core.Interfaces;
using UnityEngine;

namespace Player
{
    public class PlayerCounterAttack : MonoBehaviour
    {
        public event Action OnCounterSuccess;

        private bool _isCounterWindowOpen;
        
        [field: SerializeField, Header("Counter Attack Details")]
        public float CounterDuration { get; set; }

        public void SetCounterWindow(bool isOpen) => _isCounterWindowOpen = isOpen;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if(!_isCounterWindowOpen)
                return;
            
            WeaponHitBox hitBox = other.GetComponent<WeaponHitBox>();

            if (hitBox != null && hitBox.Owner is ICounterable counterable)
            {
                if (counterable.CanBeCountered)
                {
                    counterable.HandleCounter();
                    
                    OnCounterSuccess?.Invoke();
                    
                    SetCounterWindow(false);
                }
            }
        }
    }
}