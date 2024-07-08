# Changelog (old)

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.3.3] - 2024-05-24

### Changed

- Remove all LINQ usage from repo (except tests)

## [1.3.2] - 2024-05-23

### Changed

- Fixed an issue where fixed versions of abstractions would result in restore failures. [microsoft/kiota-http-dotnet#256](https://github.com/microsoft/kiota-http-dotnet/issues/258)

## [1.3.1] - 2024-05-20

### Changed

- Updated serialization and deserialization of enums to remove LINQ to resolve NativeAOT compatibility issue

## [1.3.0] - 2024-05-13

### Added

- Implements IAsyncParseNodeFactory interface which adds async support

## [1.2.3] - 2024-04-25

### Changed

- Parse empty strings as nullable Guid

## [1.2.2] - 2024-04-19

### Changed

- Replaced the included license by license expression for better auditing capabilities.

## [1.2.1] - 2024-04-17

### Changed

- Have made System.Text.Json only be included on Net Standard's TFM & net 5

## [1.2.0] - 2024-03-22

### Added

- Added support for untyped nodes. (<https://github.com/microsoft/kiota-serialization-json-dotnet/issues/197>)

## [1.1.8] - 2024-02-27

- Reduced `DynamicallyAccessedMembers` scope for enum methods to prevent ILC warnings.

## [1.1.7] - 2024-02-26

- Add ability to use `JsonSerializerContext` (and `JsonSerialzerOptions`) when serializing and deserializing

## [1.1.6] - 2024-02-23

### Changed

- Added `net6.0` and `net8.0` as target frameworks.

## [1.1.5] - 2024-02-05

### Changed

- Fixes `IsTrimmable` property on the project.

## [1.1.4] - 2024-01-30

### Changed

- Fixed AOT warnings caused by reflection on enum types.

## [1.1.3] - 2024-01-29

### Changed

- Fixed a bug where serialization of decimal values would write them as empty objects.

### Added

## [1.1.2] - 2023-11-15

### Added

- Added support for dotnet 8.

## [1.1.1] - 2023-10-23

### Changed

- Fixed a bug where deserialization of downcast type fields would be ignored.

## [1.1.0] - 2023-10-23

### Added

- Added support for dotnet trimming.

## [1.0.8] - 2023-07-14

### Changed

- Fixes deserialization of arrays with item type long

## [1.0.7] - 2023-06-28

### Changed

- Fixed composed types serialization.

## [1.0.6] - 2023-05-19

### Changed

- #86: Fix inheritance new keyword for hiding existing implementation of deserializing method
- #85: Bump Microsoft.NET.Test.Sdk from 17.5.0 to 17.6.0
- #84: Bump Microsoft.TestPlatform.ObjectModel from 17.5.0 to 17.6.0
- #82: Bump dependabot/fetch-metadata from 1.3.6 to 1.4.0
- #81: Bump Microsoft.Kiota.Abstractions from 1.1.0 to 1.1.1

## [1.0.5] - 2023-05-17

### Changed

- Fixes a bug where 'new' keyword on derived classes from IParsable is not being respected, returning null properties for json parsed nodes

### Added

## [1.0.5] - 2023-04-04

### Changed

- Fixes a bug where EnumMember attribute enums would have the first letter lowecased

## [1.0.4] - 2023-04-03

### Changed

- Fixes a bug where EnumMember attribute was not taken into account during serialization/deserialization

## [1.0.3] - 2023-03-15

### Changed

- Fixes serialization of DateTime type in the additionalData

## [1.0.2] - 2023-03-10

### Changed

- Bumps abstraction dependency

## [1.0.1] - 2023-03-08

### Changed

- Update minimum version of [`System.Text.Json`](https://www.nuget.org/packages/System.Text.Json) to `6.0.0`.

## [1.0.0] - 2023-02-27

### Added

- GA release

## [1.0.0-rc.3] - 2023-01-27

### Changed

- Relaxed nullability tolerance when merging objects for composed types.

## [1.0.0-rc.2] - 2023-01-17

### Changed

- Adds support for nullable reference types

## [1.0.0-rc.1] - 2022-12-15

### Changed

- Release candidate 1

## [1.0.0-preview.7] - 2022-09-02

### Added

- Added support for composed types serialization.

## [1.0.0-preview.6] - 2022-05-27

### Changed

- Fixes a bug where JsonParseNode.GetChildNode would throw an exception if the property name did not exist in the json.

## [1.0.0-preview.5] - 2022-05-18

### Changed

- Updated abstractions version to 1.0.0.preview8

## [1.0.0-preview.4] - 2022-04-12

### Changed

- Breaking: Changes target runtime to netstandard2.0

## [1.0.0-preview.3] - 2022-04-11

### Changed

- Fixes handling of JsonElement types in additionData during serialization

## [1.0.0-preview.2] - 2022-04-04

### Changed

- Breaking: simplifies the field deserializers.

## [1.0.0-preview.1] - 2022-03-18

### Added

- Initial Nuget release
