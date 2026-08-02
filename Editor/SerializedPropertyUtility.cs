using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace SKUnityToolkit.SerializableDictionary
{
    internal readonly struct SerializedPropertyIdentity : IEquatable<SerializedPropertyIdentity>
    {
        public SerializedPropertyIdentity(SerializedProperty property)
        {
            TargetInstanceId = RuntimeHelpers.GetHashCode(property.serializedObject.targetObject);
            PropertyPath = property.propertyPath;
        }

        public int TargetInstanceId { get; }
        public string PropertyPath { get; }

        public bool Equals(SerializedPropertyIdentity other)
        {
            return TargetInstanceId == other.TargetInstanceId && PropertyPath == other.PropertyPath;
        }

        public override bool Equals(object obj)
        {
            return obj is SerializedPropertyIdentity other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (TargetInstanceId * 397) ^ (PropertyPath != null ? PropertyPath.GetHashCode() : 0);
            }
        }

        public static bool RefersToSameProperty(SerializedProperty left, SerializedProperty right)
        {
            if (left == null || right == null)
            {
                return false;
            }

            try
            {
                return new SerializedPropertyIdentity(left).Equals(new SerializedPropertyIdentity(right));
            }
            catch (NullReferenceException)
            {
                return false;
            }
            catch (ObjectDisposedException)
            {
                return false;
            }
        }
    }

    internal sealed class SerializedPropertyStateCache<TState> where TState : class, new()
    {
        sealed class Entry
        {
            public Entry(UnityEngine.Object target)
            {
                Target = new WeakReference<UnityEngine.Object>(target);
                State = new TState();
            }

            public WeakReference<UnityEngine.Object> Target { get; }
            public TState State { get; }
        }

        readonly Dictionary<SerializedPropertyIdentity, Entry> m_entries =
            new Dictionary<SerializedPropertyIdentity, Entry>();

        int m_accessCount;

        internal int Count => m_entries.Count;

        public TState Get(SerializedProperty property)
        {
            if ((++m_accessCount & 31) == 0)
            {
                RemoveDeadEntries();
            }

            var identity = new SerializedPropertyIdentity(property);
            var target = property.serializedObject.targetObject;

            if (m_entries.TryGetValue(identity, out var entry) &&
                entry.Target.TryGetTarget(out var cachedTarget) &&
                ReferenceEquals(cachedTarget, target))
            {
                return entry.State;
            }

            entry = new Entry(target);
            m_entries[identity] = entry;
            return entry.State;
        }

        public void RemoveDeadEntries()
        {
            var deadKeys = new List<SerializedPropertyIdentity>();
            foreach (var pair in m_entries)
            {
                if (!pair.Value.Target.TryGetTarget(out var target) || target == null)
                {
                    deadKeys.Add(pair.Key);
                }
            }

            foreach (var key in deadKeys)
            {
                m_entries.Remove(key);
            }
        }
    }

    internal sealed class SerializedPropertySnapshot
    {
        sealed class Node
        {
            public string RelativePath;
            public bool IsArray;
            public bool IsExpanded;
            public bool HasValue;
            public object Value;
            public readonly List<Node> Children = new List<Node>();
        }

        readonly Node m_root;

        SerializedPropertySnapshot(Node root)
        {
            m_root = root;
        }

        public static SerializedPropertySnapshot Capture(SerializedProperty property)
        {
            return new SerializedPropertySnapshot(CaptureNode(property, string.Empty));
        }

        public void Restore(SerializedProperty property)
        {
            RestoreNode(property, m_root);
        }

        static Node CaptureNode(SerializedProperty property, string relativePath)
        {
            var node = new Node
            {
                RelativePath = relativePath,
                IsArray = property.isArray && property.propertyType != SerializedPropertyType.String,
                IsExpanded = property.isExpanded
            };

            if (node.IsArray)
            {
                for (var i = 0; i < property.arraySize; i++)
                {
                    node.Children.Add(CaptureNode(property.GetArrayElementAtIndex(i), i.ToString()));
                }
                return node;
            }

            try
            {
                node.Value = property.boxedValue;
                node.HasValue = true;
                return node;
            }
            catch (InvalidOperationException)
            {
                CaptureChildren(property, node);
                return node;
            }
            catch (NotSupportedException)
            {
                CaptureChildren(property, node);
                return node;
            }
        }

        static void CaptureChildren(SerializedProperty property, Node node)
        {
            var iterator = property.Copy();
            var end = property.GetEndProperty();
            var parentDepth = property.depth;

            if (!iterator.Next(true))
            {
                return;
            }

            do
            {
                if (iterator.depth == parentDepth + 1)
                {
                    node.Children.Add(CaptureNode(iterator.Copy(), iterator.name));
                }
            }
            while (iterator.Next(false) && !SerializedProperty.EqualContents(iterator, end));
        }

        static void RestoreNode(SerializedProperty property, Node node)
        {
            property.isExpanded = node.IsExpanded;

            if (node.IsArray)
            {
                property.arraySize = node.Children.Count;
                for (var i = 0; i < node.Children.Count; i++)
                {
                    RestoreNode(property.GetArrayElementAtIndex(i), node.Children[i]);
                }
                return;
            }

            if (node.HasValue)
            {
                property.boxedValue = node.Value;
                return;
            }

            foreach (var child in node.Children)
            {
                var childProperty = property.FindPropertyRelative(child.RelativePath);
                if (childProperty != null)
                {
                    RestoreNode(childProperty, child);
                }
            }
        }
    }

    internal static class SerializedPropertyUtility
    {
        public static bool TryFindConflict(
            SerializedProperty arrayProperty,
            string valueRelativePath,
            out int conflictIndex,
            out int otherIndex)
        {
            for (var i = 0; i < arrayProperty.arraySize; i++)
            {
                var firstValue = GetComparedValue(arrayProperty, i, valueRelativePath);
                if (IsNullReference(firstValue))
                {
                    conflictIndex = i;
                    otherIndex = -1;
                    return true;
                }

                for (var j = i + 1; j < arrayProperty.arraySize; j++)
                {
                    var secondValue = GetComparedValue(arrayProperty, j, valueRelativePath);
                    if (SerializedProperty.DataEquals(firstValue, secondValue))
                    {
                        conflictIndex = j;
                        otherIndex = i;
                        return true;
                    }
                }
            }

            conflictIndex = -1;
            otherIndex = -1;
            return false;
        }

        public static bool IsNullReference(SerializedProperty property)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.ObjectReference:
                    return property.objectReferenceValue == null;
                case SerializedPropertyType.ManagedReference:
                    return property.managedReferenceValue == null;
                default:
                    return false;
            }
        }

        public static void DeleteArrayElement(SerializedProperty arrayProperty, int index)
        {
            var element = arrayProperty.GetArrayElementAtIndex(index);
            if (element.propertyType == SerializedPropertyType.ObjectReference)
            {
                element.objectReferenceValue = null;
            }

            arrayProperty.DeleteArrayElementAtIndex(index);
        }

        public static GUIContent IconContent(string name, string tooltip)
        {
            var builtinIcon = EditorGUIUtility.IconContent(name);
            return new GUIContent(builtinIcon.image, tooltip);
        }

        static SerializedProperty GetComparedValue(
            SerializedProperty arrayProperty,
            int index,
            string valueRelativePath)
        {
            var element = arrayProperty.GetArrayElementAtIndex(index);
            return string.IsNullOrEmpty(valueRelativePath)
                ? element
                : element.FindPropertyRelative(valueRelativePath);
        }
    }
}
