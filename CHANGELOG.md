# Changelog

All notable changes to this package are documented in this file.

## [0.2.0] - 2026-08-02

### Added

- Runtime PlayMode and Editor tests for serialization, conflict handling and editor state utilities.
- GitHub Actions package validation and optional Unity 2022.3/Unity 6 test matrix.
- OpenUPM installation instructions, package metadata, formal documentation and a Basic Usage sample.
- `DeserializationConflictCount` for inspecting discarded dictionary entries.

### Changed

- Raised the verified minimum Unity version to 2022.3.
- Dictionary deserialization now explicitly retains the first duplicate key and counts discarded duplicate, null and destroyed-object keys.
- Refactored the Editor drawers around shared serialized-property snapshot, identity and weak state-cache utilities.
- Replaced the legacy HashSet drawer implementation with a reorderable list and removed obsolete dictionary-value paths.
- Documented that custom equality comparers are not persisted by Unity serialization.

### Fixed

- Corrected the nested collection example, which previously referenced a missing `SerializableDictionaryStorage` type.
- Prevented drawer list reuse between different properties that happen to contain equal data.
- Prevented static editor conflict-state caches from retaining destroyed Unity objects.
- Corrected layout height and spacing for expandable `SerializableKeyValuePair` values.

## [0.0.3] - 2025-02-28

- Added support for Addressables `AssetReference` fields in the Inspector.

## [0.0.2] - 2021-09-29

- Added dictionary constructors and the reorderable dictionary Inspector.

[0.2.0]: https://github.com/StromKuo/SerializableDictionary/compare/v0.0.3...v0.2.0
[0.0.3]: https://github.com/StromKuo/SerializableDictionary/compare/v0.0.2...v0.0.3
[0.0.2]: https://github.com/StromKuo/SerializableDictionary/releases/tag/v0.0.2
