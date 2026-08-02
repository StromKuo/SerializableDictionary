using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace SKUnityToolkit.SerializableDictionary.Tests
{
    public class SerializationTests
    {
        [Test]
        public void DictionaryRoundTripPreservesEntriesAndOrder()
        {
            var source = new SerializableDictionary<string, int>
            {
                { "first", 1 },
                { "second", 2 }
            };

            InvokeBeforeSerialize(source);
            var serializedPairs = GetPrivateField<SerializableKeyValuePair<string, int>[]>(source, "m_pairs");
            Assert.That(serializedPairs, Has.Length.EqualTo(2));
            Assert.That(serializedPairs[0].Key, Is.EqualTo("first"));
            Assert.That(serializedPairs[1].Key, Is.EqualTo("second"));

            var restored = new SerializableDictionary<string, int>();
            SetPrivateField(restored, "m_pairs", serializedPairs);
            InvokeAfterDeserialize(restored);

            Assert.That(restored, Is.EquivalentTo(source));
            Assert.That(restored.DeserializationConflictCount, Is.Zero);
        }

        [Test]
        public void DictionaryDeserializationKeepsFirstDuplicateAndReportsConflicts()
        {
            var dictionary = new SerializableDictionary<string, int>();
            SetPrivateField(dictionary, "m_pairs", new[]
            {
                new SerializableKeyValuePair<string, int>("same", 1),
                new SerializableKeyValuePair<string, int>("same", 2),
                new SerializableKeyValuePair<string, int>(null, 3),
                null
            });

            InvokeAfterDeserialize(dictionary);

            Assert.That(dictionary, Has.Count.EqualTo(1));
            Assert.That(dictionary["same"], Is.EqualTo(1));
            Assert.That(dictionary.DeserializationConflictCount, Is.EqualTo(3));
        }

        [Test]
        public void DictionaryDeserializationRejectsDestroyedUnityObjectKeys()
        {
            var key = ScriptableObject.CreateInstance<TestAsset>();
            var pair = new SerializableKeyValuePair<UnityEngine.Object, int>(key, 1);
            UnityEngine.Object.DestroyImmediate(key);

            var dictionary = new SerializableDictionary<UnityEngine.Object, int>();
            SetPrivateField(dictionary, "m_pairs", new[] { pair });
            InvokeAfterDeserialize(dictionary);

            Assert.That(dictionary, Is.Empty);
            Assert.That(dictionary.DeserializationConflictCount, Is.EqualTo(1));
        }

        [Test]
        public void HashSetRoundTripPreservesValues()
        {
            var source = new SerializableHashSet<string> { "one", "two" };
            InvokeBeforeSerialize(source);
            var serializedKeys = GetPrivateField<string[]>(source, "m_keys");

            var restored = new SerializableHashSet<string>();
            SetPrivateField(restored, "m_keys", serializedKeys);
            InvokeAfterDeserialize(restored);

            Assert.That(restored.SetEquals(source), Is.True);
            Assert.That(restored.DeserializationConflictCount, Is.Zero);
        }

        [Test]
        public void HashSetDeserializationDiscardsNullAndDuplicateValues()
        {
            var hashSet = new SerializableHashSet<string>();
            SetPrivateField(hashSet, "m_keys", new[] { "one", "one", null });

            InvokeAfterDeserialize(hashSet);

            Assert.That(hashSet, Is.EquivalentTo(new[] { "one" }));
            Assert.That(hashSet.DeserializationConflictCount, Is.EqualTo(2));
        }

        [Test]
        public void ComparerConstructorAppliesToCurrentDictionaryInstance()
        {
            var dictionary = new SerializableDictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                ["KEY"] = 1
            };

            Assert.That(dictionary.ContainsKey("key"), Is.True);
        }

        static void InvokeBeforeSerialize(object value)
        {
            ((ISerializationCallbackReceiver)value).OnBeforeSerialize();
        }

        static void InvokeAfterDeserialize(object value)
        {
            ((ISerializationCallbackReceiver)value).OnAfterDeserialize();
        }

        static T GetPrivateField<T>(object target, string name)
        {
            return (T)target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(target);
        }

        static void SetPrivateField(object target, string name, object value)
        {
            target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
        }

        sealed class TestAsset : ScriptableObject { }
    }
}
