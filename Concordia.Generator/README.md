# ⚠️ Concordia.Generator has been renamed to Synaptrix.Generator

**This package is deprecated.** Starting from version **3.0.0**, Concordia has been renamed to **Synaptrix**. This is the final release of the `Concordia.Generator` package.

## What should I do?

Replace `Concordia.Core` + `Concordia.Generator` with the `Synaptrix` meta-package:

```bash
dotnet remove package Concordia.Core
dotnet remove package Concordia.Generator
dotnet add package Synaptrix
```

The `Synaptrix` meta-package installs both `Synaptrix.Core` and `Synaptrix.Generator` in one step.

## What changed?

| Before (Concordia v2.x) | After (Synaptrix v3.0.0+) |
|---|---|
| `Concordia` namespace | `Synaptrix` namespace |
| `Concordia.Core` package | `Synaptrix.Core` package |
| `Concordia.Generator` package | `Synaptrix.Generator` package |
| `AddConcordiaHandlers()` | `AddSynaptrixHandlers()` |
| `[DiscoverConcordiaHandlers]` | `[DiscoverSynaptrixHandlers]` |
| `<ConcordiaGeneratedMethodName>` | `<SynaptrixGeneratedMethodName>` |
| `Concordia.MediatR` package | **Discontinued** |
| — | `Synaptrix` meta-package (new, recommended) |

Update your `using` directives and DI registration:

```csharp
// Before
using Concordia;
builder.Services.AddConcordiaHandlers();

// After
using Synaptrix;
builder.Services.AddSynaptrixHandlers();
```

## What's new in Synaptrix v3.0.0?

- **ValueTask everywhere**: All handler, behavior, publisher, and processor return types changed from `Task`/`Task<T>` to `ValueTask`/`ValueTask<T>`, reducing allocations on synchronous hot paths.
- **IAsyncEnumerable streaming**: New `IStreamRequest<TResponse>` / `IStreamRequestHandler<TRequest, TResponse>` interfaces with `CreateStream<TResponse>()` on `ISender` for efficient streaming of multiple results.
- **Improved benchmarks**: SynaptrixGen matches or beats Martin on command and publish benchmarks.

For full documentation, visit the [Synaptrix GitHub repository](https://github.com/mrdevrobot/Synaptrix).

