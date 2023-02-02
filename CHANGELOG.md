# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

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
