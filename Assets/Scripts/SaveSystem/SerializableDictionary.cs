// Done
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SaveSystem
{
    [Serializable]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField, HideInInspector]
        private List<TKey> keys = new();
        [SerializeField, HideInInspector]
        private List<TValue> values = new();
        
        public void OnBeforeSerialize()
        {
            keys.Clear();
            values.Clear();

            foreach (KeyValuePair<TKey, TValue> kvp in this)
            {
                keys.Add(kvp.Key);
                values.Add(kvp.Value);
            }
        }

        public void OnAfterDeserialize()
        {
            Clear();
            
            for (int i = 0; i < keys.Count && i < values.Count; i++)
                this[keys[i]] = values[i];
        }
    }
}