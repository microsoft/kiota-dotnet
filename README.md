# Kiota Libraries for dotnet

The Kiota libraries define the basic constructs for Kiota projects needed once an SDK has been generated from an OpenAPI definition and provide default implementations.

A [Kiota](https://github.com/microsoft/kiota) generated project will need a reference to the libraries to build and execute by providing default implementations for serialization, authentication and http transport.

Read more about Kiota [here](https://github.com/microsoft/kiota/blob/main/README.md).

## Build Status

[![Build, Test, CodeQl](https://github.com/microsoft/kiota-abstractions-dotnet/actions/workflows/build-and-test.yml/badge.svg?branch=main)](https://github.com/microsoft/kiota-abstractions-dotnet/actions/workflows/build-and-test.yml)

## Libraries

| Library                                                              | Nuget Release                                                                                                                                                                              |
|----------------------------------------------------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| [Abstractions](./src/abstractions/README.md)                         | [![NuGet Version](https://img.shields.io/nuget/vpre/Microsoft.Kiota.Abstractions?label=Latest&logo=nuget)](https://www.nuget.org/packages/Microsoft.Kiota.Abstractions/)                       |
| [Authentication - Azure](./src/authentication/azure/README.md)       | [![NuGet Version](https://img.shields.io/nuget/vpre/Microsoft.Kiota.Authentication.Azure?label=Latest&logo=nuget)](https://www.nuget.org/packages/Microsoft.Kiota.Authentication.Azure/)       |
| [Http - HttpClientLibrary](./src/http/httpClient/README.md)          | [![NuGet Version](https://img.shields.io/nuget/vpre/Microsoft.Kiota.Http.HttpClientLibrary?label=Latest&logo=nuget)](https://www.nuget.org/packages/Microsoft.Kiota.Http.HttpClientLibrary/)   |
| [Serialization - JSON](./src/serialization/json/README.md)           | [![NuGet Version](https://img.shields.io/nuget/vpre/Microsoft.Kiota.Serialization.Json?label=Latest&logo=nuget)](https://www.nuget.org/packages/Microsoft.Kiota.Serialization.Json/)           |
| [Serialization - FORM](./src/serialization/form/README.md)           | [![NuGet Version](https://img.shields.io/nuget/vpre/Microsoft.Kiota.Serialization.Form?label=Latest&logo=nuget)](https://www.nuget.org/packages/Microsoft.Kiota.Serialization.Form/)           |
| [Serialization - TEXT](./src/serialization/text/README.md)           | [![NuGet Version](https://img.shields.io/nuget/vpre/Microsoft.Kiota.Serialization.Text?label=Latest&logo=nuget)](https://www.nuget.org/packages/Microsoft.Kiota.Serialization.Text/)           |
| [Serialization - MULTIPART](./src/serialization/multipart/README.md) | [![NuGet Version](https://img.shields.io/nuget/vpre/Microsoft.Kiota.Serialization.Multipart?label=Latest&logo=nuget)](https://www.nuget.org/packages/Microsoft.Kiota.Serialization.Multipart/) |
| [Bundle](./src/bundle/README.md)                                     | [![NuGet Version](https://img.shields.io/nuget/vpre/Microsoft.Kiota.Bundle?label=Latest&logo=nuget)](https://www.nuget.org/packages/Microsoft.Kiota.Bundle/)                                   |

## Release notes

The Kiota Libraries releases notes are available from the [CHANGELOG](CHANGELOG.md)

## Debugging

If you are using Visual Studio Code as your IDE, the **launch.json** file already contains the configuration to build and test the library. Otherwise, you can open the **Microsoft.Kiota.sln** with Visual Studio.

## Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit [https://cla.opensource.microsoft.com](https://cla.opensource.microsoft.com).

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Trademarks

This project may contain trademarks or logos for projects, products, or services. Authorized use of Microsoft
trademarks or logos is subject to and must follow
[Microsoft's Trademark & Brand Guidelines](https://www.microsoft.com/legal/intellectualproperty/trademarks/usage/general).
Use of Microsoft trademarks or logos in modified versions of this project must not cause confusion or imply Microsoft sponsorship.
Any use of third-party trademarks or logos are subject to those third-party's policies.
