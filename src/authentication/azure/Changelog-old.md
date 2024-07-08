# Changelog (old)

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.1.7] - 2024-05-24

### Changed

- Remove all LINQ usage from product code

## [1.1.6] - 2024-05-23

### Changed

- Fixed an issue where fixed versions of abstractions would result in restore failures. [microsoft/kiota-http-dotnet#256](https://github.com/microsoft/kiota-http-dotnet/issues/258)

## [1.1.5] - 2024-04-19

- Have made System.Diagnostics.DiagnosticSource only be included on Net Standard's TFM & net 5 (<https://github.com/microsoft/kiota-authentication-azure-dotnet/issues/191>)

## [1.1.4] - 2024-02-26

### Added

- Added `net6.0` and `net8.0` as target frameworks.

## [1.1.3] - 2024-02-05

### Changed

- Fixes `IsTrimmable` property on the project.

## [1.1.2] - 2023-11-15

### Added

- Added support for dotnet 8.

## [1.1.1] - 2023-11-03

### Added

- Allow http scheme on localhost.

## [1.1.0] - 2023-10-23

### Added

- Added support for dotnet trimming.

## [1.0.3] - 2023-06-26

### Changed

- Fix unwanted scopes collection modification in AzureIdentityAccessTokenProvider ([#73]([https://github.com/microsoft/kiota-authentication-azure-dotnet/issues/93])).
- Add missing ConfigureAwait(false) to GetTokenAsync call.
- Replaced true/false values in SetTag method calls with pre-initialized values to prevent boxing.

## [1.0.2] - 2023-03-24

### Changed

- Update minimum version of [`Azure.Core`]([https://www.nuget.org/packages/System.Diagnostics.DiagnosticSource](https://www.nuget.org/packages/Azure.Core)) to `1.3.0` to fix Azure.Blob storage issues. <https://github.com/Azure/azure-sdk-for-net/issues/35010>

### Changed

## [1.0.1] - 2023-03-10

### Changed

- Update minimum version of [`System.Diagnostics.DiagnosticSource`](https://www.nuget.org/packages/System.Diagnostics.DiagnosticSource) to `6.0.0`.

## [1.0.0] - 2023-02-27

### Added

- GA release

## [1.0.0-rc.3] - 2023-01-17

### Added

- Adds support for nullabe reference types

## [1.0.0-rc.2] - 2023-01-16

### Changed

- Removed microsoft graph specific constants to make usage easier for other MIP protected APIs.

## [1.0.0-rc.1] - 2022-12-15

### Changed

- Release candidate 1

## [1.0.0-preview.5] - 2022-12-12

### Changed

- Updates abstractions reference to add support for multi-valued headers.

## [1.0.0-preview.4] - 2022-09-19

### Added

- Added tracing through Open Telemetry.

## [1.0.0-preview.3] - 2022-05-17

### Added

- Added support for continuous access evaluation.

## [1.0.0-preview.2] - 2022-04-12

### Changed

- Breaking: Changes target runtime to netstandard2.0

## [1.0.0-preview.1] - 2022-03-18

### Added

- Initial Nuget release
