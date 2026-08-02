using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace SKUnityToolkit.SerializableDictionary.Tests
{
    public class PlayModeSerializationTests
    {
        [UnityTest]
        public IEnumerator DictionarySurvivesJsonRoundTripInPlayMode()
        {
            Assert.That(Application.isPlaying, Is.True);

            var source = ScriptableObject.CreateInstance<Host>();
            var restored = ScriptableObject.CreateInstance<Host>();
            source.Dictionary.Add("one", 1);

            var json = JsonUtility.ToJson(source);
            JsonUtility.FromJsonOverwrite(json, restored);
            yield return null;

            Assert.That(restored.Dictionary, Has.Count.EqualTo(1));
            Assert.That(restored.Dictionary["one"], Is.EqualTo(1));

            Object.Destroy(source);
            Object.Destroy(restored);
        }

        sealed class Host : ScriptableObject
        {
            public SerializableDictionary<string, int> Dictionary =
                new SerializableDictionary<string, int>();
        }
    }
}
