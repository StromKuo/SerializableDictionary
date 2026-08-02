using System;
using UnityEngine;

namespace SKUnityToolkit.SerializableDictionary.Samples
{
    public class SerializableDictionaryExample : MonoBehaviour
    {
        [SerializeField]
        SerializableDictionary<string, Color> colorsByName = new SerializableDictionary<string, Color>();

        [SerializeField]
        SerializableHashSet<string> uniqueTags = new SerializableHashSet<string>();

        [SerializeField]
        SerializableKeyValuePair<string, int> score = new SerializableKeyValuePair<string, int>("Player", 0);
    }
}
