# .NET 10 Upgrade Plan

## Execution Steps

Execute steps below sequentially one by one in the order they are listed.

1. Validate that an .NET 10 SDK required for this upgrade is installed on the machine and if not, help to get it installed.
2. Ensure that the SDK version specified in any `global.json` files is compatible with the .NET 10 upgrade.

3. Upgrade stdTernary/stdTernary.csproj
4. Upgrade stdTernary/stdTernary.Tests/stdTernary.Tests.csproj

5. Run unit tests to validate upgrade in the projects listed below:
  - stdTernary.Tests/stdTernary.Tests.csproj


## Settings

This section contains settings and data used by execution steps.

### Excluded projects

| Project name                                   | Description                 |
|:-----------------------------------------------|:---------------------------:|


### Aggregate NuGet packages modifications across all projects

No NuGet package version updates are required according to the analysis results. All referenced packages were marked compatible with .NET 10.


### Project upgrade details

#### stdTernary/stdTernary.csproj modifications

Project properties changes:
  - Target framework should be changed from `net8.0` to `net10.0`

NuGet packages changes:
  - None required; packages in project are compatible with .NET 10.

Feature upgrades:
  - No API compatibility issues reported by analysis; recompile and run tests to validate runtime behavior.

Other changes:
  - None identified by analysis.


#### stdTernary.Tests/stdTernary.Tests.csproj modifications

Project properties changes:
  - Target framework should be changed from `net8.0` to `net10.0`

NuGet packages changes:
  - None required; test-related packages are compatible with .NET 10.

Feature upgrades:
  - Ensure test SDK and runners continue to work after framework change; run test suite.

Other changes:
  - None identified by analysis.
