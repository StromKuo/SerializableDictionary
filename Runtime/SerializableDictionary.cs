using System;
using System.Collections.Generic;
using UnityEngine;

namespace SKUnityToolkit.SerializableDictionary
{
    [Serializable]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField]
        SerializableKeyValuePair<TKey, TValue>[] m_pairs;

        public SerializableDictionary() : base() { }
        public SerializableDictionary(IDictionary<TKey, TValue> dictionary) : base(dictionary) { }
        public SerializableDictionary(IEqualityComparer<TKey> comparer) : base(comparer) { }
        public SerializableDictionary(int capacity) : base(capacity) { }
        public SerializableDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer) : base(dictionary, comparer) { }
        public SerializableDictionary(int capacity, IEqualityComparer<TKey> comparer) : base(capacity, comparer) { }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            if (this.m_pairs != null)
            {
                this.Clear();
                foreach (var pair in this.m_pairs)
                {
                    if (!this.ContainsKey(pair.Key))
                    {
                        this[pair.Key] = pair.Value;
                    }
                }
                this.m_pairs = null;
            }
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            int n = this.Count;

            this.m_pairs = new SerializableKeyValuePair<TKey, TValue>[n];

            int i = 0;
            foreach (var pair in this)
            {
                this.m_pairs[i++] = new SerializableKeyValuePair<TKey, TValue>(pair.Key, pair.Value);
            }
        }
    }
}