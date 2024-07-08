# Changelog (old)

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.4.3] - 2024-05-24

### Changed

- Remove all LINQ usage from product code

## [1.4.2] - 2024-05-21

### Added

- Added an optional parameter to kiota middleware factory so options can be configured directly. [#233](https://github.com/microsoft/kiota-http-dotnet/issues/233)
- `GetDefaultHandlerTypes` added to `KiotaClientFactory` if you're creating your own `HttpClient` and still want to use the default handlers.

### Changed

- Fixed an issue where fixed versions of abstractions would result in restore failures. [#256](https://github.com/microsoft/kiota-http-dotnet/issues/256)

## [1.4.1] - 2024-05-07

## Changed

- Use `SocketsHttpHandler` with `EnableMultipleHttp2Connections` as default HTTP message handler.

## [1.4.0]

## Added

- KiotaClientFactory `create()` overload that accepts a list of handlers.

## [1.3.12] - 2024-04-22

- UriReplacementHandler improvements to be added to middleware pipeline by default and respects options set in the HttpRequestMessage [#242](https://github.com/microsoft/kiota-http-dotnet/issues/242)
- Adds `ConfigureAwait(false)` calls to async calls [#240](https://github.com/microsoft/kiota-http-dotnet/issues/240).

## [1.3.11] - 2024-04-19

## Changed

- Fixes default handler for NET framework to unlock HTTP/2 scenarios [#237](https://github.com/microsoft/kiota-http-dotnet/issues/237)

## [1.3.10] - 2024-04-19

## Changed

- Have made System.* dependencies only be included on Net Standard's TFM & net 5 [#230](https://github.com/microsoft/kiota-http-dotnet/issues/230)

## [1.3.9] - 2024-04-17

## Changed

- Set default request version to be Http/2

## [1.3.8] - 2024-03-25]

## Changed

- When too many retries are attempted, the RetryHandler will now throw an `AggregateException` (instead of an `InvalidOperationException`).
  The `InnerExceptions` property of the `AggregateException` will contain a list of `ApiException` with the HTTP status code and an error message if available.

## [1.3.7] - 2024-02-26

### Changed

- Added `net6.0` and `net8.0` as target frameworks.

## [1.3.6] - 2023-02-05

- Fixes `IsTrimmable` property on the project.

## [1.3.5] - 2023-01-23

### Added

- Adds support for `XXX` status code error mapping to HttpClientRequestAdapter.

## [1.3.4] - 2023-12-29

### Added

- Fixes `ActicitySource` memory leak when the HttpClientRequestAdapter does not construct the HttpClient internally.

## [1.3.3] - 2023-11-28

### Added

- Fixes a bug with internal `CloneAsync` method when using stream content types.

## [1.3.2] - 2023-11-15

### Added

- Added support for dotnet 8.

## [1.3.1] - 2023-11-10

### Added

- Fixes multiple initialization of `ActivitySource` instances on each request send [#161](https://github.com/microsoft/kiota-http-dotnet/issues/161).

## [1.3.0] - 2023-11-02

### Added

- Added uri replacement handler.

## [1.2.0] - 2023-10-23

### Added

- Added support for dotnet trimming.

## [1.1.1] - 2023-08-28

- Fixes a bug where the `ParametersNameDecodingHandler` would also decode query parameter values.

## [1.1.0] - 2023-08-11

### Added

- Added headers inspection handler to allow clients to observe request and response headers.

## [1.0.6] - 2023-07-06

- Fixes a bug where empty streams would be passed to the serializers if the response content header is set.

## [1.0.5] - 2023-06-29

- Fixes regression in request building when the passed httpClient base address ends with a `\`

## [1.0.4] - 2023-06-15

- Fixes a bug where NullReference Exception is thrown if a requestInformation is sent without providing UriTemplate
- RequestAdapter passes `HttpCompletionOption.ResponseHeadersRead` to HttpClient for Stream responses to avoid memory consumption for large payloads.

## [1.0.3] - 2023-06-09

- Added propagating the HttpClientRequestAdapter's supplied HttpClient BaseAddress as the adapter's initial BaseUrl

### Added

## [1.0.2] - 2023-04-06

### Changed

- Includes Response headers in APIException for failed requests.

## [1.0.1] - 2023-03-10

### Changed

- Update minimum version of [`System.Diagnostics.DiagnosticSource`](https://www.nuget.org/packages/System.Diagnostics.DiagnosticSource) to `6.0.0`.
- Update minimum version of [`System.Text.Json`](https://www.nuget.org/packages/System.Text.Json) to `6.0.0`.

## [1.0.0] - 2023-02-27

### Added

- GA release

## [1.0.0-rc.6] - 2023-02-03

### Added

- Added the HTTP response status code on API exception.

## [1.0.0-rc.5] - 2023-01-23

### Changed

- Aligns the HttpClientRequestAdapter with other langugages to use the BaseUrl from the RequestAdapter as the baseUrl for making requests.

## [1.0.0-rc.4] - 2023-01-09

### Added

- Adds support for nullalbe reference types.

## [1.0.0-rc.3] - 2023-01-09

### Added

- Added a method to convert abstract requests to native requests in the request adapter interface.

## [1.0.0-rc.2] - 2023-01-05

### Added

- Adds this library version as a product in the user-agent

## [1.0.0-rc.1] - 2022-12-15

### Changed

- Release candidate 1

### Changed

## [1.0.0-preview.13] - 2022-12-14

### Changed

- Added multi-value headers support.

## [1.0.0-preview.12] - 2022-12-01

### Changed

- Fixes RetryHandler to return the real wait time

## [1.0.0-preview.11] - 2022-10-17

### Changed

- Changes the ResponseHandler parameter in IRequestAdapter to be a RequestOption

## [1.0.0-preview.10] - 2022-09-19

### Added

- Added tracing support through OpenTelemetry.

## [1.0.0-preview.9] - 2022-09-07

### Added

- Added support for additional status codes.

## [1.0.0-preview.8] - 2022-05-19

### Changed

- Fixed a bug where CAE support would keep connections open when retrying.

## [1.0.0-preview.7] - 2022-05-13

### Added

- Added support for continuous access evaluation.

## [1.0.0-preview.6] - 2022-04-12

### Changed

- Breaking: Changes target runtime to netstandard2.0

## [1.0.0-preview.5] - 2022-04-07

### Added

- Added supports for decoding parameter names.

## [1.0.0-preview.4] - 2022-04-06

### Changed

- Fix issue with `HttpRequestAdapter` returning disposed streams when the requested return type is a Stream [#10](https://github.com/microsoft/kiota-http-dotnet/issues/10)

## [1.0.0-preview.3] - 2022-03-28

### Added

- Added support for 204 no content responses

### Changed

- Fixed a bug where BaseUrl would not be set in some scenarios

## [1.0.0-preview.2] - 2022-03-18

### Changed

- Fixed a bug where scalar request would not deserialize correctly.

## [1.0.0-preview.1] - 2022-03-18

### Added

- Initial Nuget release
