# Projects and dependencies analysis

This document provides a comprehensive overview of the projects and their dependencies in the context of upgrading to .NETCoreApp,Version=v10.0.

## Table of Contents

- [Executive Summary](#executive-Summary)
  - [Highlevel Metrics](#highlevel-metrics)
  - [Projects Compatibility](#projects-compatibility)
  - [Package Compatibility](#package-compatibility)
  - [API Compatibility](#api-compatibility)
- [Aggregate NuGet packages details](#aggregate-nuget-packages-details)
- [Top API Migration Challenges](#top-api-migration-challenges)
  - [Technologies and Features](#technologies-and-features)
  - [Most Frequent API Issues](#most-frequent-api-issues)
- [Projects Relationship Graph](#projects-relationship-graph)
- [Project Details](#project-details)

  - [Synaptrix.Core\Synaptrix.Core.csproj](#concordiacoreconcordiacorecsproj)
  - [Synaptrix.Generator\Synaptrix.Generator.csproj](#concordiageneratorconcordiageneratorcsproj)
  - [Synaptrix.MediatR\Synaptrix.MediatR.csproj](#concordiamediatrconcordiamediatrcsproj)
  - [examples\Synaptrix.Examples.Web\Synaptrix.Examples.Web.csproj](#examplesconcordiaexampleswebconcordiaexampleswebcsproj)
  - [tests\Synaptrix.Core.Tests\Synaptrix.Core.Tests.csproj](#testsconcordiacoretestsconcordiacoretestscsproj)


## Executive Summary

### Highlevel Metrics

| Metric | Count | Status |
| :--- | :---: | :--- |
| Total Projects | 5 | 4 require upgrade |
| Total NuGet Packages | 12 | 4 need upgrade |
| Total Code Files | 30 |  |
| Total Code Files with Incidents | 5 |  |
| Total Lines of Code | 3773 |  |
| Total Number of Issues | 12 |  |
| Estimated LOC to modify | 3+ | at least 0,1% of codebase |

### Projects Compatibility

| Project | Target Framework | Difficulty | Package Issues | API Issues | Est. LOC Impact | Description |
| :--- | :---: | :---: | :---: | :---: | :---: | :--- |
| [Synaptrix.Core\Synaptrix.Core.csproj](#concordiacoreconcordiacorecsproj) | net9.0;net8.0;netstandard2.0 | 🟢 Low | 2 | 0 |  | ClassLibrary, Sdk Style = True |
| [Synaptrix.Generator\Synaptrix.Generator.csproj](#concordiageneratorconcordiageneratorcsproj) | netstandard2.0 | ✅ None | 0 | 0 |  | ClassLibrary, Sdk Style = True |
| [Synaptrix.MediatR\Synaptrix.MediatR.csproj](#concordiamediatrconcordiamediatrcsproj) | net9.0;net8.0;netstandard2.0 | 🟢 Low | 1 | 0 |  | ClassLibrary, Sdk Style = True |
| [examples\Synaptrix.Examples.Web\Synaptrix.Examples.Web.csproj](#examplesconcordiaexampleswebconcordiaexampleswebcsproj) | net9.0 | 🟢 Low | 1 | 3 | 3+ | AspNetCore, Sdk Style = True |
| [tests\Synaptrix.Core.Tests\Synaptrix.Core.Tests.csproj](#testsconcordiacoretestsconcordiacoretestscsproj) | net9.0 | 🟢 Low | 1 | 0 |  | DotNetCoreApp, Sdk Style = True |

### Package Compatibility

| Status | Count | Percentage |
| :--- | :---: | :---: |
| ✅ Compatible | 8 | 66,7% |
| ⚠️ Incompatible | 0 | 0,0% |
| 🔄 Upgrade Recommended | 4 | 33,3% |
| ***Total NuGet Packages*** | ***12*** | ***100%*** |

### API Compatibility

| Category | Count | Impact |
| :--- | :---: | :--- |
| 🔴 Binary Incompatible | 0 | High - Require code changes |
| 🟡 Source Incompatible | 0 | Medium - Needs re-compilation and potential conflicting API error fixing |
| 🔵 Behavioral change | 3 | Low - Behavioral changes that may require testing at runtime |
| ✅ Compatible | 2715 |  |
| ***Total APIs Analyzed*** | ***2718*** |  |

## Aggregate NuGet packages details

| Package | Current Version | Suggested Version | Projects | Description |
| :--- | :---: | :---: | :--- | :--- |
| coverlet.collector | 6.0.4 |  | [Synaptrix.Core.Tests.csproj](#testsconcordiacoretestsconcordiacoretestscsproj) | ✅Compatible |
| Microsoft.AspNetCore.OpenApi | 9.0.8 | 10.0.1 | [Synaptrix.Examples.Web.csproj](#examplesconcordiaexampleswebconcordiaexampleswebcsproj) | È consigliabile eseguire l'aggiornamento del pacchetto NuGet |
| Microsoft.CodeAnalysis.Analyzers | 3.3.4 |  | [Synaptrix.Generator.csproj](#concordiageneratorconcordiageneratorcsproj) | ✅Compatible |
| Microsoft.CodeAnalysis.CSharp | 4.9.2 |  | [Synaptrix.Generator.csproj](#concordiageneratorconcordiageneratorcsproj) | ✅Compatible |
| Microsoft.Extensions.DependencyInjection | 9.0.8 | 10.0.1 | [Synaptrix.Core.Tests.csproj](#testsconcordiacoretestsconcordiacoretestscsproj) | È consigliabile eseguire l'aggiornamento del pacchetto NuGet |
| Microsoft.Extensions.DependencyInjection.Abstractions | 9.0.8 | 10.0.1 | [Synaptrix.Core.csproj](#concordiacoreconcordiacorecsproj)<br/>[Synaptrix.MediatR.csproj](#concordiamediatrconcordiamediatrcsproj) | È consigliabile eseguire l'aggiornamento del pacchetto NuGet |
| Microsoft.Extensions.Logging.Abstractions | 9.0.8 | 10.0.1 | [Synaptrix.Core.csproj](#concordiacoreconcordiacorecsproj) | È consigliabile eseguire l'aggiornamento del pacchetto NuGet |
| Microsoft.NET.Test.Sdk | 17.14.1 |  | [Synaptrix.Core.Tests.csproj](#testsconcordiacoretestsconcordiacoretestscsproj) | ✅Compatible |
| NETStandard.Library | 2.0.3 |  | [Synaptrix.Generator.csproj](#concordiageneratorconcordiageneratorcsproj) | ✅Compatible |
| Swashbuckle.AspNetCore | 9.0.4 |  | [Synaptrix.Examples.Web.csproj](#examplesconcordiaexampleswebconcordiaexampleswebcsproj) | ✅Compatible |
| xunit | 2.9.3 |  | [Synaptrix.Core.Tests.csproj](#testsconcordiacoretestsconcordiacoretestscsproj) | ✅Compatible |
| xunit.runner.visualstudio | 3.1.4 |  | [Synaptrix.Core.Tests.csproj](#testsconcordiacoretestsconcordiacoretestscsproj) | ✅Compatible |

## Top API Migration Challenges

### Technologies and Features

| Technology | Issues | Percentage | Migration Path |
| :--- | :---: | :---: | :--- |

### Most Frequent API Issues

| API | Count | Percentage | Category |
| :--- | :---: | :---: | :--- |
| T:System.Uri | 2 | 66,7% | Behavioral Change |
| M:System.Uri.#ctor(System.String) | 1 | 33,3% | Behavioral Change |

## Projects Relationship Graph

Legend:
📦 SDK-style project
⚙️ Classic project

```mermaid
flowchart LR
    P1["<b>📦&nbsp;Synaptrix.Core.csproj</b><br/><small>net9.0;net8.0;netstandard2.0</small>"]
    P2["<b>📦&nbsp;Synaptrix.Generator.csproj</b><br/><small>netstandard2.0</small>"]
    P3["<b>📦&nbsp;Synaptrix.MediatR.csproj</b><br/><small>net9.0;net8.0;netstandard2.0</small>"]
    P4["<b>📦&nbsp;Synaptrix.Examples.Web.csproj</b><br/><small>net9.0</small>"]
    P5["<b>📦&nbsp;Synaptrix.Core.Tests.csproj</b><br/><small>net9.0</small>"]
    P2 --> P1
    P3 --> P1
    P4 --> P3
    P4 --> P2
    P5 --> P3
    P5 --> P1
    click P1 "#concordiacoreconcordiacorecsproj"
    click P2 "#concordiageneratorconcordiageneratorcsproj"
    click P3 "#concordiamediatrconcordiamediatrcsproj"
    click P4 "#examplesconcordiaexampleswebconcordiaexampleswebcsproj"
    click P5 "#testsconcordiacoretestsconcordiacoretestscsproj"

```

## Project Details

<a id="concordiacoreconcordiacorecsproj"></a>
### Synaptrix.Core\Synaptrix.Core.csproj

#### Project Info

- **Current Target Framework:** net9.0;net8.0;netstandard2.0
- **Proposed Target Framework:** net9.0;net8.0;netstandard2.0;net10.0
- **SDK-style**: True
- **Project Kind:** ClassLibrary
- **Dependencies**: 0
- **Dependants**: 3
- **Number of Files**: 21
- **Number of Files with Incidents**: 1
- **Lines of Code**: 959
- **Estimated LOC to modify**: 0+ (at least 0,0% of the project)

#### Dependency Graph

Legend:
📦 SDK-style project
⚙️ Classic project

```mermaid
flowchart TB
    subgraph upstream["Dependants (3)"]
        P2["<b>📦&nbsp;Synaptrix.Generator.csproj</b><br/><small>netstandard2.0</small>"]
        P3["<b>📦&nbsp;Synaptrix.MediatR.csproj</b><br/><small>net9.0;net8.0;netstandard2.0</small>"]
        P5["<b>📦&nbsp;Synaptrix.Core.Tests.csproj</b><br/><small>net9.0</small>"]
        click P2 "#concordiageneratorconcordiageneratorcsproj"
        click P3 "#concordiamediatrconcordiamediatrcsproj"
        click P5 "#testsconcordiacoretestsconcordiacoretestscsproj"
    end
    subgraph current["Synaptrix.Core.csproj"]
        MAIN["<b>📦&nbsp;Synaptrix.Core.csproj</b><br/><small>net9.0;net8.0;netstandard2.0</small>"]
        click MAIN "#concordiacoreconcordiacorecsproj"
    end
    P2 --> MAIN
    P3 --> MAIN
    P5 --> MAIN

```

### API Compatibility

| Category | Count | Impact |
| :--- | :---: | :--- |
| 🔴 Binary Incompatible | 0 | High - Require code changes |
| 🟡 Source Incompatible | 0 | Medium - Needs re-compilation and potential conflicting API error fixing |
| 🔵 Behavioral change | 0 | Low - Behavioral changes that may require testing at runtime |
| ✅ Compatible | 558 |  |
| ***Total APIs Analyzed*** | ***558*** |  |

<a id="concordiageneratorconcordiageneratorcsproj"></a>
### Synaptrix.Generator\Synaptrix.Generator.csproj

#### Project Info

- **Current Target Framework:** netstandard2.0✅
- **SDK-style**: True
- **Project Kind:** ClassLibrary
- **Dependencies**: 1
- **Dependants**: 1
- **Number of Files**: 3
- **Lines of Code**: 231
- **Estimated LOC to modify**: 0+ (at least 0,0% of the project)

#### Dependency Graph

Legend:
📦 SDK-style project
⚙️ Classic project

```mermaid
flowchart TB
    subgraph upstream["Dependants (1)"]
        P4["<b>📦&nbsp;Synaptrix.Examples.Web.csproj</b><br/><small>net9.0</small>"]
        click P4 "#examplesconcordiaexampleswebconcordiaexampleswebcsproj"
    end
    subgraph current["Synaptrix.Generator.csproj"]
        MAIN["<b>📦&nbsp;Synaptrix.Generator.csproj</b><br/><small>netstandard2.0</small>"]
        click MAIN "#concordiageneratorconcordiageneratorcsproj"
    end
    subgraph downstream["Dependencies (1"]
        P1["<b>📦&nbsp;Synaptrix.Core.csproj</b><br/><small>net9.0;net8.0;netstandard2.0</small>"]
        click P1 "#concordiacoreconcordiacorecsproj"
    end
    P4 --> MAIN
    MAIN --> P1

```

### API Compatibility

| Category | Count | Impact |
| :--- | :---: | :--- |
| 🔴 Binary Incompatible | 0 | High - Require code changes |
| 🟡 Source Incompatible | 0 | Medium - Needs re-compilation and potential conflicting API error fixing |
| 🔵 Behavioral change | 0 | Low - Behavioral changes that may require testing at runtime |
| ✅ Compatible | 254 |  |
| ***Total APIs Analyzed*** | ***254*** |  |

<a id="concordiamediatrconcordiamediatrcsproj"></a>
### Synaptrix.MediatR\Synaptrix.MediatR.csproj

#### Project Info

- **Current Target Framework:** net9.0;net8.0;netstandard2.0
- **Proposed Target Framework:** net9.0;net8.0;netstandard2.0;net10.0
- **SDK-style**: True
- **Project Kind:** ClassLibrary
- **Dependencies**: 1
- **Dependants**: 2
- **Number of Files**: 2
- **Number of Files with Incidents**: 1
- **Lines of Code**: 896
- **Estimated LOC to modify**: 0+ (at least 0,0% of the project)

#### Dependency Graph

Legend:
📦 SDK-style project
⚙️ Classic project

```mermaid
flowchart TB
    subgraph upstream["Dependants (2)"]
        P4["<b>📦&nbsp;Synaptrix.Examples.Web.csproj</b><br/><small>net9.0</small>"]
        P5["<b>📦&nbsp;Synaptrix.Core.Tests.csproj</b><br/><small>net9.0</small>"]
        click P4 "#examplesconcordiaexampleswebconcordiaexampleswebcsproj"
        click P5 "#testsconcordiacoretestsconcordiacoretestscsproj"
    end
    subgraph current["Synaptrix.MediatR.csproj"]
        MAIN["<b>📦&nbsp;Synaptrix.MediatR.csproj</b><br/><small>net9.0;net8.0;netstandard2.0</small>"]
        click MAIN "#concordiamediatrconcordiamediatrcsproj"
    end
    subgraph downstream["Dependencies (1"]
        P1["<b>📦&nbsp;Synaptrix.Core.csproj</b><br/><small>net9.0;net8.0;netstandard2.0</small>"]
        click P1 "#concordiacoreconcordiacorecsproj"
    end
    P4 --> MAIN
    P5 --> MAIN
    MAIN --> P1

```

### API Compatibility

| Category | Count | Impact |
| :--- | :---: | :--- |
| 🔴 Binary Incompatible | 0 | High - Require code changes |
| 🟡 Source Incompatible | 0 | Medium - Needs re-compilation and potential conflicting API error fixing |
| 🔵 Behavioral change | 0 | Low - Behavioral changes that may require testing at runtime |
| ✅ Compatible | 777 |  |
| ***Total APIs Analyzed*** | ***777*** |  |

<a id="examplesconcordiaexampleswebconcordiaexampleswebcsproj"></a>
### examples\Synaptrix.Examples.Web\Synaptrix.Examples.Web.csproj

#### Project Info

- **Current Target Framework:** net9.0
- **Proposed Target Framework:** net10.0
- **SDK-style**: True
- **Project Kind:** AspNetCore
- **Dependencies**: 2
- **Dependants**: 0
- **Number of Files**: 3
- **Number of Files with Incidents**: 2
- **Lines of Code**: 314
- **Estimated LOC to modify**: 3+ (at least 1,0% of the project)

#### Dependency Graph

Legend:
📦 SDK-style project
⚙️ Classic project

```mermaid
flowchart TB
    subgraph current["Synaptrix.Examples.Web.csproj"]
        MAIN["<b>📦&nbsp;Synaptrix.Examples.Web.csproj</b><br/><small>net9.0</small>"]
        click MAIN "#examplesconcordiaexampleswebconcordiaexampleswebcsproj"
    end
    subgraph downstream["Dependencies (2"]
        P3["<b>📦&nbsp;Synaptrix.MediatR.csproj</b><br/><small>net9.0;net8.0;netstandard2.0</small>"]
        P2["<b>📦&nbsp;Synaptrix.Generator.csproj</b><br/><small>netstandard2.0</small>"]
        click P3 "#concordiamediatrconcordiamediatrcsproj"
        click P2 "#concordiageneratorconcordiageneratorcsproj"
    end
    MAIN --> P3
    MAIN --> P2

```

### API Compatibility

| Category | Count | Impact |
| :--- | :---: | :--- |
| 🔴 Binary Incompatible | 0 | High - Require code changes |
| 🟡 Source Incompatible | 0 | Medium - Needs re-compilation and potential conflicting API error fixing |
| 🔵 Behavioral change | 3 | Low - Behavioral changes that may require testing at runtime |
| ✅ Compatible | 254 |  |
| ***Total APIs Analyzed*** | ***257*** |  |

<a id="testsconcordiacoretestsconcordiacoretestscsproj"></a>
### tests\Synaptrix.Core.Tests\Synaptrix.Core.Tests.csproj

#### Project Info

- **Current Target Framework:** net9.0
- **Proposed Target Framework:** net10.0
- **SDK-style**: True
- **Project Kind:** DotNetCoreApp
- **Dependencies**: 2
- **Dependants**: 0
- **Number of Files**: 5
- **Number of Files with Incidents**: 1
- **Lines of Code**: 1373
- **Estimated LOC to modify**: 0+ (at least 0,0% of the project)

#### Dependency Graph

Legend:
📦 SDK-style project
⚙️ Classic project

```mermaid
flowchart TB
    subgraph current["Synaptrix.Core.Tests.csproj"]
        MAIN["<b>📦&nbsp;Synaptrix.Core.Tests.csproj</b><br/><small>net9.0</small>"]
        click MAIN "#testsconcordiacoretestsconcordiacoretestscsproj"
    end
    subgraph downstream["Dependencies (2"]
        P3["<b>📦&nbsp;Synaptrix.MediatR.csproj</b><br/><small>net9.0;net8.0;netstandard2.0</small>"]
        P1["<b>📦&nbsp;Synaptrix.Core.csproj</b><br/><small>net9.0;net8.0;netstandard2.0</small>"]
        click P3 "#concordiamediatrconcordiamediatrcsproj"
        click P1 "#concordiacoreconcordiacorecsproj"
    end
    MAIN --> P3
    MAIN --> P1

```

### API Compatibility

| Category | Count | Impact |
| :--- | :---: | :--- |
| 🔴 Binary Incompatible | 0 | High - Require code changes |
| 🟡 Source Incompatible | 0 | Medium - Needs re-compilation and potential conflicting API error fixing |
| 🔵 Behavioral change | 0 | Low - Behavioral changes that may require testing at runtime |
| ✅ Compatible | 872 |  |
| ***Total APIs Analyzed*** | ***872*** |  |

