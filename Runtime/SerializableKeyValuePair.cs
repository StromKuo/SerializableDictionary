using System;
using System.Collections.Generic;
using UnityEngine;

namespace SKUnityToolkit.SerializableDictionary
{
    [Serializable]
    public class SerializableKeyValuePair<TKey, TValue>
    {
        [SerializeField]
        private TKey m_key;

        [SerializeField]
        private TValue m_value;

        public SerializableKeyValuePair(TKey key, TValue value)
        {
            this.m_key = key;
            this.m_value = value;
        }

        public TKey Key => m_key;

        public TValue Value => m_value;

        public override string ToString()
        {
            return ((KeyValuePair<TKey, TValue>)this).ToString();
        }

        public static implicit operator KeyValuePair<TKey, TValue>(SerializableKeyValuePair<TKey, TValue> reference)
        {
            return new KeyValuePair<TKey, TValue>(reference.m_key, reference.m_value);
        }
    }
}