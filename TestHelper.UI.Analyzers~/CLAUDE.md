# Development Guidelines

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Roslyn analyzers for code using the UI Test Helper package (`com.nowsprinting.test-helper.ui`).
The analyzer is built with the .NET SDK (not Unity) and the resulting dll is shipped inside the UPM package.
It is not published to nuget.org.

## Directory Structure

This directory (`TestHelper.UI.Analyzers~/`) is the .NET solution root. The trailing `~` hides it from Unity's asset importer.
The parent directory is the UPM package root.

- `TestHelper.UI.Analyzers/` # Analyzer project (netstandard2.0)
- `TestHelper.UI.Analyzers.Tests/` # xUnit tests using Microsoft.CodeAnalysis.Testing (net10.0)
- `../Runtime/` # UPM-facing directory. Contains the built `TestHelper.UI.Analyzers.dll` next to `TestHelper.UI.asmdef`, and the package's own runtime code that the analyzers diagnose the usage of
- `../Documentation~/rules/` # Rule documentation pages

## Analyzer Scope

Unity applies an analyzer dll to the assembly whose folder contains it and to every assembly referencing that assembly.
The dll is in `../Runtime/`, so it runs on user assemblies that reference `TestHelper.UI`, and also on the package's own
`TestHelper.UI`, `Editor/`, and `Tests/` assemblies.
When a rule reports on the package's own code by design (e.g., `GameObjectFinder` calling `GameObject.Find`), lower the severity for that assembly in
`../Runtime/TestHelper.UI.globalconfig` (or the `.globalconfig` next to the other `.asmdef`) instead of suppressing each site.

The dll references Microsoft.CodeAnalysis 4.3.0, so Unity loads it only in versions whose Roslyn is 4.3 or later (Unity 2022.3.12f1 or later).
Older versions report CS8032 instead of loading it. Do not raise the Microsoft.CodeAnalysis version without deciding the new minimum Unity version.

## Test Project Policy

Tests must run on CI without a local Unity installation, so the test project does not reference any local dll.
Unity, NUnit, and TestHelper.UI APIs that analyzers depend on are provided as dummy sources under `TestHelper.UI.Analyzers.Tests/TestData/Dummies/`,
declared with the real namespace, type name, and member signatures.

Test data (the code an analyzer inspects) is placed as individual `.cs` files inside the test project
(`TestHelper.UI.Analyzers.Tests/TestData/<DIAGNOSTIC_ID>/`) rather than as string literals, so that the compiler guarantees every fixture compiles.

## Diagnostics

Diagnostic IDs use the `TestHelperUI` prefix with the category encoded in the first digit:

| Range            | Category      | Description                                                      |
|------------------|---------------|------------------------------------------------------------------|
| TestHelperUI1xxx | Usage         | How to use the package's APIs                                    |
| TestHelperUI2xxx | Alternative   | Use the package's APIs instead of the Unity standard APIs        |
| TestHelperUI3xxx | Performance   | Usage to avoid at runtime                                        |
| TestHelperUI4xxx | Extensibility | Rules for authors extending the package (e.g., custom operators) |

Set `helpLinkUri` on each descriptor to `https://github.com/nowsprinting/test-helper.ui/tree/master/Documentation~/rules/<DIAGNOSTIC_ID>.md`.

When adding or changing a diagnostic:

1. Write a documentation page for the rule under `../Documentation~/rules/`
2. Add a row linking to that page in the matching category table under "Roslyn analyzers" in `../README.md`
3. Add xUnit tests in `TestHelper.UI.Analyzers.Tests/`

## Build

Build the analyzer in Release configuration:

```bash
dotnet build -c Release TestHelper.UI.Analyzers
```

The `CopyToUnityPackage` target in `TestHelper.UI.Analyzers.csproj` copies `TestHelper.UI.Analyzers.dll` into `../Runtime/` after a Release build.
The copied dll is what the UPM package ships, and CI does not build or commit it.
Do not commit `bin/` or `obj/`.

Commit the dll once per pull request, right before pushing, to keep binary commits to a minimum:

- Do not stage or commit `../Runtime/TestHelper.UI.Analyzers.dll` in the commits during development, even after a Release build changes it
- Before pushing, rebuild it in Release configuration from the final source and commit it alone
- Commit it only when the user asks to push or create a pull request

Unity requires the `RoslynAnalyzer` asset label and all platforms disabled on `TestHelper.UI.Analyzers.dll.meta` to load the dll as an analyzer
rather than as a managed plugin. Keep both when the meta file is regenerated.

## Run Tests

```bash
dotnet test
```

## Language

All files, commit messages, GitHub Issues, and Pull Requests must be written in English.
