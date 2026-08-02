using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace SKUnityToolkit.SerializableDictionary.Tests
{
    public class SerializedPropertyUtilityTests
    {
        TestHost m_host;
        SerializedObject m_serializedObject;

        [SetUp]
        public void SetUp()
        {
            m_host = ScriptableObject.CreateInstance<TestHost>();
            m_host.Dictionary.Add("one", 1);
            m_host.HashSet.Add("one");
            ((ISerializationCallbackReceiver)m_host.Dictionary).OnBeforeSerialize();
            ((ISerializationCallbackReceiver)m_host.HashSet).OnBeforeSerialize();
            m_serializedObject = new SerializedObject(m_host);
        }

        [TearDown]
        public void TearDown()
        {
            m_serializedObject?.Dispose();
            if (m_host != null)
            {
                Object.DestroyImmediate(m_host);
            }
        }

        [Test]
        public void SnapshotRestoresGenericValuesAndArrays()
        {
            var property = m_serializedObject.FindProperty(nameof(TestHost.Nested));
            var snapshot = SerializedPropertySnapshot.Capture(property);

            property.FindPropertyRelative("position").vector2IntValue = new Vector2Int(99, 100);
            property.FindPropertyRelative("values").arraySize = 0;
            snapshot.Restore(property);

            Assert.That(property.FindPropertyRelative("position").vector2IntValue, Is.EqualTo(new Vector2Int(3, 4)));
            var values = property.FindPropertyRelative("values");
            Assert.That(values.arraySize, Is.EqualTo(2));
            Assert.That(values.GetArrayElementAtIndex(0).intValue, Is.EqualTo(5));
            Assert.That(values.GetArrayElementAtIndex(1).intValue, Is.EqualTo(6));
        }

        [Test]
        public void StateCacheSeparatesPropertiesWithEqualData()
        {
            var cache = new SerializedPropertyStateCache<State>();
            var first = m_serializedObject.FindProperty(nameof(TestHost.First));
            var second = m_serializedObject.FindProperty(nameof(TestHost.Second));

            var firstState = cache.Get(first);
            var secondState = cache.Get(second);

            Assert.That(firstState, Is.SameAs(cache.Get(first)));
            Assert.That(secondState, Is.Not.SameAs(firstState));
            Assert.That(cache.Count, Is.EqualTo(2));
        }

        [Test]
        public void StateCacheDoesNotRetainDestroyedTargets()
        {
            var cache = new SerializedPropertyStateCache<State>();
            cache.Get(m_serializedObject.FindProperty(nameof(TestHost.First)));

            m_serializedObject.Dispose();
            Object.DestroyImmediate(m_host);
            m_serializedObject = null;
            m_host = null;
            cache.RemoveDeadEntries();

            Assert.That(cache.Count, Is.Zero);
        }

        [Test]
        public void ConflictUtilityFindsDuplicateDictionaryKeys()
        {
            var pairs = m_serializedObject
                .FindProperty(nameof(TestHost.Dictionary))
                .FindPropertyRelative("m_pairs");
            pairs.InsertArrayElementAtIndex(1);

            var found = SerializedPropertyUtility.TryFindConflict(
                pairs, "m_key", out var conflictIndex, out var otherIndex);

            Assert.That(found, Is.True);
            Assert.That(conflictIndex, Is.EqualTo(1));
            Assert.That(otherIndex, Is.EqualTo(0));
        }

        [Test]
        public void ConflictUtilityFindsDuplicateHashSetValues()
        {
            var keys = m_serializedObject
                .FindProperty(nameof(TestHost.HashSet))
                .FindPropertyRelative("m_keys");
            keys.InsertArrayElementAtIndex(1);

            var found = SerializedPropertyUtility.TryFindConflict(
                keys, null, out var conflictIndex, out var otherIndex);

            Assert.That(found, Is.True);
            Assert.That(conflictIndex, Is.EqualTo(1));
            Assert.That(otherIndex, Is.EqualTo(0));
        }

        [Test]
        public void DrawersCalculateExpandedListHeights()
        {
            var dictionaryProperty = m_serializedObject.FindProperty(nameof(TestHost.Dictionary));
            dictionaryProperty.isExpanded = true;
            var hashSetProperty = m_serializedObject.FindProperty(nameof(TestHost.HashSet));
            hashSetProperty.isExpanded = true;

            var dictionaryHeight = new SerializableDictionaryPropertyDrawer().GetPropertyHeight(
                dictionaryProperty, GUIContent.none);
            var hashSetHeight = new SerializableHashSetPropertyDrawer().GetPropertyHeight(
                hashSetProperty, GUIContent.none);

            Assert.That(dictionaryHeight, Is.GreaterThan(EditorGUIUtility.singleLineHeight));
            Assert.That(hashSetHeight, Is.GreaterThan(EditorGUIUtility.singleLineHeight));
        }

        [Test]
        public void KeyValuePairDrawerIncludesSpacingForExpandedValue()
        {
            var pairProperty = m_serializedObject.FindProperty(nameof(TestHost.Pair));
            var keyProperty = pairProperty.FindPropertyRelative("m_key");
            var valueProperty = pairProperty.FindPropertyRelative("m_value");
            valueProperty.isExpanded = true;

            var expected = EditorGUI.GetPropertyHeight(keyProperty) +
                EditorGUIUtility.standardVerticalSpacing +
                EditorGUI.GetPropertyHeight(valueProperty);
            var actual = new SerializableKeyValuePairPropertyDrawer().GetPropertyHeight(
                pairProperty, GUIContent.none);

            Assert.That(actual, Is.EqualTo(expected));
        }

        sealed class State { }

        sealed class TestHost : ScriptableObject
        {
            public int First = 10;
            public int Second = 10;
            public SerializableDictionary<string, int> Dictionary = new SerializableDictionary<string, int>();
            public SerializableHashSet<string> HashSet = new SerializableHashSet<string>();
            public SerializableKeyValuePair<string, NestedValue> Pair =
                new SerializableKeyValuePair<string, NestedValue>("nested", new NestedValue());
            public NestedValue Nested = new NestedValue
            {
                position = new Vector2Int(3, 4),
                values = new[] { 5, 6 }
            };
        }

        [System.Serializable]
        sealed class NestedValue
        {
            public Vector2Int position;
            public int[] values;
        }
    }
}
