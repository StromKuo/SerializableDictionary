using UnityEditor;
using UnityEngine;

namespace SKUnityToolkit.SerializableDictionary
{
    [CustomPropertyDrawer(typeof(SerializableHashSet<>), true)]
    public class SerializableHashSetPropertyDrawer : SerializableCollectionPropertyDrawer
    {
        static readonly GUIContent s_conflictIcon = SerializedPropertyUtility.IconContent(
            "console.warnicon.sml", "Duplicate value. The first entry will be retained.");
        static readonly GUIContent s_otherConflictIcon = SerializedPropertyUtility.IconContent(
            "console.infoicon.sml", "This entry has the same value.");
        static readonly GUIContent s_nullIcon = SerializedPropertyUtility.IconContent(
            "console.warnicon.sml", "Null values are not supported and will be discarded.");

        protected override string ArrayFieldName => "m_keys";
        protected override string ComparedValueRelativePath => null;
        protected override GUIContent ConflictIcon => s_conflictIcon;
        protected override GUIContent OtherConflictIcon => s_otherConflictIcon;
        protected override GUIContent NullIcon => s_nullIcon;

        protected override void DrawElementContent(Rect rect, SerializedProperty element, int index)
        {
            EditorGUI.PropertyField(rect, element, GUIContent.none, true);
        }
    }
}
