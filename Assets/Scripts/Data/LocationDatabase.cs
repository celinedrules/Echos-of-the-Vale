// Done
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "LocationDatabase", menuName = "Echos of the Vale/Location Database")]
    public class LocationDatabase : ScriptableObject
    {
        [SerializeField] private List<LocationEntry> locations = new();

        private Dictionary<string, string> _lookupCache;

        public string GetDisplayName(string sceneName)
        {
            if (_lookupCache == null)
            {
                _lookupCache = new Dictionary<string, string>();
                foreach (var entry in locations)
                {
                    if (!string.IsNullOrEmpty(entry.sceneName))
                        _lookupCache[entry.sceneName] = entry.displayName;
                }
            }

            return _lookupCache.TryGetValue(sceneName, out string displayName) 
                ? displayName 
                : sceneName;
        }

        public List<WaypointEntry> GetWaypointsForScene(string sceneName)
        {
            var location = locations.FirstOrDefault(l => l.sceneName == sceneName);
            return location?.waypoints ?? new List<WaypointEntry>();
        }

        public WaypointEntry GetWaypoint(string sceneName, string waypointId)
        {
            return GetWaypointsForScene(sceneName)
                .FirstOrDefault(w => w.waypointId == waypointId);
        }
        
        public void InvalidateCache() => _lookupCache = null;

        [Serializable]
        public class LocationEntry
        {
            [HideInInspector] public string sceneName;
            public string displayName;
            public List<WaypointEntry> waypoints = new();
        }

        [Serializable]
        public class WaypointEntry
        {
            public string waypointId;
            public string displayName;
        }
    }
}