---
title: "Kiota .NET Libraries Upgrade Guide: v2.x to v3.x"
applies-to: microsoft/kiota-dotnet
from-version: "2.x"
to-version: "3.x"
breaking-changes:
  - changed-target-frameworks: [netstandard2.0, netstandard2.1, net10.0, net11.0]
  - removed-target-frameworks: [net8.0, net8.0-browser, net462]
---

# Upgrade Guide: Kiota .NET Libraries v2.x to v3.x

## Target Framework Changes

Version 3.x targets **netstandard2.0**, **netstandard2.1**, **net10.0**, and **net11.0**.
The HTTP client library also targets **net481**, **net10.0-browser**, and **net11.0-browser**.

The explicit `net8.0`, `net8.0-browser`, and HTTP client `net462` targets have been removed.
Both .NET Standard targets remain unchanged.
The `netstandard2.0` assets remain available for compatible consumers, including .NET Framework 4.7.2 and later.

**Action required:** Target `net10.0` or `net11.0` to use the modern .NET assets.
Browser consumers should target `net10.0-browser` or `net11.0-browser`.
Upgrade .NET Framework applications to 4.8.1 (`net481`) to use the framework-specific HTTP assets.
Earlier compatible .NET Framework consumers continue to select `netstandard2.0` assets.

The `net481` HTTP assets retain the existing `WinHttpHandler` implementation and its proxy and HTTP/2 behavior.

## Building the Repository

Install the .NET 11 SDK, the .NET 10 runtime, and the .NET Framework 4.8.1 developer pack. The repository builds both modern .NET targets and runs CI tests on .NET 11 and .NET Framework 4.8.1.
Trimming and Native AOT validation target .NET 11.
