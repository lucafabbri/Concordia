# ⚠️ Concordia.MediatR has been discontinued

**This package is deprecated and will not receive further updates.** Starting from version **3.0.0**, Concordia has been renamed to **Synaptrix**, and the MediatR compatibility layer has been removed.

## Why was this package discontinued?

Synaptrix v3.0.0 focuses on compile-time source generation via `Synaptrix.Generator`, which provides zero-allocation, sub-2 ns dispatch. The runtime reflection-based approach offered by `Concordia.MediatR` is no longer maintained. If you need a MediatR-style `AddMediator()` call with assembly scanning, consider using MediatR directly alongside Synaptrix.

## What should I do?

Remove both `Concordia.Core` and `Concordia.MediatR`, then install the `Synaptrix` meta-package which includes `Synaptrix.Core` and `Synaptrix.Generator`:

```bash
dotnet remove package Concordia.Core
dotnet remove package Concordia.MediatR
dotnet add package Synaptrix
```

## Migration guide

| Before (Concordia + Concordia.MediatR) | After (Synaptrix v3.0.0+) |
|---|---|
| `using Concordia;` | `using Synaptrix;` |
| `using Concordia.MediatR;` | *(removed — no equivalent)* |
| `builder.Services.AddMediator(cfg => ...)` | `builder.Services.AddSynaptrixHandlers()` |
| Runtime reflection-based discovery | Compile-time source generation |
| `Concordia.Core` + `Concordia.MediatR` packages | `Synaptrix` meta-package |

Update your `Program.cs`:

```csharp
// Before
using Concordia;
using Concordia.MediatR;

builder.Services.AddMediator(cfg =>
{
    cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
});

// After
using Synaptrix;

builder.Services.AddSynaptrixHandlers();
```

All interfaces (`IRequest`, `IRequestHandler`, `INotification`, `INotificationHandler`, `IPipelineBehavior`, etc.) remain the same — just under the `Synaptrix` namespace instead of `Concordia`.

## What's new in Synaptrix v3.0.0?

- **ValueTask everywhere**: All handler, behavior, publisher, and processor return types changed from `Task`/`Task<T>` to `ValueTask`/`ValueTask<T>`, reducing allocations on synchronous hot paths.
- **IAsyncEnumerable streaming**: New `IStreamRequest<TResponse>` / `IStreamRequestHandler<TRequest, TResponse>` interfaces with `CreateStream<TResponse>()` on `ISender` for efficient streaming of multiple results.
- **Compile-time only**: The source generator produces a sealed `GeneratedMediator` with constructor-injected handlers and direct type-switch dispatch — no runtime reflection, no DI lookups on the hot path.

For full documentation, visit the [Synaptrix GitHub repository](https://github.com/mrdevrobot/Synaptrix).
