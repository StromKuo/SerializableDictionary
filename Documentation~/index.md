# Serializable Dictionary 0.2.0

Serializable Dictionary provides serializable `Dictionary`, `HashSet` and
`KeyValuePair`-style types with reorderable Inspector controls.

## Requirements

- Unity 2022.3 or newer.
- Key and value types must follow Unity's normal serialization rules.

The package is tested against Unity 2022.3 LTS and Unity 6.

The CI workflow always validates the UPM layout. Repository maintainers can enable the Unity
EditMode and PlayMode matrix by configuring GameCI license secrets and setting the repository
variable `RUN_UNITY_TESTS` to `true`.

## Runtime types

- `SerializableDictionary<TKey, TValue>` derives from `Dictionary<TKey, TValue>`.
- `SerializableHashSet<T>` derives from `HashSet<T>`.
- `SerializableKeyValuePair<TKey, TValue>` exposes read-only `Key` and `Value` properties.

All types live in the `SKUnityToolkit.SerializableDictionary` namespace.

## Conflict behavior

A dictionary cannot contain null or duplicate keys, and this package treats null HashSet values
as invalid. During deserialization:

1. The first entry for a key is retained.
2. Later duplicate entries are discarded.
3. Null keys and destroyed `UnityEngine.Object` keys are discarded.
4. The number of discarded entries is available from `DeserializationConflictCount`.

The Inspector keeps an invalid entry visible with a warning so that it can be corrected.

## Equality comparers

Comparer constructors affect the current in-memory collection. Unity does not serialize an
`IEqualityComparer<T>`, so a custom comparer is not guaranteed to survive a Unity serialization
round trip. Prefer the default comparer for serialized fields unless the collection is rebuilt
by code.

## Nested collections

Unity does not directly serialize nested containers such as `List<List<T>>`. Wrap the inner
collection in a serializable class:

```csharp
[Serializable]
public class ColorList
{
    public List<Color> values = new List<Color>();
}

[SerializeField]
SerializableDictionary<string, ColorList> colorsByCategory;
```

## Samples

Open Package Manager, select Serializable Dictionary, and import **Basic Usage** from the
Samples tab.

## Known limitations

- Dictionary and HashSet serialization order follows collection enumeration order; do not use
  serialized order as application logic.
- Custom comparers are not serialized.
- Unsupported Unity field types remain subject to Unity's serialization rules.
