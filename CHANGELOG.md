# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Changed

- Added sanitization of guid values in query parameters

## [1.3.2] - 2023-09-21

### Changed

- Switched from `Tavis.UriTemplates` to `Std.UriTemplate` for URI template parsing.

## [1.3.1] - 2023-08-08

### Fixed

- Fixed a bug where excess duplicate subscriptions would be created on the same property in the backing store causing performance issues in some scenarios. Related to https://github.com/microsoftgraph/msgraph-sdk-dotnet/issues/1994

## [1.3.0] - 2023-08-01

### Added

- Added support for multipart form data request body serialization.

## [1.2.1] - 2023-07-03

### Fixed

- Fixed a bug that caused the uri parameters not to be applied when the Uri template had a different casing than the parameter name that was used to set it.

## [1.2.0] - 2023-06-28

### Added

- Added an interface to mark composed type wrappers and facilitate serialization.

## [1.1.4] - 2023-06-22

### Fixed

- Use concurrent dictionary for In memory backing store registry to avoid race conditions.

## [1.1.3] - 2023-06-13

### Fixed

- Fixed a bug that would allow multiple "Content-Type", "Content-Length", and "Content-Location" header values.

## [1.1.2] - 2023-05-17

### Changed

- Fixes a bug in the InMemoryBackingStore that would not leave out properties in nested IBackedModel properties.

## [1.1.1] - 2023-04-06

### Added

- Adds the Response Headers to the ApiException class

## [1.1.0] - 2023-03-22

### Added

- Added a base request builder and a request configuration class to reduce the amount of code being generated.

## [1.0.1] - 2023-03-10

### Changed

- Update minimum version of [`System.Diagnostics.DiagnosticSource`](https://www.nuget.org/packages/System.Diagnostics.DiagnosticSource) to `6.0.0`.

## [1.0.0] - 2023-02-27

### Added

- GA release

### Changed

## [1.0.0-rc.7] - 2023-02-03

### Added

- Added a status code field to the API exception class.

## [1.0.0-rc.6] - 2023-01-27

### Changed

- Relaxed nullability tolerance when merging objects for composed types.

## [1.0.0-rc.5] - 2023-01-26

### Changed

- Use concurrent dictionary for serialization registry to avoid race conditions.

### Changed

## [1.0.0-rc.4] - 2023-01-17

### Changed

- Adds support for nullable reference types

## [1.0.0-rc.3] - 2023-01-09

### Changed

- Adds a method to convert abstract requests to native requests in the request adapter interface.

## [1.0.0-rc.2] - 2023-01-05

### Changed

- Release candidate 2
- Prevents sending requests with empty query parameter values

## [1.0.0-rc.1] - 2022-12-15

### Changed

- Release candidate 1

## [1.0.0-preview.19] - 2022-12-13

### Changed

- Added support for multi-valued request headers

## [1.0.0-preview.18] - 2022-11-22

### Changed

- Bumps Tavis.UriTemplates to strongly name binary version

## [1.0.0-preview.17] - 2022-11-11

### Changed

- Fixes a bug in the InMemoryBackingstore that would not detect changes in nested collections of complex types that had backing stores

## [1.0.0-preview.16] - 2022-10-28

### Changed

- Fixed a bug where request bodies that are collections of single items would not serialize properly

## [1.0.0-preview.15] - 2022-10-18

### Added

- Adds an API key authentication provider.

## [1.0.0-preview.14] - 2022-10-17

### Changed

- Changes the ResponeHandler parameter in IRequestAdapter to be a RequestOption

## [1.0.0-preview.13] - 2022-10-05

### Changed

- Fixes a bug in the InMemoryBackingstore that would not detect changes in nested complex types and collections

## [1.0.0-preview.12] - 2022-09-19

### Added

- Added tracing support for request information content type.

## [1.0.0-preview.11] - 2022-09-06

### Added

- Added support for composed types serialization.

## [1.0.0-preview.10] - 2022-08-11

### Changed

- DateTime instances added to the url paths to default to ISO 8601
- Adds explicit error message if the url template expects URI when accessing the URI from RequestInformation

## [1.0.0-preview.9] - 2022-06-13

### Changed

- Fixes a bug where the backing store would fail to be set in clients running .Net framework.

## [1.0.0-preview.8] - 2022-05-11

### Added

- Breaking: added an additional parameter to authentication methods to carry contextual information.

## [1.0.0-preview.7] - 2022-05-11

### Added

- Adds a method to support scalar request bodies

## [1.0.0-preview.6] - 2022-04-22

### Added

- Adds support for api surface revamp for query parameters

## [1.0.0-preview.5] - 2022-04-12

### Changed

- Breaking: Changes target runtime to netstandard2.0

## [1.0.0-preview.4] - 2022-04-06

### Added

- Adds the ability to get the query parameter name from attribute.

## [1.0.0-preview.3] - 2022-04-04

### Changed

- Breaking: simplifies the field deserializers.

## [1.0.0-preview.2] - 2022-03-29

### Added

- Added support for vendor specific serialization in registries

## [1.0.0-preview.1] - 2022-03-18

### Added

- Initial Nuget release
