using System.Collections.Generic;
using UnityEngine;

[SerializeField]
public class SerializableKeyValuePair<TKey, TValue>
{
    [SerializeField]
    private TKey m_key;

    [SerializeField]
    private TValue m_value;

    //public SerializableKeyValuePair(TKey key, TValue value)
    //{
    //    this._key = key;
    //    this._value = value;
    //}

    public TKey Key {
        get { return m_key; }
    }

    public TValue Value {
        get { return m_value; }
    }

    public override string ToString()
    {
        return ((KeyValuePair<TKey, TValue>)this).ToString();
    }

    public static implicit operator KeyValuePair<TKey, TValue>(SerializableKeyValuePair<TKey, TValue> reference)
    {
        return new KeyValuePair<TKey, TValue>(reference.m_key, reference.m_value);
    }
}
