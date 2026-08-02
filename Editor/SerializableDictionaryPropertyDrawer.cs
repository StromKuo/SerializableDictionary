using UnityEditor;
using UnityEngine;

namespace SKUnityToolkit.SerializableDictionary
{
    [CustomPropertyDrawer(typeof(SerializableDictionary<,>), true)]
    public class SerializableDictionaryPropertyDrawer : SerializableCollectionPropertyDrawer
    {
        const string KeyFieldName = "m_key";

        static readonly GUIContent s_conflictIcon = SerializedPropertyUtility.IconContent(
            "console.warnicon.sml", "Duplicate key. The first entry will be retained.");
        static readonly GUIContent s_otherConflictIcon = SerializedPropertyUtility.IconContent(
            "console.infoicon.sml", "This entry has the same key.");
        static readonly GUIContent s_nullIcon = SerializedPropertyUtility.IconContent(
            "console.warnicon.sml", "Null keys are not supported and will be discarded.");

        protected override string ArrayFieldName => "m_pairs";
        protected override string ComparedValueRelativePath => KeyFieldName;
        protected override GUIContent ConflictIcon => s_conflictIcon;
        protected override GUIContent OtherConflictIcon => s_otherConflictIcon;
        protected override GUIContent NullIcon => s_nullIcon;

        protected override void DrawElementContent(Rect rect, SerializedProperty element, int index)
        {
            var keyProperty = element.FindPropertyRelative(KeyFieldName);
            var valueProperty = element.FindPropertyRelative("m_value");
            DrawKeyValuePairHelper.DrawKeyValueLine(keyProperty, valueProperty, rect, index);
        }
    }
}
