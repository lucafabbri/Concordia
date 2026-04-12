# .NET 10.0 Upgrade Plan for Concordia Solution

## Table of Contents

- [Executive Summary](#executive-summary)
- [Migration Strategy](#migration-strategy)
- [Detailed Dependency Analysis](#detailed-dependency-analysis)
- [Project-by-Project Plans](#project-by-project-plans)
- [Package Update Reference](#package-update-reference)
- [Breaking Changes Catalog](#breaking-changes-catalog)
- [Risk Management](#risk-management)
- [Testing & Validation Strategy](#testing--validation-strategy)
- [Complexity & Effort Assessment](#complexity--effort-assessment)
- [Source Control Strategy](#source-control-strategy)
- [Success Criteria](#success-criteria)

---

## Executive Summary

### Scenario Overview

**Objective**: Upgrade the Concordia solution from .NET 9/.NET 8/.NET Standard 2.0 to include .NET 10.0 support while maintaining backward compatibility for existing targets.

**Scope**: 
- **Total Projects**: 5 (4 require upgrade, 1 source generator remains netstandard2.0)
- **Current State**: Multi-targeted class libraries (net9.0;net8.0;netstandard2.0), single-targeted applications/tests (net9.0)
- **Target State**: Add net10.0 to multi-target libraries, update applications/tests to net10.0

### Solution Complexity Assessment

**Discovered Metrics**:
- Total projects: 5
- Dependency depth: 2 levels (Core ? MediatR/Generator ? Examples/Tests)
- Total NuGet packages: 12 (4 need updates)
- Total LOC: 3,773
- Estimated LOC to modify: 3+ (0.1% of codebase)
- Security vulnerabilities: 0
- Breaking changes: 3 behavioral changes in System.Uri (Examples.Web only)

**Complexity Classification: ?? Simple**

**Justification**:
- ? Small project count (5 projects)
- ? Shallow dependency structure (2 levels)
- ? No security vulnerabilities
- ? No binary/source incompatibilities
- ? Minimal behavioral changes (isolated to one project)
- ? All SDK-style projects
- ? All packages have clear upgrade paths

### Selected Strategy

**All-At-Once Strategy** - All projects upgraded simultaneously in single coordinated operation.

**Rationale**:
- Small solution (5 projects) enables atomic upgrade
- All library projects currently on net9.0 (or multi-target including net9.0)
- Clean dependency structure with no circular dependencies
- All NuGet packages have known compatible versions for net10.0
- No blocking issues or complex breaking changes
- Multi-targeting approach minimizes risk (existing targets remain functional)

### Critical Issues

**None** - This is a low-risk upgrade:
- ? No security vulnerabilities identified
- ? No binary or source incompatibilities
- ? Only 3 behavioral changes (System.Uri constructor) in Examples.Web
- ? All package updates are straightforward version bumps

### Iteration Strategy

**Fast Batch Approach** (2-3 iterations):
1. Foundation phase (dependency analysis, strategy details)
2. Batch all project details together (simple solution warrants single iteration)
3. Final phase (success criteria, source control)

### Expected Timeline

All projects will be upgraded simultaneously with validation after completion. The atomic nature of this upgrade enables rapid execution with clear rollback points.

---

## Migration Strategy

### Approach Selection

**Selected Strategy: All-At-Once Strategy**

All projects in the solution will be upgraded simultaneously in a single coordinated operation.

### Rationale for All-At-Once

**Solution Characteristics Supporting This Approach**:

1. **Small Scale** (5 projects)
   - Well within the threshold for atomic updates
   - Easy to coordinate and test as a unit
   - Low coordination overhead

2. **All Projects on Modern .NET** (net9.0 or multi-target including net9.0)
   - No .NET Framework projects requiring complex migration
   - Straightforward version bumps rather than framework transitions
   - Multi-targeting strategy preserves existing compatibility

3. **Homogeneous Codebase**
   - All SDK-style projects
   - Consistent patterns across projects
   - Same package ecosystem (Microsoft.Extensions.*)

4. **Low Dependency Complexity**
   - Clean 2-level hierarchy
   - No circular dependencies
   - Clear dependency flow

5. **Assessment Shows Clear Path**
   - All 4 packages have known net10.0 versions
   - No incompatible packages
   - No binary/source breaking changes
   - Only 3 behavioral changes (isolated, low impact)

### All-At-Once Strategy: Specific Considerations

**Simultaneity Approach**:
- All project TargetFramework properties updated at once
- All package versions updated in single pass
- Unified build and validation step
- No intermediate states

**Advantages for This Solution**:
- Fastest completion time (single upgrade cycle)
- No multi-targeting complexity during migration (only in final state)
- All projects benefit from .NET 10 simultaneously
- Clean dependency resolution
- Single commit captures entire upgrade

**Risk Mitigation**:
- Multi-targeting preserves backward compatibility
- Comprehensive test suite validates behavior
- Behavioral changes isolated to Examples.Web
- Clean rollback point (single commit)

### Dependency-Based Ordering

**Within the atomic operation, respect this sequence**:

1. **Update TargetFramework properties** (all projects)
   - Multi-target libraries: append net10.0
   - Single-target apps/tests: replace net9.0 with net10.0

2. **Update package references** (all projects with package updates)
   - Microsoft.Extensions.* packages: 9.0.8 ? 10.0.1
   - Microsoft.AspNetCore.OpenApi: 9.0.8 ? 10.0.1

3. **Restore and build** (entire solution)
   - Validates all changes work together
   - Identifies any compilation issues

4. **Address breaking changes** (if any discovered during build)
   - Expected: None (assessment shows no binary/source incompatibilities)
   - Possible: Behavioral changes in Examples.Web (System.Uri)

5. **Run tests** (test projects)
   - Validates functionality preserved
   - Catches behavioral changes

### Parallel vs Sequential Execution

**Not Applicable** - All-At-Once strategy performs all updates as single batch operation, not sequentially by project.

### Multi-Targeting Strategy

**For Class Libraries** (Synaptrix.Core, Synaptrix.MediatR):
- **Approach**: Append net10.0 to existing targets
- **Current**: `<TargetFrameworks>net9.0;net8.0;netstandard2.0</TargetFrameworks>`
- **Updated**: `<TargetFrameworks>net9.0;net8.0;netstandard2.0;net10.0</TargetFrameworks>`
- **Rationale**: Maintains compatibility for existing consumers while enabling net10.0 optimizations

**For Applications/Tests** (Examples.Web, Core.Tests):
- **Approach**: Replace net9.0 with net10.0
- **Current**: `<TargetFramework>net9.0</TargetFramework>`
- **Updated**: `<TargetFramework>net10.0</TargetFramework>`
- **Rationale**: Applications should target single framework for simplicity

**For Source Generators** (Synaptrix.Generator):
- **Approach**: No change
- **Current & Target**: `<TargetFramework>netstandard2.0</TargetFramework>`
- **Rationale**: Source generators must remain netstandard2.0 for maximum compatibility with all SDK versions

---

## Detailed Dependency Analysis

### Dependency Graph Summary

The Concordia solution has a clean, two-level dependency structure:

```
Level 0 (Foundation):
  ?? Synaptrix.Core (net9.0;net8.0;netstandard2.0)
     - No project dependencies
     - Depended upon by: Generator, MediatR, Tests

Level 1 (Integration Layer):
  ?? Synaptrix.Generator (netstandard2.0)
  ?  ?? Depends on: Core
  ?  ?? Depended upon by: Examples.Web
  ?
  ?? Synaptrix.MediatR (net9.0;net8.0;netstandard2.0)
     ?? Depends on: Core
     ?? Depended upon by: Examples.Web, Tests

Level 2 (Consumers):
  ?? Synaptrix.Examples.Web (net9.0)
  ?  ?? Depends on: MediatR, Generator
  ?
  ?? Synaptrix.Core.Tests (net9.0)
     ?? Depends on: Core, MediatR
```

### Project Groupings by Migration Phase

**All-At-Once Strategy**: All projects upgraded simultaneously in single atomic operation.

**Atomic Upgrade Group** (All 4 projects):
1. **Synaptrix.Core** - Foundation library (add net10.0 to multi-target)
2. **Synaptrix.MediatR** - Integration library (add net10.0 to multi-target)
3. **Synaptrix.Examples.Web** - ASP.NET Core example (net9.0 ? net10.0)
4. **Synaptrix.Core.Tests** - Test project (net9.0 ? net10.0)

**Unchanged**:
- **Synaptrix.Generator** - Source generator remains netstandard2.0 (compatible with all .NET versions)

### Critical Path Identification

**No critical path constraints** - The dependency structure is straightforward:
- Core library is the foundation (no dependencies)
- MediatR and Generator depend only on Core
- Examples and Tests consume the libraries

Since we're using multi-targeting for libraries (adding net10.0 alongside existing targets), all projects can be updated simultaneously without breaking the build.

### Circular Dependencies

**None detected** - Clean unidirectional dependency flow.

### Multi-Targeting Considerations

**Strategy**: Append net10.0 to existing multi-target frameworks

**Projects with multi-targeting**:
- **Synaptrix.Core**: `net9.0;net8.0;netstandard2.0` ? `net9.0;net8.0;netstandard2.0;net10.0`
- **Synaptrix.MediatR**: `net9.0;net8.0;netstandard2.0` ? `net9.0;net8.0;netstandard2.0;net10.0`

**Projects with single target upgrade**:
- **Synaptrix.Examples.Web**: `net9.0` ? `net10.0`
- **Synaptrix.Core.Tests**: `net9.0` ? `net10.0`

**Generator stays unchanged**:
- **Synaptrix.Generator**: `netstandard2.0` (no change - source generators should remain netstandard2.0)

This approach ensures backward compatibility while enabling .NET 10 consumers to use the latest optimizations.

---

## Project-by-Project Plans

### Overview

All 4 projects requiring upgrade will be updated simultaneously in a single atomic operation. Details for each project are provided below for reference.

---

### Project: Synaptrix.Core

**Current State**:
- Target Framework: `net9.0;net8.0;netstandard2.0`
- Project Type: ClassLibrary (SDK-style)
- Dependencies: 0 project dependencies
- Dependants: 3 (Generator, MediatR, Tests)
- NuGet Packages: 2 (both need updates)
- Lines of Code: 959
- Files with Issues: 1

**Target State**:
- Target Framework: `net9.0;net8.0;netstandard2.0;net10.0`
- Updated Packages: 2

**Migration Steps**:

1. **Prerequisites**
   - ? No dependencies (foundation library)
   - ? .NET 10 SDK installed
   - ? Project is SDK-style (no conversion needed)

2. **TargetFramework Update**
   - **File**: `Synaptrix.Core\Synaptrix.Core.csproj`
   - **Change**: Append `net10.0` to existing targets
   - **Before**: `<TargetFrameworks>net9.0;net8.0;netstandard2.0</TargetFrameworks>`
   - **After**: `<TargetFrameworks>net9.0;net8.0;netstandard2.0;net10.0</TargetFrameworks>`

3. **Package Updates**

   | Package | Current Version | Target Version | Reason |
   |---------|----------------|----------------|--------|
   | Microsoft.Extensions.DependencyInjection.Abstractions | 9.0.8 | 10.0.1 | Framework alignment, .NET 10 compatibility |
   | Microsoft.Extensions.Logging.Abstractions | 9.0.8 | 10.0.1 | Framework alignment, .NET 10 compatibility |

   **Update method**: Modify PackageReference elements in .csproj file

4. **Expected Breaking Changes**
   - **None** - Assessment shows 0 API compatibility issues for this project
   - No binary incompatibilities
   - No source incompatibilities
   - No behavioral changes

5. **Code Modifications**
   - **None required** - All APIs compatible
   - No obsolete API usage detected
   - No namespace changes needed
   - No configuration changes

6. **Testing Strategy**
   - **Build verification**: Project builds for all 4 target frameworks (net9.0, net8.0, netstandard2.0, net10.0)
   - **Dependency validation**: Dependant projects (Generator, MediatR, Tests) can reference updated Core
   - **Unit tests**: Validated through Synaptrix.Core.Tests project

7. **Validation Checklist**
   - [ ] Project builds without errors for all target frameworks
   - [ ] Project builds without warnings
   - [ ] No package dependency conflicts
   - [ ] Dependant projects can build successfully
   - [ ] Unit tests pass (validated in Synaptrix.Core.Tests)

---

### Project: Synaptrix.MediatR

**Current State**:
- Target Framework: `net9.0;net8.0;netstandard2.0`
- Project Type: ClassLibrary (SDK-style)
- Dependencies: 1 (Core)
- Dependants: 2 (Examples.Web, Tests)
- NuGet Packages: 1 (needs update)
- Lines of Code: 896
- Files with Issues: 1

**Target State**:
- Target Framework: `net9.0;net8.0;netstandard2.0;net10.0`
- Updated Packages: 1

**Migration Steps**:

1. **Prerequisites**
   - ? Depends on Synaptrix.Core (upgraded in same atomic operation)
   - ? .NET 10 SDK installed
   - ? Project is SDK-style (no conversion needed)

2. **TargetFramework Update**
   - **File**: `Synaptrix.MediatR\Synaptrix.MediatR.csproj`
   - **Change**: Append `net10.0` to existing targets
   - **Before**: `<TargetFrameworks>net9.0;net8.0;netstandard2.0</TargetFrameworks>`
   - **After**: `<TargetFrameworks>net9.0;net8.0;netstandard2.0;net10.0</TargetFrameworks>`

3. **Package Updates**

   | Package | Current Version | Target Version | Reason |
   |---------|----------------|----------------|--------|
   | Microsoft.Extensions.DependencyInjection.Abstractions | 9.0.8 | 10.0.1 | Framework alignment, .NET 10 compatibility |

   **Update method**: Modify PackageReference element in .csproj file

4. **Expected Breaking Changes**
   - **None** - Assessment shows 0 API compatibility issues for this project
   - No binary incompatibilities
   - No source incompatibilities
   - No behavioral changes

5. **Code Modifications**
   - **None required** - All APIs compatible
   - No obsolete API usage detected
   - No namespace changes needed
   - No configuration changes

6. **Testing Strategy**
   - **Build verification**: Project builds for all 4 target frameworks
   - **Dependency validation**: References updated Core library, consumed by Examples.Web and Tests
   - **Unit tests**: Validated through Synaptrix.Core.Tests project

7. **Validation Checklist**
   - [ ] Project builds without errors for all target frameworks
   - [ ] Project builds without warnings
   - [ ] No package dependency conflicts
   - [ ] ProjectReference to Core resolves correctly
   - [ ] Dependant projects (Examples.Web, Tests) can build successfully
   - [ ] Unit tests pass (validated in Synaptrix.Core.Tests)

---

### Project: Synaptrix.Examples.Web

**Current State**:
- Target Framework: `net9.0`
- Project Type: AspNetCore (SDK-style)
- Dependencies: 2 (MediatR, Generator)
- Dependants: 0
- NuGet Packages: 1 (needs update)
- Lines of Code: 314
- Files with Issues: 2
- API Issues: 3 behavioral changes (System.Uri)

**Target State**:
- Target Framework: `net10.0`
- Updated Packages: 1

**Migration Steps**:

1. **Prerequisites**
   - ? Depends on Synaptrix.MediatR (upgraded in same atomic operation)
   - ? Depends on Synaptrix.Generator (no changes, netstandard2.0 compatible)
   - ? .NET 10 SDK installed
   - ? Project is SDK-style (no conversion needed)

2. **TargetFramework Update**
   - **File**: `examples\Synaptrix.Examples.Web\Synaptrix.Examples.Web.csproj`
   - **Change**: Replace net9.0 with net10.0
   - **Before**: `<TargetFramework>net9.0</TargetFramework>`
   - **After**: `<TargetFramework>net10.0</TargetFramework>`

3. **Package Updates**

   | Package | Current Version | Target Version | Reason |
   |---------|----------------|----------------|--------|
   | Microsoft.AspNetCore.OpenApi | 9.0.8 | 10.0.1 | Framework alignment, ASP.NET Core 10 compatibility |

   **Additional compatible packages** (no update needed):
   - Swashbuckle.AspNetCore 9.0.4 (compatible with .NET 10)

4. **Expected Breaking Changes**
   
   ?? **System.Uri Behavioral Changes**
   
   | API | Count | Category | Impact |
   |-----|-------|----------|--------|
   | System.Uri (type) | 2 | Behavioral Change | URI parsing/handling behavior may differ |
   | System.Uri constructor | 1 | Behavioral Change | Constructor behavior may differ |

   **Affected Areas**:
   - 2 files with incidents
   - Estimated 3+ lines of code potentially affected
   
   **Mitigation**:
   - Review URI construction and usage patterns
   - Test any URI-dependent functionality
   - Validate API endpoints that use URIs
   - Check for any URI parsing edge cases

5. **Code Modifications**
   
   **Potential changes needed**:
   - Review URI construction in affected files
   - Update tests if URI behavior differs
   - Document any behavioral differences
   
   **Areas to review**:
   - API endpoint URI handling
   - OpenAPI/Swagger URI generation
   - Any custom URI manipulation logic

6. **Testing Strategy**
   - **Build verification**: Project builds successfully for net10.0
   - **Application startup**: Run application and verify startup
   - **OpenAPI/Swagger**: Validate Swagger UI loads correctly
   - **API endpoints**: Test endpoints that use URI handling
   - **Behavioral validation**: Compare URI handling with .NET 9 version if necessary

7. **Validation Checklist**
   - [ ] Project builds without errors
   - [ ] Project builds without warnings
   - [ ] No package dependency conflicts
   - [ ] ProjectReferences resolve correctly (MediatR, Generator)
   - [ ] Application starts without errors
   - [ ] Swagger UI loads and displays API documentation
   - [ ] API endpoints respond correctly
   - [ ] URI-dependent functionality works as expected
   - [ ] No runtime exceptions related to System.Uri

---

### Project: Synaptrix.Core.Tests

**Current State**:
- Target Framework: `net9.0`
- Project Type: DotNetCoreApp (SDK-style, Test Project)
- Dependencies: 2 (Core, MediatR)
- Dependants: 0
- NuGet Packages: 1 (needs update)
- Lines of Code: 1,373
- Files with Issues: 1

**Target State**:
- Target Framework: `net10.0`
- Updated Packages: 1

**Migration Steps**:

1. **Prerequisites**
   - ? Depends on Synaptrix.Core (upgraded in same atomic operation)
   - ? Depends on Synaptrix.MediatR (upgraded in same atomic operation)
   - ? .NET 10 SDK installed
   - ? Project is SDK-style (no conversion needed)

2. **TargetFramework Update**
   - **File**: `tests\Synaptrix.Core.Tests\Synaptrix.Core.Tests.csproj`
   - **Change**: Replace net9.0 with net10.0
   - **Before**: `<TargetFramework>net9.0</TargetFramework>`
   - **After**: `<TargetFramework>net10.0</TargetFramework>`

3. **Package Updates**

   | Package | Current Version | Target Version | Reason |
   |---------|----------------|----------------|--------|
   | Microsoft.Extensions.DependencyInjection | 9.0.8 | 10.0.1 | Framework alignment, .NET 10 compatibility |

   **Additional compatible packages** (no update needed):
   - xunit 2.9.3 (compatible)
   - xunit.runner.visualstudio 3.1.4 (compatible)
   - Microsoft.NET.Test.Sdk 17.14.1 (compatible)
   - coverlet.collector 6.0.4 (compatible)

4. **Expected Breaking Changes**
   - **None** - Assessment shows 0 API compatibility issues for this project
   - No binary incompatibilities
   - No source incompatibilities
   - No behavioral changes

5. **Code Modifications**
   - **None required** - All APIs compatible
   - Test framework compatible with .NET 10
   - No test pattern changes needed

6. **Testing Strategy**
   - **Build verification**: Project builds successfully for net10.0
   - **Test discovery**: All tests discovered by test runner
   - **Test execution**: All tests execute
   - **Test results**: All tests pass

7. **Validation Checklist**
   - [ ] Project builds without errors
   - [ ] Project builds without warnings
   - [ ] No package dependency conflicts
   - [ ] ProjectReferences resolve correctly (Core, MediatR)
   - [ ] All tests discovered
   - [ ] All tests execute without errors
   - [ ] All tests pass
   - [ ] Code coverage collection works (if enabled)

---

## Package Update Reference

### Common Package Updates

**Microsoft.Extensions.DependencyInjection.Abstractions** - Affecting multiple projects

| Current Version | Target Version | Projects Affected | Update Reason |
|----------------|----------------|-------------------|---------------|
| 9.0.8 | 10.0.1 | 2 projects | Framework alignment with .NET 10, compatibility |

**Affected Projects**:
- Synaptrix.Core
- Synaptrix.MediatR

**Breaking Changes**: None reported
**Migration Notes**: Straightforward version bump, abstraction package with stable API

---

### Project-Specific Package Updates

**Synaptrix.Core**

| Package | Current Version | Target Version | Update Reason |
|---------|----------------|----------------|---------------|
| Microsoft.Extensions.DependencyInjection.Abstractions | 9.0.8 | 10.0.1 | Framework alignment |
| Microsoft.Extensions.Logging.Abstractions | 9.0.8 | 10.0.1 | Framework alignment |

**Synaptrix.MediatR**

| Package | Current Version | Target Version | Update Reason |
|---------|----------------|----------------|---------------|
| Microsoft.Extensions.DependencyInjection.Abstractions | 9.0.8 | 10.0.1 | Framework alignment |

**Synaptrix.Examples.Web**

| Package | Current Version | Target Version | Update Reason |
|---------|----------------|----------------|---------------|
| Microsoft.AspNetCore.OpenApi | 9.0.8 | 10.0.1 | Framework alignment, ASP.NET Core 10 compatibility |

**Synaptrix.Core.Tests**

| Package | Current Version | Target Version | Update Reason |
|---------|----------------|----------------|---------------|
| Microsoft.Extensions.DependencyInjection | 9.0.8 | 10.0.1 | Framework alignment |

---

### Compatible Packages (No Update Required)

The following packages are already compatible with .NET 10 and do not require updates:

**Synaptrix.Generator**:
- Microsoft.CodeAnalysis.Analyzers 3.3.4 ?
- Microsoft.CodeAnalysis.CSharp 4.9.2 ?
- NETStandard.Library 2.0.3 ?

**Synaptrix.Examples.Web**:
- Swashbuckle.AspNetCore 9.0.4 ?

**Synaptrix.Core.Tests**:
- xunit 2.9.3 ?
- xunit.runner.visualstudio 3.1.4 ?
- Microsoft.NET.Test.Sdk 17.14.1 ?
- coverlet.collector 6.0.4 ?

---

### Package Update Strategy

**All-At-Once Approach**:
All 4 package updates performed simultaneously as part of the atomic upgrade operation.

**Update Method**:
1. Modify `<PackageReference>` elements in .csproj files
2. Update `Version` attribute from 9.0.8 to 10.0.1
3. Restore packages for entire solution: `dotnet restore`
4. Verify no package conflicts

**Example**:
```xml
<!-- Before -->
<PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version="9.0.8" />

<!-- After -->
<PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version="10.0.1" />
```

---

### Package Compatibility Notes

**Microsoft.Extensions.* Packages**:
- Well-tested, stable abstractions
- Minor version bump (9.0.8 ? 10.0.1)
- No breaking changes in abstraction contracts
- Backward compatible APIs

**Microsoft.AspNetCore.OpenApi**:
- ASP.NET Core package aligned with framework version
- Version 10.0.1 required for .NET 10 applications
- No breaking changes reported

**Risk Level**: ?? Low for all package updates

---

## Breaking Changes Catalog

### Overview

**Total Breaking Changes**: 3 behavioral changes (all in Examples.Web)

**Severity Distribution**:
- ?? Binary Incompatible: 0
- ?? Source Incompatible: 0
- ?? Behavioral Change: 3
- ? Compatible: 2,715 APIs

**Impact**: Minimal - 0.1% of analyzed APIs have behavioral changes, isolated to single project.

---

### Behavioral Changes

#### System.Uri - Behavioral Changes in .NET 10

**Affected Project**: Synaptrix.Examples.Web

**Affected APIs**:
1. `System.Uri` (type) - 2 occurrences
2. `System.Uri..ctor(String)` (constructor) - 1 occurrence

**Impact Level**: ?? Low - Behavioral changes require runtime testing

**Description**:
.NET 10 may introduce changes in how `System.Uri` parses, validates, or normalizes URI strings. This can affect:
- URI parsing behavior
- URI normalization (scheme, host, path handling)
- Query string parsing
- Relative vs absolute URI resolution
- URI validation rules

**Affected Files**:
- 2 files in Examples.Web project contain URI usage
- Estimated 3+ lines of code potentially impacted

**Migration Actions**:

1. **Review URI Construction**:
   - Locate all `new Uri(...)` constructor calls
   - Review URI string formats being passed
   - Check for any complex URI manipulation logic

2. **Test URI-Dependent Functionality**:
   - API endpoint URL generation
   - OpenAPI/Swagger URL construction
   - Any redirect or link generation logic
   - URI validation/parsing logic

3. **Validate Behavioral Consistency**:
   - Compare URI outputs between .NET 9 and .NET 10
   - Check for edge cases (special characters, encoding, relative paths)
   - Verify query string handling
   - Test URI normalization behavior

4. **Update Tests** (if necessary):
   - Update test expectations if URI behavior legitimately changes
   - Add tests for new edge cases if discovered
   - Document intentional behavioral differences

**Recommended Approach**:
- ? Run existing tests first (may already validate URI behavior)
- ? Manual testing of Examples.Web application
- ? Check .NET 10 release notes for specific System.Uri changes
- ? Consider logging URI outputs during testing to compare

**Fallback Options**:
- If behavioral changes are problematic, consider URI abstraction layer
- Use conditional compilation for .NET 10-specific URI handling if needed
- Document known behavioral differences in code comments

---

### Framework Breaking Changes

**None Detected** ?

The assessment found no binary or source incompatibilities across the entire solution.

---

### Package Breaking Changes

**None Detected** ?

All package updates (Microsoft.Extensions.* and Microsoft.AspNetCore.OpenApi) have no reported breaking changes between versions 9.0.8 and 10.0.1.

---

### Breaking Changes by Project

| Project | Binary Incompatible | Source Incompatible | Behavioral Changes | Total Issues |
|---------|---------------------|---------------------|-------------------|--------------|
| Synaptrix.Core | 0 | 0 | 0 | 0 ? |
| Synaptrix.Generator | 0 | 0 | 0 | 0 ? |
| Synaptrix.MediatR | 0 | 0 | 0 | 0 ? |
| Synaptrix.Examples.Web | 0 | 0 | 3 | 3 ?? |
| Synaptrix.Core.Tests | 0 | 0 | 0 | 0 ? |

---

### Expected vs Actual Breaking Changes

**Expected Breaking Changes**: 
- Based on assessment: Only System.Uri behavioral changes in Examples.Web

**Compilation Impact**:
- **Expected**: 0 compilation errors
- **Reason**: No binary or source incompatibilities detected

**Runtime Impact**:
- **Expected**: Possible URI behavior differences in Examples.Web
- **Validation**: Requires testing to confirm actual impact
- **Likelihood**: Low - behavioral changes often subtle or edge-case related

---

### Breaking Changes Resolution Strategy

**For System.Uri Behavioral Changes**:

1. **Detection Phase**:
   - Build solution (should succeed)
   - Run Synaptrix.Core.Tests (validate core functionality)
   - Run Examples.Web (check for runtime exceptions)
   - Manual testing of URI-dependent features

2. **Resolution Phase** (only if issues found):
   - **Option 1**: Update code to match new behavior (preferred)
   - **Option 2**: Add conditional compilation for .NET 10
   - **Option 3**: Abstract URI operations into compatibility layer
   - **Option 4**: Document behavioral difference and update tests

3. **Validation Phase**:
   - Verify all tests pass
   - Validate Examples.Web functions correctly
   - Document any intentional behavioral differences

**Expected Resolution Time**: 
- If no actual issues: None (behavioral changes may not manifest)
- If issues found: Low complexity (isolated to 3 code locations)

---

### Known .NET 10 Breaking Changes (General)

Consult official Microsoft documentation for comprehensive .NET 10 breaking changes:
- [.NET 10 Breaking Changes Documentation](https://learn.microsoft.com/en-us/dotnet/core/compatibility/10.0)

**Categories to Monitor**:
- Core .NET libraries
- ASP.NET Core
- System.Text.Json
- System.Net (including System.Uri)

For this solution, only System.Uri changes were detected by analysis.

---

## Testing & Validation Strategy

### Multi-Level Testing Approach

This upgrade requires validation at three levels: per-project (atomic operation), comprehensive solution-wide, and application-level.

---

### Level 1: Per-Project Validation (Within Atomic Operation)

**Timing**: During the atomic upgrade operation, after all changes applied

**Validation Steps**:

#### Synaptrix.Core
- [ ] **Build Verification**:
  - Builds successfully for net9.0
  - Builds successfully for net8.0
  - Builds successfully for netstandard2.0
  - Builds successfully for net10.0
- [ ] **Package Verification**:
  - Microsoft.Extensions.DependencyInjection.Abstractions 10.0.1 resolves correctly
  - Microsoft.Extensions.Logging.Abstractions 10.0.1 resolves correctly
  - No package conflicts across target frameworks
- [ ] **Build Quality**:
  - 0 compilation errors
  - 0 warnings (or warnings reviewed and acceptable)

#### Synaptrix.MediatR
- [ ] **Build Verification**:
  - Builds successfully for all 4 target frameworks (net9.0, net8.0, netstandard2.0, net10.0)
- [ ] **Dependency Verification**:
  - ProjectReference to Synaptrix.Core resolves correctly for all frameworks
  - Microsoft.Extensions.DependencyInjection.Abstractions 10.0.1 resolves correctly
- [ ] **Build Quality**:
  - 0 compilation errors
  - 0 warnings (or warnings reviewed and acceptable)

#### Synaptrix.Examples.Web
- [ ] **Build Verification**:
  - Builds successfully for net10.0
- [ ] **Dependency Verification**:
  - ProjectReference to Synaptrix.MediatR resolves correctly
  - ProjectReference to Synaptrix.Generator resolves correctly
  - Microsoft.AspNetCore.OpenApi 10.0.1 resolves correctly
- [ ] **Application Startup**:
  - Application starts without errors
  - No runtime exceptions during startup
- [ ] **Build Quality**:
  - 0 compilation errors
  - 0 warnings (or warnings reviewed and acceptable)

#### Synaptrix.Core.Tests
- [ ] **Build Verification**:
  - Builds successfully for net10.0
- [ ] **Dependency Verification**:
  - ProjectReference to Synaptrix.Core resolves correctly
  - ProjectReference to Synaptrix.MediatR resolves correctly
  - Microsoft.Extensions.DependencyInjection 10.0.1 resolves correctly
  - All test framework packages resolve correctly
- [ ] **Build Quality**:
  - 0 compilation errors
  - 0 warnings (or warnings reviewed and acceptable)

---

### Level 2: Solution-Wide Validation

**Timing**: After atomic upgrade completes, before moving to test execution

**Full Solution Build**:
- [ ] `dotnet build Synaptrix.sln` succeeds
- [ ] All projects build without errors
- [ ] No package dependency conflicts
- [ ] Multi-targeting projects build for all frameworks

**Dependency Graph Validation**:
- [ ] All ProjectReferences resolve correctly
- [ ] No circular dependencies introduced
- [ ] Transitive dependencies resolve correctly

**Package Restore Validation**:
- [ ] `dotnet restore` completes successfully
- [ ] No package version conflicts
- [ ] All packages downloaded successfully

---

### Level 3: Functional Testing

**Timing**: After solution builds successfully

#### Unit Tests Execution

**Synaptrix.Core.Tests**:
```bash
dotnet test tests\Synaptrix.Core.Tests\Synaptrix.Core.Tests.csproj
```

**Success Criteria**:
- [ ] All tests discovered
- [ ] All tests execute
- [ ] All tests pass (0 failures)
- [ ] No test infrastructure errors

**Expected Test Coverage**:
- Core mediator functionality
- MediatR integration layer
- Dependency injection registration
- Notification publishing patterns

#### Application Testing

**Synaptrix.Examples.Web**:

**Startup Validation**:
```bash
dotnet run --project examples\Synaptrix.Examples.Web\Synaptrix.Examples.Web.csproj
```

**Success Criteria**:
- [ ] Application starts without errors
- [ ] Web server listens on expected port
- [ ] No runtime exceptions in console

**Swagger UI Validation**:
- [ ] Navigate to Swagger UI endpoint (typically `/swagger`)
- [ ] Swagger UI loads successfully
- [ ] API documentation displays correctly
- [ ] No JavaScript errors in browser console

**API Endpoint Validation**:
- [ ] Test at least one GET endpoint
- [ ] Test at least one POST endpoint (if applicable)
- [ ] Verify responses are correct
- [ ] Check for any URI-related errors in responses

---

### Level 4: Behavioral Change Validation

**Focus**: System.Uri behavioral changes in Examples.Web

#### URI Behavior Testing

**Manual Validation Steps**:
1. **Identify URI Usage**:
   - Review 2 files flagged with URI issues
   - Locate all `new Uri(...)` constructor calls
   - Identify URI-dependent functionality

2. **Test URI Operations**:
   - [ ] Test API endpoint URL generation
   - [ ] Test any redirect logic
   - [ ] Test any link generation
   - [ ] Test any URI parsing/validation

3. **Compare Behavior** (optional):
   - Run Examples.Web on .NET 9 and .NET 10 side-by-side
   - Compare URI outputs/behavior
   - Document any differences

**Automated URI Testing** (if applicable):
- [ ] Run any existing tests that exercise URI operations
- [ ] Add new tests if URI edge cases discovered
- [ ] Validate URI normalization behavior

**Success Criteria**:
- [ ] No URI-related exceptions
- [ ] URI-dependent features work correctly
- [ ] Any behavioral differences are acceptable/documented

---

### Testing Checklist Summary

**Before Declaring Upgrade Complete**:

? **Build Phase**:
- [ ] `dotnet restore Synaptrix.sln` succeeds
- [ ] `dotnet build Synaptrix.sln --configuration Release` succeeds with 0 errors
- [ ] All warnings reviewed (0 warnings or all acceptable)

? **Tests**:
- [ ] `dotnet test Synaptrix.sln` passes with 0 failures
- [ ] All tests discovered and executed
- [ ] No test infrastructure errors

? **Application**:
- [ ] `dotnet run --project examples\Synaptrix.Examples.Web\Synaptrix.Examples.Web.csproj` starts successfully
- [ ] Swagger UI accessible and functional
- [ ] API endpoints respond correctly
- [ ] No console errors or warnings

? **Dependencies**:
- [ ] All 4 packages updated to 10.0.1
- [ ] No package conflicts (`dotnet list package` shows no issues)
- [ ] Transitive dependencies compatible

? **Projects**:
- [ ] Synaptrix.Core targets net10.0 (multi-target)
- [ ] Synaptrix.MediatR targets net10.0 (multi-target)
- [ ] Synaptrix.Examples.Web targets net10.0 (single)
- [ ] Synaptrix.Core.Tests targets net10.0 (single)
- [ ] Synaptrix.Generator remains netstandard2.0

? **Breaking Changes**:
- [ ] System.Uri behavioral changes validated
- [ ] No unexpected runtime issues
- [ ] All functionality works as expected

? **Source Control**:
- [ ] Changes committed to `upgrade-to-NET10` branch
- [ ] Commit messages clear and descriptive
- [ ] Ready for PR review and merge

? **Documentation**:
- [ ] Assessment.md reviewed
- [ ] Plan.md completed (this document)
- [ ] Any code comments added for behavioral changes
- [ ] README updated (if needed)

---

### Definition of Done

**The .NET 10.0 upgrade is DONE when**:

1. ? **All Technical Criteria Met**:
   - All 4 projects targeting/including net10.0
   - All 4 packages updated to version 10.0.1
   - Solution builds without errors
   - All tests pass
   - Application runs successfully

2. ? **All Quality Criteria Met**:
   - Code quality maintained
   - Test coverage maintained
   - Documentation updated
   - Backward compatibility preserved (multi-targeting)

3. ? **All Process Criteria Met**:
   - All-At-Once strategy followed
   - Source control strategy followed
   - Testing strategy completed
   - Risk management applied

4. ? **Changes Merged**:
   - PR created and approved
   - Changes merged to main branch
   - Branch cleanup completed

5. ? **Stakeholders Informed**:
   - Team notified of upgrade completion
   - Any behavioral changes communicated
   - Documentation updated for developers

---

### Post-Completion Validation (Optional)

**After merge to main**:

- [ ] CI/CD pipeline passes on main branch
- [ ] Integration tests pass (if configured)
- [ ] Performance benchmarks acceptable
- [ ] No regressions reported
- [ ] Downstream consumers can upgrade successfully

---

### Continuous Improvement

**Lessons Learned**:
- Document any unexpected issues encountered
- Note any assessment inaccuracies
- Record any process improvements for future upgrades
- Share knowledge with team

**Future Considerations**:
- Monitor .NET 10 release notes for updates
- Plan for .NET 11 upgrade timeline
- Consider removing older target frameworks (net8.0, netstandard2.0) in future major version
- Evaluate .NET 10 performance improvements and new features

---

### Final Sign-Off

**Upgrade Complete When All Criteria Met** ?

**Validated By**: [Name/Role]
**Date**: [Date]
**Version**: Concordia v[X.X.X] with .NET 10.0 support

**Notes**: [Any additional notes or observations]
