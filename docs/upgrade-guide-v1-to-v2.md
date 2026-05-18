---
title: "Kiota .NET Libraries Upgrade Guide: v1.x to v2.x"
applies-to: microsoft/kiota-dotnet
from-version: "1.x"
to-version: "2.x"
breaking-changes:
  - removed-interface: IAsyncParseNodeFactory
  - removed-method: IParseNodeFactory.GetRootParseNode
  - removed-method: KiotaSerializer.Deserialize
  - removed-method: KiotaSerializer.DeserializeCollection
  - removed-method: KiotaJsonSerializer.Deserialize
  - removed-method: KiotaJsonSerializer.DeserializeCollection
  - removed-method: MultipartBody.GetPartValue<T>(string)
  - removed-method: MultipartBody.RemovePart(string)
  - removed-method: ParseNodeFactoryRegistry.GetFactory<T>
  - changed-target-frameworks: [net8.0, net10.0, netstandard2.0, netstandard2.1]
  - removed-target-frameworks: [net5.0, net6.0]
  - minimum-dependency: Microsoft.Extensions.DependencyInjection.Abstractions >= 8.0
  - minimum-dependency: System.Text.Json >= 8.0
---

# Upgrade Guide: Kiota .NET Libraries v1.x to v2.x

This guide covers the breaking changes introduced in version 2.0 of the Kiota .NET libraries and provides instructions for updating your code.

## Summary of Breaking Changes

| Area | Change |
|------|--------|
| Target Frameworks | Dropped `net5.0`, `net6.0`; minimum is now `net8.0` |
| `IAsyncParseNodeFactory` | Merged into `IParseNodeFactory`; interface removed |
| `IParseNodeFactory.GetRootParseNode` | Sync method removed; use `GetRootParseNodeAsync` |
| `KiotaSerializer` / `KiotaJsonSerializer` | Sync `Deserialize` and `DeserializeCollection` methods removed |
| `MultipartBody` | Single-parameter `GetPartValue<T>(string)` and `RemovePart(string)` removed |
| Dependency Versions | `Microsoft.Extensions.DependencyInjection.Abstractions` ≥ 8.0, `System.Text.Json` ≥ 8.0 |

---

## Target Framework Changes

Version 2.x targets **netstandard2.0**, **netstandard2.1**, **net8.0**, and **net10.0**.

The following targets have been removed:

- `net5.0`
- `net6.0`

**Action required:** Update your project to target `net8.0` or later. If you are using `netstandard2.0` or `netstandard2.1`, no change is needed for the target framework itself, but you must ensure the minimum dependency versions listed below are satisfied.

---

## IParseNodeFactory Interface Consolidation

The `IAsyncParseNodeFactory` interface has been merged into `IParseNodeFactory`. The synchronous `GetRootParseNode` method has been removed.

### Before (v1.x)

```csharp
// IParseNodeFactory had a synchronous method
public class MyParseNodeFactory : IParseNodeFactory
{
    public string ValidContentType => "application/json";

    public IParseNode GetRootParseNode(string contentType, Stream content)
    {
        // synchronous implementation
    }
}

// IAsyncParseNodeFactory extended IParseNodeFactory with an async method
public class MyAsyncParseNodeFactory : IAsyncParseNodeFactory
{
    public string ValidContentType => "application/json";

    public IParseNode GetRootParseNode(string contentType, Stream content)
    {
        // synchronous implementation (required by base interface)
    }

    public Task<IParseNode> GetRootParseNodeAsync(string contentType, Stream content,
        CancellationToken cancellationToken = default)
    {
        // async implementation
    }
}
```

### After (v2.x)

```csharp
// IAsyncParseNodeFactory no longer exists.
// IParseNodeFactory now only has the async method.
public class MyParseNodeFactory : IParseNodeFactory
{
    public string ValidContentType => "application/json";

    public Task<IParseNode> GetRootParseNodeAsync(string contentType, Stream content,
        CancellationToken cancellationToken = default)
    {
        // async implementation
    }
}
```

### Migration Steps

1. Remove any `using` statements referencing `IAsyncParseNodeFactory`.
2. If your class implemented `IAsyncParseNodeFactory`, change it to implement `IParseNodeFactory`.
3. Remove the synchronous `GetRootParseNode` method.
4. Ensure your class implements `GetRootParseNodeAsync`.

