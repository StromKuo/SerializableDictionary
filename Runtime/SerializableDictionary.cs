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