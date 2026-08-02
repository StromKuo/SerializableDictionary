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

        [NonSerialized]
        int m_deserializationConflictCount;

        /// <summary>
        /// Gets the number of null or duplicate keys discarded by the most recent deserialization.
        /// The first entry for a duplicate key is retained.
        /// </summary>
        public int DeserializationConflictCount => m_deserializationConflictCount;

        public SerializableDictionary() : base() { }
        public SerializableDictionary(IDictionary<TKey, TValue> dictionary) : base(dictionary) { }

        /// <remarks>
        /// Unity does not serialize the comparer. The comparer only applies to this in-memory instance.
        /// </remarks>
        public SerializableDictionary(IEqualityComparer<TKey> comparer) : base(comparer) { }
        public SerializableDictionary(int capacity) : base(capacity) { }

        /// <remarks>
        /// Unity does not serialize the comparer. The comparer only applies to this in-memory instance.
        /// </remarks>
        public SerializableDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer) : base(dictionary, comparer) { }

        /// <remarks>
        /// Unity does not serialize the comparer. The comparer only applies to this in-memory instance.
        /// </remarks>
        public SerializableDictionary(int capacity, IEqualityComparer<TKey> comparer) : base(capacity, comparer) { }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            this.m_deserializationConflictCount = 0;
            if (this.m_pairs != null)
            {
                this.Clear();
                foreach (var pair in this.m_pairs)
                {
                    if (pair == null || SerializableCollectionUtility.IsNull(pair.Key) || this.ContainsKey(pair.Key))
                    {
                        this.m_deserializationConflictCount++;
                        continue;
                    }

                    this.Add(pair.Key, pair.Value);
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
