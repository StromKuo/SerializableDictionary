using System;
using System.Collections.Generic;
using UnityEngine;

namespace SKUnityToolkit.SerializableDictionary
{
    [Serializable]
    public class SerializableHashSet<T> : HashSet<T>, ISerializationCallbackReceiver
    {
        [SerializeField]
        T[] m_keys;

        [NonSerialized]
        int m_deserializationConflictCount;

        /// <summary>
        /// Gets the number of null or duplicate values discarded by the most recent deserialization.
        /// </summary>
        public int DeserializationConflictCount => m_deserializationConflictCount;

        public SerializableHashSet() { }
        public SerializableHashSet(IEnumerable<T> collection) : base(collection) { }

        /// <remarks>
        /// Unity does not serialize the comparer. The comparer only applies to this in-memory instance.
        /// </remarks>
        public SerializableHashSet(IEqualityComparer<T> comparer) : base(comparer) { }

        /// <remarks>
        /// Unity does not serialize the comparer. The comparer only applies to this in-memory instance.
        /// </remarks>
        public SerializableHashSet(IEnumerable<T> collection, IEqualityComparer<T> comparer) : base(collection, comparer) { }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            m_deserializationConflictCount = 0;
            if (m_keys != null)
            {
                Clear();
                for (int i = 0; i < m_keys.Length; ++i)
                {
                    if (SerializableCollectionUtility.IsNull(m_keys[i]) || !Add(m_keys[i]))
                    {
                        m_deserializationConflictCount++;
                    }
                }

                m_keys = null;
            }
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            int n = Count;
            m_keys = new T[n];

            int i = 0;
            foreach (var value in this)
            {
                m_keys[i] = value;
                ++i;
            }
        }
    }
}
