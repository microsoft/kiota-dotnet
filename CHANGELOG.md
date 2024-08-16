# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

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
