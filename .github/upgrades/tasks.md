# Concordia .NET 10.0 Upgrade Tasks

## Overview

This document tracks the execution of the Concordia solution upgrade from .NET 9/.NET 8/.NET Standard 2.0 to include .NET 10.0 support. The upgrade will proceed tier-by-tier following the dependency hierarchy.

**Progress**: 5/8 tasks complete (62%) ![0%](https://progress-bar.xyz/62)

---

## Tasks

### [✓] TASK-001: Verify prerequisites *(Completed: 2025-12-30 00:03)*
**References**: Plan §Executive Summary

- [✓] (1) Verify .NET 10 SDK installed and accessible via command line
- [✓] (2) .NET 10 SDK version meets minimum requirements (**Verify**)
- [✓] (3) Verify NuGet package sources are accessible
- [✓] (4) Package sources respond successfully (**Verify**)

---

### [✓] TASK-002: Upgrade Tier 1 (Synaptrix.Core) *(Completed: 2025-12-30 00:04)*
**References**: Plan §Project-by-Project Plans §Synaptrix.Core, Plan §Package Update Reference

- [✓] (1) Update TargetFrameworks in Synaptrix.Core\Synaptrix.Core.csproj to append net10.0 per Plan §Synaptrix.Core (from `net9.0;net8.0;netstandard2.0` to `net9.0;net8.0;netstandard2.0;net10.0`)
- [✓] (2) All TargetFrameworks updated correctly (**Verify**)
- [✓] (3) Update package references per Plan §Package Update Reference: Microsoft.Extensions.DependencyInjection.Abstractions 9.0.8 → 10.0.1, Microsoft.Extensions.Logging.Abstractions 9.0.8 → 10.0.1
- [✓] (4) All package references updated to version 10.0.1 (**Verify**)
- [✓] (5) Restore dependencies for Synaptrix.Core project
- [✓] (6) Dependencies restored successfully (**Verify**)
- [✓] (7) Build Synaptrix.Core project for all target frameworks
- [✓] (8) Project builds with 0 errors for all frameworks (net9.0, net8.0, netstandard2.0, net10.0) (**Verify**)
- [✓] (9) Commit changes with message: "TASK-002: Upgrade Tier 1 (Synaptrix.Core) to include .NET 10.0"

---

### [✓] TASK-003: Test Tier 1 (Synaptrix.Core) *(Completed: 2025-12-30 00:02)*
**References**: Plan §Testing & Validation Strategy

- [✓] (1) Build solution to verify dependent projects can reference updated Core
- [✓] (2) Synaptrix.MediatR and Synaptrix.Generator build successfully with updated Core reference (**Verify**)
- [✓] (3) Run tests in Synaptrix.Core.Tests that validate Core functionality
- [✓] (4) All Core-related tests pass with 0 failures (**Verify**)
- [✓] (5) Commit changes with message: "TASK-003: Validate Tier 1 (Synaptrix.Core)"

---

### [✓] TASK-004: Upgrade Tier 2 (Synaptrix.MediatR) *(Completed: 2025-12-29 23:13)*
**References**: Plan §Project-by-Project Plans §Synaptrix.MediatR, Plan §Package Update Reference

- [✓] (1) Update TargetFrameworks in Synaptrix.MediatR\Synaptrix.MediatR.csproj to append net10.0 per Plan §Synaptrix.MediatR (from `net9.0;net8.0;netstandard2.0` to `net9.0;net8.0;netstandard2.0;net10.0`)
- [✓] (2) All TargetFrameworks updated correctly (**Verify**)
- [✓] (3) Update package reference per Plan §Package Update Reference: Microsoft.Extensions.DependencyInjection.Abstractions 9.0.8 → 10.0.1
- [✓] (4) Package reference updated to version 10.0.1 (**Verify**)
- [✓] (5) Restore dependencies for Synaptrix.MediatR project
- [✓] (6) Dependencies restored successfully (**Verify**)
- [✓] (7) Build Synaptrix.MediatR project for all target frameworks
- [✓] (8) Project builds with 0 errors for all frameworks (net9.0, net8.0, netstandard2.0, net10.0) (**Verify**)
- [✓] (9) Commit changes with message: "TASK-004: Upgrade Tier 2 (Synaptrix.MediatR) to include .NET 10.0"

---

### [✓] TASK-005: Test Tier 2 (Synaptrix.MediatR) *(Completed: 2025-12-30 00:13)*
**References**: Plan §Testing & Validation Strategy

- [✓] (1) Build solution to verify dependent projects can reference updated MediatR
- [✓] (2) Synaptrix.Examples.Web and Synaptrix.Core.Tests build successfully with updated MediatR reference (**Verify**)
- [✓] (3) Run tests in Synaptrix.Core.Tests that validate MediatR functionality
- [✓] (4) All MediatR-related tests pass with 0 failures (**Verify**)
- [✓] (5) Commit changes with message: "TASK-005: Validate Tier 2 (Synaptrix.MediatR)"

---

### [▶] TASK-006: Upgrade Tier 3 (Synaptrix.Examples.Web and Synaptrix.Core.Tests)
**References**: Plan §Project-by-Project Plans §Synaptrix.Examples.Web, Plan §Project-by-Project Plans §Synaptrix.Core.Tests, Plan §Package Update Reference

- [ ] (1) Update TargetFramework in examples\Synaptrix.Examples.Web\Synaptrix.Examples.Web.csproj per Plan §Synaptrix.Examples.Web (from `net9.0` to `net10.0`)
- [ ] (2) Examples.Web TargetFramework updated to net10.0 (**Verify**)
- [ ] (3) Update package reference in Examples.Web per Plan §Package Update Reference: Microsoft.AspNetCore.OpenApi 9.0.8 → 10.0.1
- [ ] (4) Examples.Web package reference updated to version 10.0.1 (**Verify**)
- [ ] (5) Update TargetFramework in tests\Synaptrix.Core.Tests\Synaptrix.Core.Tests.csproj per Plan §Synaptrix.Core.Tests (from `net9.0` to `net10.0`)
- [ ] (6) Core.Tests TargetFramework updated to net10.0 (**Verify**)
- [ ] (7) Update package reference in Core.Tests per Plan §Package Update Reference: Microsoft.Extensions.DependencyInjection 9.0.8 → 10.0.1
- [ ] (8) Core.Tests package reference updated to version 10.0.1 (**Verify**)
- [ ] (9) Restore dependencies for both projects
- [ ] (10) Dependencies restored successfully for both projects (**Verify**)
- [ ] (11) Build Synaptrix.Examples.Web project
- [ ] (12) Examples.Web builds with 0 errors (**Verify**)
- [ ] (13) Build Synaptrix.Core.Tests project
- [ ] (14) Core.Tests builds with 0 errors (**Verify**)
- [ ] (15) Commit changes with message: "TASK-006: Upgrade Tier 3 (Examples.Web and Core.Tests) to .NET 10.0"

---

### [ ] TASK-007: Test Tier 3 and validate complete solution
**References**: Plan §Testing & Validation Strategy, Plan §Breaking Changes Catalog

- [ ] (1) Run full test suite in Synaptrix.Core.Tests project
- [ ] (2) Fix any test failures referencing Plan §Breaking Changes Catalog for System.Uri behavioral changes if encountered
- [ ] (3) Re-run tests after any fixes
- [ ] (4) All tests pass with 0 failures (**Verify**)
- [ ] (5) Build entire solution to verify all projects integrate correctly
- [ ] (6) Solution builds with 0 errors (**Verify**)
- [ ] (7) Commit changes with message: "TASK-007: Complete testing and validation of .NET 10.0 upgrade"

---

### [ ] TASK-008: Final validation
**References**: Plan §Success Criteria

- [ ] (1) Verify all projects target or include net10.0: Core (multi-target includes net10.0), MediatR (multi-target includes net10.0), Examples.Web (net10.0), Core.Tests (net10.0), Generator (netstandard2.0 - unchanged)
- [ ] (2) All target frameworks verified correct (**Verify**)
- [ ] (3) Verify all 4 package updates completed: Microsoft.Extensions.DependencyInjection.Abstractions (10.0.1), Microsoft.Extensions.Logging.Abstractions (10.0.1), Microsoft.AspNetCore.OpenApi (10.0.1), Microsoft.Extensions.DependencyInjection (10.0.1)
- [ ] (4) All package versions verified at 10.0.1 (**Verify**)
- [ ] (5) Run `dotnet list package` to check for any package conflicts or warnings
- [ ] (6) No package conflicts or outdated packages reported (**Verify**)

---



