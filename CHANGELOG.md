# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.15.2] - 2024-11-13

### Changed

- Fixed an issue where System.Diagnostics.DiagnosticSource would be locked version < 9.0.

## [1.15.1] - 2024-11-13

### Added

- Fixes serialization collections of primitives present in additional data. [microsoftgraph/msgraph-sdk-dotnet#2729](https://github.com/microsoftgraph/msgraph-sdk-dotnet/issues/2729)

## [1.15.0] - 2024-11-13

### Added

- Added support for `net9.0`.

## [1.14.0] - 2024-11-06

### Added

- Added `AuthorizationHandler` to authenticate requests and `GraphClientFactory.create(authProvider)` to instantiate
an HttpClient with the built-in Authorization Handler.

## [1.13.2] - 2024-10-28

### Changed

- Added Non-Generic Solution to KiotaDeserialization helper method. [#436](https://github.com/microsoft/kiota-dotnet/pull/436)

## [1.13.1] - 2024-10-10

### Changed

- Updated HTTP span attributes to comply with updated OpenTelemetry semantic conventions. [#344](https://github.com/microsoft/kiota-dotnet/issues/344)

## [1.13.0] - 2024-09-25

### Changed

- Adds extension methods for make serializing easier [#338](https://github.com/microsoft/kiota-dotnet/issues/338).

## [1.12.4] - 2024-09-05

### Changed

- Improves performance of the InMemoryBackingStore when reading properties. [#347](https://github.com/microsoft/kiota-dotnet/issues/347)

## [1.12.3] - 2024-09-03

### Changed

- Fixed optional parameters in KiotaJsonSerialization. [#366](https://github.com/microsoft/kiota-dotnet/issues/366)

## [1.12.2] - 2024-08-23

### Changed

- Fixed a bug where calls to ApiClientBuilder.EnableBackingStoreForParseNodeFactory and ApiClientBuilder.EnableBackingStoreForSerializationWriterFactory would enable a BackingStore around BackingStores. [#2563] (<https://github.com/microsoftgraph/msgraph-sdk-dotnet/issues/2563>) [#2588] (<https://github.com/microsoftgraph/msgraph-sdk-dotnet/issues/2588>)

## [1.12.1] - 2024-08-21

### Changed

- Fixed a bug where the cancellation token would not be passed to the ParseNodeFactory.

## [1.12.0] - 2024-08-20

### Changed

- Improved serialization helper methods to take boolean parameter to override the BackingStore functionality. [#310](https://github.com/microsoft/kiota-dotnet/issues/310)

## [1.11.3] - 2024-08-16

### Changed

- Replaced Convert.FromBase64 by System.Text.Json GetBytesFromBase64 to improve performance (10x improvement).

## [1.11.2] - 2024-08-14

### Changed

- Fixed an additional performance regression with the backing store introduced in version 1.9.2 by #243

## [1.11.1] - 2024-08-12

### Changed

- Fixed a performance regression with the backing store introduced in version 1.9.2 by #243

## [1.11.0] - 2024-08-08

- Enabled Continuous Access evaluation by default.

## [1.10.1] - 2024-08-01

- Cleans up enum serialization to read from attributes for form and text serialization [#284](https://github.com/microsoft/kiota-dotnet/issues/284)
- Pass relevant `JsonWriterOptions` from the `KiotaJsonSerializationContext.Options` to the `Utf8JsonWriter` when writing json to enable customization. [#281](https://github.com/microsoft/kiota-dotnet/issues/281)

## [1.10.0] - 2024-07-17

- Adds Kiota bundle package to provide default adapter with registrations setup. [#290](https://github.com/microsoft/kiota-dotnet/issues/290)

## [1.9.12] - 2024-07-30

- Fix non IParasable object serialization.
- Add basic support for serializing dictionary values in AdditionalData.

## [1.9.11] - 2024-07-22

- Obsoletes custom decompression handler in favor of native client capabilities.

## [1.9.10] - 2024-07-18

- Fix DateTime serialization and deserialization

## [1.9.9] - 2024-07-12

- Fix enum deserialization for SendPrimitiveAsync and SendPrimitiveCollectionAsync

## [1.9.8] - 2024-07-08

- Migrated source of various libraries to mono repository at <https://github.com/microsoft/kiota-dotnet>.

Refer to the following for earlier releases of libraries in the project.

1. [Abstractions](./src/abstractions/Changelog-old.md)
1. [Authentication - Azure](./src/authentication/azure/Changelog-old.md)
1. [Http - HttpClientLibrary](./src/http/httpClient/Changelog-old.md)
1. [Serialization - JSON](./src/serialization/json/Changelog-old.md)
1. [Serialization - FORM](./src/serialization/form/Changelog-old.md)
1. [Serialization - TEXT](./src/serialization/text/Changelog-old.md)
1. [Serialization - MULTIPART](./src/serialization/multipart/Changelog-old.md)
