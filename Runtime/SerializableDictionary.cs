using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class SerializableDictionaryBase<TKey, TValue, TValueStorage> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
{
	[SerializeField]
	TKey[] m_keys;

	[SerializeField]
	TValueStorage[] m_values;

	protected abstract void SetValue(TValueStorage[] storage, int i, TValue value);
	protected abstract TValue GetValue(TValueStorage[] storage, int i);

	void ISerializationCallbackReceiver.OnAfterDeserialize()
	{
		if (m_keys != null && m_values != null && m_keys.Length == m_values.Length)
		{
			this.Clear();
			for (int i = 0; i < m_keys.Length; ++i)
			{
				this[m_keys[i]] = GetValue(m_values, i);
			}

			m_keys = null;
			m_values = null;
		}
	}

	void ISerializationCallbackReceiver.OnBeforeSerialize()
	{
		int n = this.Count;
		m_keys = new TKey[n];
		m_values = new TValueStorage[n];

		int i = 0;
		foreach (var pair in this)
		{
			m_keys[i] = pair.Key;
			SetValue(m_values, i, pair.Value);
			++i;
		}
	}
}

[Serializable]
public class SerializableDictionaryStorage<T>
{
	public T data;
}

[Serializable]
public class SerializableDictionary<TKey, TValue> : SerializableDictionaryBase<TKey, TValue, TValue>
{
	protected override TValue GetValue(TValue[] storage, int i)
	{
		return storage[i];
	}

	protected override void SetValue(TValue[] storage, int i, TValue value)
	{
		storage[i] = value;
	}
}

[Serializable]
public class SerializableDictionary<TKey, TValue, TValueStorage> : SerializableDictionaryBase<TKey, TValue, TValueStorage> where TValueStorage : SerializableDictionaryStorage<TValue>, new()
{
	protected override TValue GetValue(TValueStorage[] storage, int i)
	{
		return storage[i].data;
	}

	protected override void SetValue(TValueStorage[] storage, int i, TValue value)
	{
		storage[i] = new TValueStorage();
		storage[i].data = value;
	}
}