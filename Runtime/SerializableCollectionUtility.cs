namespace SKUnityToolkit.SerializableDictionary
{
    internal static class SerializableCollectionUtility
    {
        public static bool IsNull<T>(T value)
        {
            if (ReferenceEquals(value, null))
            {
                return true;
            }

            var unityObject = (object)value as UnityEngine.Object;
            return !ReferenceEquals(unityObject, null) && unityObject == null;
        }
    }
}
