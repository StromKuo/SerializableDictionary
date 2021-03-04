using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace SKUnityToolkit.SerializableDictionary
{
    [CustomPropertyDrawer(typeof(SerializableDictionary<,>), true)]
    public class SerializableDictionaryPropertyDrawer : PropertyDrawer
    {
        const string PairsFieldName = "m_pairs";
        const string KeyFieldName = "m_key";

        static GUIContent s_warningIconConflict = IconContent("console.warnicon.sml", "Conflicting key, this entry will be lost");
        static GUIContent s_warningIconOther = IconContent("console.infoicon.sml", "Conflicting key");
        static GUIContent s_warningIconNull = IconContent("console.warnicon.sml", "Null key, this entry will be lost");

        static Dictionary<PropertyIdentity, ConflictState> s_conflictStateDict = new Dictionary<PropertyIdentity, ConflictState>();

        ReorderableList _reorderableList = null;
        ConflictState _conflictState = null;

        static Dictionary<SerializedPropertyType, PropertyInfo> s_serializedPropertyValueAccessorsDict;

        static SerializableDictionaryPropertyDrawer()
        {
            Dictionary<SerializedPropertyType, string> serializedPropertyValueAccessorsNameDict = new Dictionary<SerializedPropertyType, string>() {
                { SerializedPropertyType.Integer, "intValue" },
                { SerializedPropertyType.Boolean, "boolValue" },
                { SerializedPropertyType.Float, "floatValue" },
                { SerializedPropertyType.String, "stringValue" },
                { SerializedPropertyType.Color, "colorValue" },
                { SerializedPropertyType.ObjectReference, "objectReferenceValue" },
                { SerializedPropertyType.LayerMask, "intValue" },
                { SerializedPropertyType.Enum, "intValue" },
                { SerializedPropertyType.Vector2, "vector2Value" },
                { SerializedPropertyType.Vector3, "vector3Value" },
                { SerializedPropertyType.Vector4, "vector4Value" },
                { SerializedPropertyType.Rect, "rectValue" },
                { SerializedPropertyType.ArraySize, "intValue" },
                { SerializedPropertyType.Character, "intValue" },
                { SerializedPropertyType.AnimationCurve, "animationCurveValue" },
                { SerializedPropertyType.Bounds, "boundsValue" },
                { SerializedPropertyType.Quaternion, "quaternionValue" },
            };
            Type serializedPropertyType = typeof(SerializedProperty);

            s_serializedPropertyValueAccessorsDict = new Dictionary<SerializedPropertyType, PropertyInfo>();
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;

            foreach (var kvp in serializedPropertyValueAccessorsNameDict)
            {
                PropertyInfo propertyInfo = serializedPropertyType.GetProperty(kvp.Value, flags);
                s_serializedPropertyValueAccessorsDict.Add(kvp.Key, propertyInfo);
            }

        }

        static object GetPropertyValue(SerializedProperty p)
        {
            PropertyInfo propertyInfo;
            if (s_serializedPropertyValueAccessorsDict.TryGetValue(p.propertyType, out propertyInfo))
            {
                return propertyInfo.GetValue(p, null);
            }
            else
            {
                if (p.isArray)
                    return GetPropertyValueArray(p);
                else
                    return GetPropertyValueGeneric(p);
            }
        }

        static void SetPropertyValue(SerializedProperty p, object v)
        {
            PropertyInfo propertyInfo;
            if (s_serializedPropertyValueAccessorsDict.TryGetValue(p.propertyType, out propertyInfo))
            {
                propertyInfo.SetValue(p, v, null);
            }
            else
            {
                if (p.isArray)
                    SetPropertyValueArray(p, v);
                else
                    SetPropertyValueGeneric(p, v);
            }
        }

        static object GetPropertyValueArray(SerializedProperty property)
        {
            object[] array = new object[property.arraySize];
            for (int i = 0; i < property.arraySize; i++)
            {
                SerializedProperty item = property.GetArrayElementAtIndex(i);
                array[i] = GetPropertyValue(item);
            }
            return array;
        }

        static object GetPropertyValueGeneric(SerializedProperty property)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            var iterator = property.Copy();
            if (iterator.Next(true))
            {
                var end = property.GetEndProperty();
                do
                {
                    string name = iterator.name;
                    object value = GetPropertyValue(iterator);
                    dict.Add(name, value);
                } while (iterator.Next(false) && iterator.propertyPath != end.propertyPath);
            }
            return dict;
        }

        static void SetPropertyValueArray(SerializedProperty property, object v)
        {
            object[] array = (object[])v;
            property.arraySize = array.Length;
            for (int i = 0; i < property.arraySize; i++)
            {
                SerializedProperty item = property.GetArrayElementAtIndex(i);
                SetPropertyValue(item, array[i]);
            }
        }

        static void SetPropertyValueGeneric(SerializedProperty property, object v)
        {
            Dictionary<string, object> dict = (Dictionary<string, object>)v;
            var iterator = property.Copy();
            if (iterator.Next(true))
            {
                var end = property.GetEndProperty();
                do
                {
                    string name = iterator.name;
                    SetPropertyValue(iterator, dict[name]);
                } while (iterator.Next(false) && iterator.propertyPath != end.propertyPath);
            }
        }

        static GUIContent IconContent(string name, string tooltip)
        {
            var builtinIcon = EditorGUIUtility.IconContent(name);
            return new GUIContent(builtinIcon.image, tooltip);
        }

        static ConflictState GetConflictState(SerializedProperty property)
        {
            ConflictState conflictState;
            PropertyIdentity propId = new PropertyIdentity(property);
            if (!s_conflictStateDict.TryGetValue(propId, out conflictState))
            {
                conflictState = new ConflictState();
                s_conflictStateDict.Add(propId, conflictState);
            }
            return conflictState;
        }

        static void SaveProperty(SerializedProperty pairProperty, int index, int otherIndex, ConflictState conflictState)
        {
            conflictState.conflictPair = GetPropertyValue(pairProperty);
            float pairPropertyHeight = EditorGUI.GetPropertyHeight(pairProperty);
            conflictState.conflictLineHeight = pairPropertyHeight;
            conflictState.conflictIndex = index;
            conflictState.conflictOtherIndex = otherIndex;
            conflictState.conflictPairPropertyExpanded = pairProperty.isExpanded;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label = EditorGUI.BeginProperty(position, label, property);

            var pairsProperty = property.FindPropertyRelative(PairsFieldName);

            _conflictState = GetConflictState(property);

            if (_conflictState.conflictIndex != -1 && _conflictState.conflictIndex <= pairsProperty.arraySize)
            {
                pairsProperty.InsertArrayElementAtIndex(_conflictState.conflictIndex);
                var pairProperty = pairsProperty.GetArrayElementAtIndex(_conflictState.conflictIndex);
                SetPropertyValue(pairProperty, _conflictState.conflictPair);
                pairProperty.isExpanded = _conflictState.conflictPairPropertyExpanded;
            }

            var labelPosition = position;
            labelPosition.height = EditorGUIUtility.singleLineHeight;

            EditorGUI.PropertyField(labelPosition, property, label, false);
            if (property.isExpanded)
            {
                ReorderableList reorderableList = GetList(pairsProperty, label);

                var listRect = position;
                listRect.y += EditorGUIUtility.singleLineHeight;
                reorderableList.DoList(listRect);
            }

            _conflictState.conflictPair = null;
            _conflictState.conflictIndex = -1;
            _conflictState.conflictOtherIndex = -1;
            _conflictState.conflictLineHeight = 0f;
            _conflictState.conflictPairPropertyExpanded = false;

            for (int i = 0; i < pairsProperty.arraySize; i++)
            {
                var pairProperty1 = pairsProperty.GetArrayElementAtIndex(i);
                var keyProperty1 = pairProperty1.FindPropertyRelative(KeyFieldName);
                object keyProperty1Value = GetPropertyValue(keyProperty1);

                if (keyProperty1Value == null)
                {
                    SaveProperty(pairProperty1, i, -1, _conflictState);
                    pairsProperty.DeleteArrayElementAtIndex(i);

                    break;
                }

                for (int j = i + 1; j < pairsProperty.arraySize; j++)
                {
                    var pairProperty2 = pairsProperty.GetArrayElementAtIndex(j);
                    var keyProperty2 = pairProperty2.FindPropertyRelative(KeyFieldName);

                    if (SerializedProperty.DataEquals(keyProperty1, keyProperty2))
                    {
                        SaveProperty(pairProperty2, j, i, _conflictState);
                        pairsProperty.DeleteArrayElementAtIndex(j);

                        goto breakLoops;
                    }
                }
            }
        breakLoops:

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var ret = EditorGUI.GetPropertyHeight(property, false);
            if (property.isExpanded)
            {
                var pairsProperty = property.FindPropertyRelative(PairsFieldName);

                if (_conflictState != null && _conflictState.conflictIndex != -1 && _conflictState.conflictIndex <= pairsProperty.arraySize)
                {
                    pairsProperty.InsertArrayElementAtIndex(_conflictState.conflictIndex);
                    var pairProperty = pairsProperty.GetArrayElementAtIndex(_conflictState.conflictIndex);
                    SetPropertyValue(pairProperty, _conflictState.conflictPair);
                    pairProperty.isExpanded = _conflictState.conflictPairPropertyExpanded;

                    ret += EditorGUI.GetPropertyHeight(pairProperty);

                    pairsProperty.DeleteArrayElementAtIndex(_conflictState.conflictIndex);
                }

                ret += this.GetList(pairsProperty, label).GetHeight();
            }

            return ret;
        }

        private ReorderableList GetList(SerializedProperty pairsProperty, GUIContent label)
        {
            bool shouldNewList = true;
            try
            {
                shouldNewList = _reorderableList == null || !SerializedProperty.DataEquals(_reorderableList.serializedProperty, pairsProperty);
            }
            //SerializedObject of _reorderableList has been Disposed.
            catch (NullReferenceException e)
            {
                Debug.Log(e.Message);
            }

            if (shouldNewList)
            {
                _reorderableList = new ReorderableList(pairsProperty.serializedObject, pairsProperty, true, false, true, true)
                {
                    drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                    {
                        if (index == _conflictState.conflictIndex && _conflictState.conflictOtherIndex == -1)
                        {
                            var iconPosition = rect;
                            iconPosition.size = GUIStyle.none.CalcSize(s_warningIconNull);
                            GUI.Label(iconPosition, s_warningIconNull);

                            rect.xMin += iconPosition.size.x;
                        }
                        else if (index == _conflictState.conflictIndex)
                        {
                            var iconPosition = rect;
                            iconPosition.size = GUIStyle.none.CalcSize(s_warningIconConflict);
                            GUI.Label(iconPosition, s_warningIconConflict);

                            rect.xMin += iconPosition.size.x;
                        }
                        else if (index == _conflictState.conflictOtherIndex)
                        {
                            var iconPosition = rect;
                            iconPosition.size = GUIStyle.none.CalcSize(s_warningIconOther);
                            GUI.Label(iconPosition, s_warningIconOther);

                            rect.xMin += iconPosition.size.x;
                        }

                        var pairProperty = pairsProperty.GetArrayElementAtIndex(index);

                        EditorGUI.PropertyField(rect, pairProperty, label, false);
                    },
                    elementHeightCallback = (int index) =>
                    {
                        var pairProperty = pairsProperty.GetArrayElementAtIndex(index);
                        return EditorGUI.GetPropertyHeight(pairProperty);
                    }
                };
            }

            return _reorderableList;
        }

        class ConflictState
        {
            public object conflictPair = null;
            public int conflictIndex = -1;
            public int conflictOtherIndex = -1;
            public bool conflictPairPropertyExpanded = false;
            public float conflictLineHeight = 0f;
        }

        struct PropertyIdentity
        {
            public PropertyIdentity(SerializedProperty property)
            {
                this.instance = property.serializedObject.targetObject;
                this.propertyPath = property.propertyPath;
            }

            public UnityEngine.Object instance;
            public string propertyPath;
        }
    }
}