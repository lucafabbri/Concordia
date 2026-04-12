---
layout: default
title: Installation
---

# Installation & Setup

Concordia is modular by design. Install the packages that match your project's needs.

## Package Ecosystem

| Package | Description |
| :--- | :--- |
| **`Synaptrix.Core`** | **Required**. Contains the core interfaces (`IMediator`, `IRequest`, `INotification`) and the default `Mediator` implementation. No dependencies on external logic. |
| **`Synaptrix.Generator`** | **Recommended**. The C# Source Generator that analyzes your code during compilation to generate handler registrations. |

---

## Setup (Source Generators)
*Recommended for all new projects.*

This approach leverages the Roslyn compiler to inject registration code directly into your assembly. It guarantees zero startup overhead.

### 1. Install Packages
Add the Core library and the Generator to your project using the .NET CLI:

```bash
dotnet add package Synaptrix.Core --version 2.3.0
dotnet add package Synaptrix.Generator --version 2.3.0
```

### 2. Verify csproj Configuration
Ensure that the `Synaptrix.Generator` is properly referenced (usually handled automatically by NuGet, but good to verify):

```xml
<ItemGroup>
    <PackageReference Include="Concordia" Version="2.3.0" />
    <PackageReference Include="Synaptrix.Generator" Version="2.3.0" PrivateAssets="all" />
</ItemGroup>
```

### 3. Register Services
The generator creates an extension method based on your project's content. By default, it follows naming conventions, but you can look for it in your startup code.

```csharp
using Concordia;
using Synaptrix.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// 1. Add Core Services (IMediator, ISender, IPublisher)
builder.Services.AddConcordiaCoreServices();

// 2. Add Generated Handlers
// The name 'AddMyProjectHandlers' is an example. 
// You can customize this via MSBuild properties if needed.
builder.Services.AddConcordiaHandlers(); 
```

> [!TIP]
> **Customizing the Generated Method Name:**
> You can control the name of the generated extension method by adding a property to your `.csproj` file:
> ```xml
> <PropertyGroup>
>    <ConcordiaGeneratedMethodName>AddMyCustomHandlers</ConcordiaGeneratedMethodName>
> </PropertyGroup>
> ```


