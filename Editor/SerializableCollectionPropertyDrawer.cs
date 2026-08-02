using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace SKUnityToolkit.SerializableDictionary
{
    public abstract class SerializableCollectionPropertyDrawer : PropertyDrawer
    {
        static readonly SerializedPropertyStateCache<ConflictState> s_conflictStates =
            new SerializedPropertyStateCache<ConflictState>();

        ReorderableList m_reorderableList;
        ConflictState m_conflictState;

        protected abstract string ArrayFieldName { get; }
        protected abstract string ComparedValueRelativePath { get; }
        protected abstract GUIContent ConflictIcon { get; }
        protected abstract GUIContent OtherConflictIcon { get; }
        protected abstract GUIContent NullIcon { get; }

        protected abstract void DrawElementContent(Rect rect, SerializedProperty element, int index);

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label = EditorGUI.BeginProperty(position, label, property);
            var arrayProperty = property.FindPropertyRelative(ArrayFieldName);
            m_conflictState = s_conflictStates.Get(property);

            RestoreConflict(arrayProperty, m_conflictState);

            var labelPosition = position;
            labelPosition.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(labelPosition, property, label, false);

            if (property.isExpanded)
            {
                var listRect = position;
                listRect.y += EditorGUIUtility.singleLineHeight;
                GetList(arrayProperty).DoList(listRect);
            }

            ResetConflict(m_conflictState);
            FindAndTemporarilyRemoveConflict(arrayProperty, m_conflictState);
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var height = EditorGUI.GetPropertyHeight(property, false);
            if (!property.isExpanded)
            {
                return height;
            }

            var arrayProperty = property.FindPropertyRelative(ArrayFieldName);
            var state = s_conflictStates.Get(property);
            if (state.Index >= 0 && state.Snapshot != null)
            {
                height += state.Height;
            }

            m_conflictState = state;
            return height + GetList(arrayProperty).GetHeight();
        }

        ReorderableList GetList(SerializedProperty arrayProperty)
        {
            if (m_reorderableList == null ||
                !SerializedPropertyIdentity.RefersToSameProperty(m_reorderableList.serializedProperty, arrayProperty))
            {
                m_reorderableList = new ReorderableList(
                    arrayProperty.serializedObject, arrayProperty, true, false, true, true)
                {
                    drawElementCallback = DrawElement,
                    elementHeightCallback = index =>
                        EditorGUI.GetPropertyHeight(arrayProperty.GetArrayElementAtIndex(index), true)
                };
            }

            return m_reorderableList;
        }

        void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            DrawConflictIcon(ref rect, index, m_conflictState);
            var element = m_reorderableList.serializedProperty.GetArrayElementAtIndex(index);
            DrawElementContent(rect, element, index);
        }

        void DrawConflictIcon(ref Rect rect, int index, ConflictState state)
        {
            GUIContent icon = null;
            if (index == state.Index)
            {
                icon = state.OtherIndex < 0 ? NullIcon : ConflictIcon;
            }
            else if (index == state.OtherIndex)
            {
                icon = OtherConflictIcon;
            }

            if (icon == null)
            {
                return;
            }

            var iconPosition = rect;
            iconPosition.size = GUIStyle.none.CalcSize(icon);
            GUI.Label(iconPosition, icon);
            rect.xMin += iconPosition.width;
        }

        void FindAndTemporarilyRemoveConflict(SerializedProperty arrayProperty, ConflictState state)
        {
            if (!SerializedPropertyUtility.TryFindConflict(
                    arrayProperty, ComparedValueRelativePath, out var conflictIndex, out var otherIndex))
            {
                return;
            }

            var conflictElement = arrayProperty.GetArrayElementAtIndex(conflictIndex);
            state.Snapshot = SerializedPropertySnapshot.Capture(conflictElement);
            state.Height = EditorGUI.GetPropertyHeight(conflictElement, true);
            state.Index = conflictIndex;
            state.OtherIndex = otherIndex;
            SerializedPropertyUtility.DeleteArrayElement(arrayProperty, conflictIndex);
        }

        static void RestoreConflict(SerializedProperty arrayProperty, ConflictState state)
        {
            if (state.Index < 0 || state.Index > arrayProperty.arraySize || state.Snapshot == null)
            {
                return;
            }

            arrayProperty.InsertArrayElementAtIndex(state.Index);
            state.Snapshot.Restore(arrayProperty.GetArrayElementAtIndex(state.Index));
        }

        static void ResetConflict(ConflictState state)
        {
            state.Snapshot = null;
            state.Index = -1;
            state.OtherIndex = -1;
            state.Height = 0f;
        }

        sealed class ConflictState
        {
            public SerializedPropertySnapshot Snapshot;
            public int Index = -1;
            public int OtherIndex = -1;
            public float Height;
        }
    }
}