---

## ParseNodeProxyFactory Changes

If you have a custom class extending `ParseNodeProxyFactory`, remove any override of the synchronous `GetRootParseNode` method. The base class now only requires `GetRootParseNodeAsync` from the concrete factory.

---

## KiotaSerializer Deserialization Methods

All synchronous deserialization methods have been removed from `KiotaSerializer` and `KiotaJsonSerializer`. Use the async equivalents instead.

### Removed Methods

- `KiotaSerializer.Deserialize<T>(string contentType, Stream stream, ParsableFactory<T> parsableFactory)`
- `KiotaSerializer.Deserialize<T>(string contentType, string serializedRepresentation, ParsableFactory<T> parsableFactory)`
- `KiotaSerializer.Deserialize<T>(string contentType, Stream stream)`
- `KiotaSerializer.Deserialize<T>(string contentType, string serializedRepresentation)`
- `KiotaSerializer.DeserializeCollection<T>(string contentType, Stream stream, ParsableFactory<T> parsableFactory)`
- `KiotaSerializer.DeserializeCollection<T>(string contentType, string serializedRepresentation, ParsableFactory<T> parsableFactory)`
- `KiotaSerializer.DeserializeCollection<T>(string contentType, Stream stream)`
- `KiotaSerializer.DeserializeCollection<T>(string contentType, string serializedRepresentation)`
- Same set of methods on `KiotaJsonSerializer`

### Before (v1.x)

```csharp
// Synchronous deserialization
var result = KiotaSerializer.Deserialize<MyModel>("application/json", stream, MyModel.CreateFromDiscriminatorValue);
var collection = KiotaSerializer.DeserializeCollection<MyModel>("application/json", jsonString, MyModel.CreateFromDiscriminatorValue);
```

### After (v2.x)

```csharp
// Use async deserialization
var result = await KiotaSerializer.DeserializeAsync<MyModel>("application/json", stream, MyModel.CreateFromDiscriminatorValue);
var collection = await KiotaSerializer.DeserializeCollectionAsync<MyModel>("application/json", jsonString, MyModel.CreateFromDiscriminatorValue);
```

### Migration Steps

1. Replace all calls to `Deserialize` with `await DeserializeAsync`.
2. Replace all calls to `DeserializeCollection` with `await DeserializeCollectionAsync`.
3. Ensure the calling method is `async` and returns a `Task` or `Task<T>`.

---

## MultipartBody Method Changes

The single-parameter overloads of `GetPartValue` and `RemovePart` have been removed. Use the two-parameter overloads that accept `fileName`.

### Removed Methods

- `MultipartBody.GetPartValue<T>(string partName)`
- `MultipartBody.RemovePart(string partName)`

### Before (v1.x)

```csharp
var body = new MultipartBody();
body.AddOrReplacePart("file", "application/json", content, "data.json");

// Single-parameter overloads
var value = body.GetPartValue<string>("file");
body.RemovePart("file");
```

### After (v2.x)

```csharp
var body = new MultipartBody();
body.AddOrReplacePart("file", "application/json", content, "data.json");

// Two-parameter overloads (pass null for fileName if not applicable)
var value = body.GetPartValue<string>("file", "data.json");
body.RemovePart("file", "data.json");
```

---

## Dependency Version Updates

The minimum required versions for the following packages have been updated:

| Package | v1.x Minimum | v2.x Minimum |
|---------|-------------|-------------|
| `Microsoft.Extensions.DependencyInjection.Abstractions` | 6.0 | 8.0 |
| `System.Text.Json` | 6.0.10 | 8.0 |

**Action required:** If you pin these dependencies in your project, update them to version 8.0 or later.

---

## ParseNodeFactoryRegistry Changes

The generic `GetFactory<T>()` method has been removed. Use the non-generic `GetFactory(string contentType)` method instead.

### Before (v1.x)

```csharp
var registry = ParseNodeFactoryRegistry.DefaultInstance;
var factory = registry.GetFactory<IParseNodeFactory>(contentType);
```

### After (v2.x)

```csharp
var registry = ParseNodeFactoryRegistry.DefaultInstance;
var (factory, resolvedContentType) = registry.GetFactory(contentType);
```

The method now returns a tuple of `(IParseNodeFactory Factory, string ContentType)` containing the resolved factory and the normalized content type that was matched.

