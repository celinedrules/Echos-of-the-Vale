// Done
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Stats
{
    [Serializable, InlineProperty, HideLabel]
    public class Stat
    {
        [SerializeField, HideLabel] private int baseValue;
        [SerializeField] private List<StatModifier> modifiers = new();

        private int _finalValue;

        public int Value
        {
            get => GetFinalValue();
            set => baseValue = value;
        }

        public Stat(int defaultValue) => baseValue = defaultValue;

        public void AddModifier(string source, int value)
        {
            StatModifier modifier = new StatModifier(source, value);
            modifiers.Add(modifier);
        }

        public void RemoveModifier(string source)
        {
            modifiers.RemoveAll(x => x.source == source);
        }

        public void ClearModifiers()
        {
            modifiers.Clear();
        }

        private int GetFinalValue()
        {
            _finalValue = baseValue;

            foreach (StatModifier modifier in modifiers)
                _finalValue += modifier.value;

            return _finalValue;
        }
    }

    [Serializable]
    public class StatModifier
    {
        public string source;
        public int value;

        public StatModifier(string source, int value)
        {
            this.value = value;
            this.source = source;
        }
    }
}