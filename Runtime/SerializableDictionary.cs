using System;
using System.Collections.Generic;
using UnityEngine;

namespace SKUnityToolkit.SerializableDictionary
{
    [Serializable]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [Obsolete]
        [SerializeField]
        TKey[] m_keys;

        [Obsolete]
        [SerializeField]
        TValue[] m_values;

        [SerializeField]
        SerializableKeyValuePair<TKey, TValue>[] m_pairs;

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            if (m_keys != null && m_values != null && m_keys.Length == m_values.Length && m_keys.Length != 0)
            {
                this.Clear();
                for (int i = 0; i < m_keys.Length; ++i)
                {
                    this[m_keys[i]] = m_values[i];
                }

                m_keys = null;
                m_values = null;
            }
            else
            {
                if (this.m_pairs != null)
                {
                    this.Clear();
                    foreach (var pair in m_pairs)
                    {
                        if (!this.ContainsKey(pair.Key))
                        {
                            this[pair.Key] = pair.Value;
                        }
                    }
                    this.m_pairs = null;
                }
            }
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            int n = this.Count;

            m_pairs = new SerializableKeyValuePair<TKey, TValue>[n];

            int i = 0;
            foreach (var pair in this)
            {

                m_pairs[i++] = new SerializableKeyValuePair<TKey, TValue>(pair.Key, pair.Value);
            }
        }
    }
}