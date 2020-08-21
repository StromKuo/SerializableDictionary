using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SerializableDictionaryNew : ISerializationCallbackReceiver
{
    void ISerializationCallbackReceiver.OnAfterDeserialize()
    {
        throw new NotImplementedException();
    }

    void ISerializationCallbackReceiver.OnBeforeSerialize()
    {
        throw new NotImplementedException();
    }
}
