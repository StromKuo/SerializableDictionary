#if NET_4_6 || NET_STANDARD_2_0
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

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            if (m_keys != null)
            {
                Clear();
                for (int i = 0; i < m_keys.Length; ++i)
                {
                    Add(m_keys[i]);
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
#endif