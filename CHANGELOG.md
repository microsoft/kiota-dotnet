# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.21.0](https://github.com/microsoft/kiota-dotnet/compare/v1.20.1...v1.21.0) (2025-10-28)


### Features

* Add SetContentFromEnumCollection method to RequestInformation class ([d970e8e](https://github.com/microsoft/kiota-dotnet/commit/d970e8edcf4c18b906d8754537eb6b8583c227e1))


### Bug Fixes

* Restore constructor backwards compatibility on old .NET versions. ([ce59749](https://github.com/microsoft/kiota-dotnet/commit/ce5974988f6636accc48388358c43825f2703339))
* Restore constructor compatibility consuming a downlevel library on .NET 5+. ([36c543e](https://github.com/microsoft/kiota-dotnet/commit/36c543ed1c44daea781fd4c969d8c38a2301f173))
* Restore HttpClientRequestAdapter constructor compatibility between .NET 5+ and .NET Standard/Framework ([08edbad](https://github.com/microsoft/kiota-dotnet/commit/08edbada7f1747f50df334654a71ef77cbdf02e4))

## [1.20.1](https://github.com/microsoft/kiota-dotnet/compare/v1.20.0...v1.20.1) (2025-09-26)


### Bug Fixes

* upgrades azure core dependency ([3d56749](https://github.com/microsoft/kiota-dotnet/commit/3d56749da4a3ec63c2e5d47fd43abfb8d4caeb8a))

## [1.20.0](https://github.com/microsoft/kiota-dotnet/compare/v1.19.1...v1.20.0) (2025-09-23)


### Features

* adds the ability to pass the http version for netstandard/netfx/netcore instead of using a hardcoded value (default http/2) ([4aa10e1](https://github.com/microsoft/kiota-dotnet/commit/4aa10e17e5385e2169f253bc5547f0e67c3ac556))


### Bug Fixes

* get the http version and version policy from the http client for net5+ instead of using a hardcoded value (default http/2) ([4aa10e1](https://github.com/microsoft/kiota-dotnet/commit/4aa10e17e5385e2169f253bc5547f0e67c3ac556))

## [1.19.1](https://github.com/microsoft/kiota-dotnet/compare/v1.19.0...v1.19.1) (2025-07-04)


### Bug Fixes

* ConfigureAwait(false) deadlock issue in BaseBearerTokenAuthenticationProvider ([473b886](https://github.com/microsoft/kiota-dotnet/commit/473b88693ce1a821a2b8218c5a85cfda241351c6))

## [1.19.0](https://github.com/microsoft/kiota-dotnet/compare/v1.18.0...v1.19.0) (2025-06-18)


### Features

* optionally parse Decimal as string ([ea198e0](https://github.com/microsoft/kiota-dotnet/commit/ea198e011a89474698c3880fbcd38477dd41a694))
* optionally parse Double as string ([e205169](https://github.com/microsoft/kiota-dotnet/commit/e205169d6bae1847884c4b900784b34867e12a2d))
* optionally parse Float as string ([cbee369](https://github.com/microsoft/kiota-dotnet/commit/cbee369dfe3cf26b001980318d3120d9229db6dd))
* optionally parse Int as string ([51fc88a](https://github.com/microsoft/kiota-dotnet/commit/51fc88a2482365383302ad733c36a613050344e6))
* optionally parse Long as string ([54c132d](https://github.com/microsoft/kiota-dotnet/commit/54c132d037b3a07d3c9167005cd085163cee5ea1))

## [1.18.0](https://github.com/microsoft/kiota-dotnet/compare/v1.17.4...v1.18.0) (2025-06-13)


### Features

* Add set content from enum ([#544](https://github.com/microsoft/kiota-dotnet/issues/544)) ([3c1356d](https://github.com/microsoft/kiota-dotnet/commit/3c1356df35a5a9ba1aedc3d4ad55f2687041e3c3))

## [1.17.4](https://github.com/microsoft/kiota-dotnet/compare/v1.17.3...v1.17.4) (2025-06-02)


### Bug Fixes

* decimal type conversion in URL templates (query or path parameters) ([c27fbeb](https://github.com/microsoft/kiota-dotnet/commit/c27fbeba5e0a86683e042ea8e726badd7773e916))

## [1.17.3](https://github.com/microsoft/kiota-dotnet/compare/v1.17.2...v1.17.3) (2025-05-22)


### Bug Fixes

* a bug where the backing store callbacks would run twice ([8fd7a9d](https://github.com/microsoft/kiota-dotnet/commit/8fd7a9d66ea5c318f585ada82e3709065f75434b))

## [1.17.2](https://github.com/microsoft/kiota-dotnet/compare/v1.17.1...v1.17.2) (2025-04-03)


### Bug Fixes

* MultiPartBody: Add Support for multiple Body Parts with the same name ([#530](https://github.com/microsoft/kiota-dotnet/issues/530)) ([21bfc3f](https://github.com/microsoft/kiota-dotnet/commit/21bfc3fcea2aef5bdb700e710fcdeb59379d0353))

## [1.17.1](https://github.com/microsoft/kiota-dotnet/compare/v1.17.0...v1.17.1) (2025-02-12)


### Bug Fixes

* inverted server.name and url.scheme open telemetry attribute values ([c41fba8](https://github.com/microsoft/kiota-dotnet/commit/c41fba86a531f540ac03ee41a8e5c5738eacd490))

## [1.17.0](https://github.com/microsoft/kiota-dotnet/compare/v1.16.4...v1.17.0) (2025-02-06)


### Features

* Add trim-safe handler lookup ([cd2520f](https://github.com/microsoft/kiota-dotnet/commit/cd2520f931a8d4ea82f5c7e279abcc9f2d96ccf7))
* Add trim-safe handler lookup ([a2800ab](https://github.com/microsoft/kiota-dotnet/commit/a2800ab7c9bfa0292b8490b91962636917fa8c4c))

## [1.16.4](https://github.com/microsoft/kiota-dotnet/compare/v1.16.3...v1.16.4) (2025-01-15)


### Bug Fixes

* browser/wasm message handler being returned was incompatible ([fa0c59e](https://github.com/microsoft/kiota-dotnet/commit/fa0c59e3bffdd868e00381148e6f674eaf035524))
* browser/wasm message handler being returned was incompatible ([aaf4ced](https://github.com/microsoft/kiota-dotnet/commit/aaf4ced980c7368cc4854a91a4b9bef2c78093ef))
* removes message handler properties that don't exist for browser ([45a41a9](https://github.com/microsoft/kiota-dotnet/commit/45a41a9679b3f8cd0badeffbbe7ad4450f825dc6))

## [1.16.3](https://github.com/microsoft/kiota-dotnet/compare/v1.16.2...v1.16.3) (2025-01-07)


### Bug Fixes

* removes bom from encoding ([7ef6862](https://github.com/microsoft/kiota-dotnet/commit/7ef6862e76a08b59fdc4ce9d3240d21ac4f33114))

## [1.16.2] - 2024-01-07

- Fixed inspecting of response body when response http content is not buffered. [#501](https://github.com/microsoft/kiota-dotnet/issues/501)
- Fixed a misalignment in return nullability for IParseNode GetObjectValue. [#429](https://github.com/microsoft/kiota-dotnet/issues/429)

## [1.16.1] - 2024-12-18

### Changed

- Aligned retry open telemetry attributes names with latest specification. [#324](https://github.com/microsoft/kiota-dotnet/issues/324)
- Fixed a bug where the client would fail on 301/302 responses with no location headers. [#272](https://github.com/microsoft/kiota-dotnet/issues/272)

## [1.16.0] - 2024-12-13

### Added

- Added body inspection handler to enable inspection of request and response bodies. [#482](https://github.com/microsoft/kiota-dotnet/issues/482)

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
