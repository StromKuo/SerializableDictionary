# SerializableDictionary

[![GitHub Actions](https://github.com/StromKuo/SerializableDictionary/actions/workflows/ci.yml/badge.svg)](https://github.com/StromKuo/SerializableDictionary/actions/workflows/ci.yml) [![Releases](https://img.shields.io/github/release/StromKuo/SerializableDictionary.svg)](https://github.com/StromKuo/SerializableDictionary/releases) [![openupm](https://img.shields.io/npm/v/com.strodio.serializable-dictionary?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.strodio.serializable-dictionary/)

[README](README.md) | [中文文档](README_zh.md)

Serializable `Dictionary`, `HashSet` and `KeyValuePair`-style types with reorderable Unity Inspector controls.

This project was developed from [azixMcAze's Unity-SerializableDictionary](https://github.com/azixMcAze/Unity-SerializableDictionary). See [Third-Party Notices](THIRD%20PARTY%20NOTICES.md).

![Serializable Dictionary Inspector](./Documentation~/SerializableDictionary_screenshot1.png)

![Serializable HashSet Inspector](./Documentation~/SerializableDictionary_screenshot2.png)

## Requirements

- Unity 2022.3 or newer.
- Key and value types must follow Unity's serialization rules.

The package is tested with Unity 2022.3 LTS and Unity 6.

## Installation

### OpenUPM

Run this command in the Unity project directory:

```sh
openupm add com.strodio.serializable-dictionary
```

See the [OpenUPM package page](https://openupm.com/packages/com.strodio.serializable-dictionary/) for scoped-registry installation.

### Git URL

In Package Manager, select **Add package from git URL** and enter:

```text
https://github.com/StromKuo/SerializableDictionary.git
```

For reproducible builds, append a release tag such as `#v0.2.0`.

## Usage

```csharp
using System;
using SKUnityToolkit.SerializableDictionary;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField]
    SerializableDictionary<string, int> itemCounts = new SerializableDictionary<string, int>();

    [SerializeField]
    SerializableHashSet<string> unlockedItems = new SerializableHashSet<string>();

    [SerializeField]
    SerializableKeyValuePair<string, Color> categoryColor =
        new SerializableKeyValuePair<string, Color>("Default", Color.white);
}
```

The types derive from the standard collection implementations, so normal dictionary and set APIs are available at runtime.

## Nested collections

Unity does not directly serialize nested containers. Wrap the inner collection in a serializable class:

```csharp
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ColorList
{
    public List<Color> values = new List<Color>();
}

[SerializeField]
SerializableDictionary<string, ColorList> colorsByCategory =
    new SerializableDictionary<string, ColorList>();
```

## Conflict behavior and limitations

- The first entry for a duplicate dictionary key is retained; later duplicates are discarded.
- Null and destroyed `UnityEngine.Object` keys are discarded.
- `DeserializationConflictCount` reports how many dictionary or HashSet entries were discarded by the latest deserialization.
- Custom equality comparers affect only the current in-memory instance and are not serialized by Unity.
- Serialization order follows collection enumeration order and must not be used as application logic.

See the [full documentation](Documentation~/index.md), [changelog](CHANGELOG.md), and the **Basic Usage** sample available from Package Manager.

## License

[MIT](LICENSE)
