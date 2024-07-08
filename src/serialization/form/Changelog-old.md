# Changelog (old)

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.2.5] - 2024-06-26

### Changed

- Fixed a bug where new parse nodes would be missing event receivers. [#153](https://github.com/microsoft/kiota-serialization-form-dotnet/issues/153)

## [1.2.4] - 2024-05-23

### Changed

- Remove all LINQ usage from repo

## [1.2.3] - 2024-05-23

### Changed

- Fixed an issue where fixed versions of abstractions would result in restore failures. [microsoft/kiota-http-dotnet#256](https://github.com/microsoft/kiota-http-dotnet/issues/258)

## [1.2.2] - 2024-05-21

### Changed

- Updated serialization and deserialization of enum collection to remove LINQ to reduce NativeAOT output size

## [1.2.1] - 2024-05-20

### Changed

- Updated serialization and deserialization of enums to remove LINQ to resolve NativeAOT compatibility issue

## [1.2.0] - 2024-05-13

### Added

- Implements IAsyncParseNodeFactory interface which adds async support

## [1.1.6] - 2024-04-19

### Changed

- Switch to license expression & bump abstractions (<https://github.com/microsoft/kiota-serialization-form-dotnet/issues/130>)

## [1.1.5] - 2024-02-27

### Changed

- Reduced `DynamicallyAccessedMembers` scope for enum methods to prevent ILC warnings.

## [1.1.4] - 2024-02-26

### Changed

- Added `net6.0` and `net8.0` as target frameworks.

## [1.1.3] - 2024-02-05

### Changed

- Fixes `IsTrimmable` property on the project.

## [1.1.2] - 2024-01-30

### Changed

- Fixed some AOT warnings due to reflection use on enum types.

## [1.1.1] - 2023-11-15

### Added

- Added support for dotnet 8.

## [1.1.0] - 2023-10-23

### Added

- Added support for dotnet trimming.

## [1.0.2] - 2023-03-10

### Changed

- Bumps abstraction dependency

## [1.0.0] - 2023-02-27

### Added

- GA release

### Changed

## [1.0.0-rc.5] - 2023-02-20

### Changed

- Adds support rendering collection of values

## [1.0.0-rc.4] - 2023-01-27

### Changed

- Relaxed nullability tolerance when merging objects for composed types.

## [1.0.0-rc.3] - 2023-01-17

### Changed

- Adds support for nullable reference types

## [1.0.0-rc.2] - 2022-12-16

### Changed

- Fixed key encoding.

## [1.0.0-rc.1] - 2022-12-15

### Changed

- Release candidate 1

## [1.0.0-preview.1] - 2022-12-15

### Added

- Initial Nuget release.
