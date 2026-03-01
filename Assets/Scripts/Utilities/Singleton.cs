using UnityEngine;

namespace Utilities
{
    public class Singleton<T> : MonoBehaviour where T : Component
    {
        [SerializeField] private bool isPersistent; // Do we want the component to survive a level load?

        private static T _instance; // The static instance of the component
        private static bool _isQuitting; // Track if application is quitting

        public static bool Exists => _instance != null;

        public bool IsPersistent
        {
            set
            {
                Debug.Log("Persistent");
                isPersistent = value;
                SetPersistence();
            }
        }

        public static T Instance
        {
            get
            {
                if (_isQuitting)
                    return null;

                if (_instance)
                    return _instance;

                _instance = FindFirstObjectByType<T>();

                if (_instance)
                    return _instance;

                // Don't create new instances during shutdown
                if (!Application.isPlaying)
                    return null;

                GameObject newGo = new() { name = typeof(T).ToString() };
                _instance = newGo.AddComponent<T>();

                return _instance;
            }
            protected set => _instance = value;
        }

        /// <summary>
        /// Called when the script is being loaded
        /// </summary>
        protected virtual void Awake()
        {
            if (!_instance)
            {
                _instance = this as T;
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
                return;
            }

            if (isPersistent)
                SetPersistence();
        }

        protected virtual void OnApplicationQuit()
        {
            _isQuitting = true;
        }

        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        /// <summary>
        /// Set the GameObject to not be destroyed
        /// </summary>
        private void SetPersistence()
        {
            // Unparent first so we only make THIS object persistent, not the entire hierarchy
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }
    }
}