---

## Quick Reference: Find and Replace

| v1.x Pattern | v2.x Replacement |
|---|---|
| `: IAsyncParseNodeFactory` | `: IParseNodeFactory` |
| `GetRootParseNode(contentType, stream)` | `await GetRootParseNodeAsync(contentType, stream)` |
| `KiotaSerializer.Deserialize<T>(...)` | `await KiotaSerializer.DeserializeAsync<T>(...)` |
| `KiotaSerializer.DeserializeCollection<T>(...)` | `await KiotaSerializer.DeserializeCollectionAsync<T>(...)` |
| `KiotaJsonSerializer.Deserialize<T>(...)` | `await KiotaJsonSerializer.DeserializeAsync<T>(...)` |
| `KiotaJsonSerializer.DeserializeCollection<T>(...)` | `await KiotaJsonSerializer.DeserializeCollectionAsync<T>(...)` |
| `.GetPartValue<T>(partName)` | `.GetPartValue<T>(partName, fileName)` |
| `.RemovePart(partName)` | `.RemovePart(partName, fileName)` |

---

## Common Compiler Errors After Upgrading

Use this section to match build errors to the appropriate migration step above.

### CS0234 / CS0246 — Type or namespace not found

```
error CS0246: The type or namespace name 'IAsyncParseNodeFactory' could not be found
```

**Fix:** Replace `IAsyncParseNodeFactory` with `IParseNodeFactory`. See [IParseNodeFactory Interface Consolidation](#iparsenodeFactory-interface-consolidation).

---

### CS0535 — Interface member not implemented

```
error CS0535: 'MyFactory' does not implement interface member 'IParseNodeFactory.GetRootParseNodeAsync(string, Stream, CancellationToken)'
```

**Fix:** Add a `GetRootParseNodeAsync` method to your class. If you previously had `GetRootParseNode`, rename it, change the return type to `Task<IParseNode>`, and add a `CancellationToken` parameter.

---

### CS0117 — Type does not contain a definition

```
error CS0117: 'KiotaSerializer' does not contain a definition for 'Deserialize'
error CS0117: 'KiotaSerializer' does not contain a definition for 'DeserializeCollection'
error CS0117: 'KiotaJsonSerializer' does not contain a definition for 'Deserialize'
error CS0117: 'KiotaJsonSerializer' does not contain a definition for 'DeserializeCollection'
```

**Fix:** Replace with the async equivalents (`DeserializeAsync`, `DeserializeCollectionAsync`). Ensure the calling method is `async`. See [KiotaSerializer Deserialization Methods](#kiotaserializer-deserialization-methods).

---

### CS0117 — MultipartBody method not found

```
error CS0117: 'MultipartBody' does not contain a definition for 'GetPartValue' (single parameter)
error CS0117: 'MultipartBody' does not contain a definition for 'RemovePart' (single parameter)
```

**Fix:** Add the `fileName` parameter (or pass `null`). See [MultipartBody Method Changes](#multipartbody-method-changes).

---

### CS1501 — No overload takes 1 argument

```
error CS1501: No overload for method 'GetPartValue' takes 1 arguments
error CS1501: No overload for method 'RemovePart' takes 1 arguments
```

**Fix:** Same as above — use the two-parameter overload: `.GetPartValue<T>(partName, fileName)` or `.RemovePart(partName, fileName)`.

---

### CS0311 / CS0246 — ParseNodeFactoryRegistry.GetFactory generic

```
error CS0311: The type 'T' cannot be used as type parameter in the generic method 'GetFactory<T>'
error CS0246: The type or namespace name 'GetFactory<T>' could not be found
```

**Fix:** Replace `registry.GetFactory<IParseNodeFactory>(contentType)` with `registry.GetFactory(contentType)`. The method now returns a `(IParseNodeFactory Factory, string ContentType)` tuple. See [ParseNodeFactoryRegistry Changes](#parsenodeFactoryregistry-changes).

---

### NETSDK1045 / NETSDK1005 — Target framework not supported

```
error NETSDK1045: The current .NET SDK does not support targeting .NET 5.0 or .NET 6.0
```

**Fix:** Update your project's `<TargetFramework>` to `net8.0` or later. See [Target Framework Changes](#target-framework-changes).

