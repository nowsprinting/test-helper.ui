---
name: implement-diagnostic
description: Implements a Roslyn analyzer and its tests test-first from a specification file (../Documentation~/rules/<DIAGNOSTIC_ID>.md). Use when asked to implement a diagnostic rule, or via /implement-diagnostic <spec file>. If the specification file does not exist yet, prompt the user to run design-diagnostic first.
argument-hint: "<spec file>"
license: MIT
metadata:
  author: Koji Hasegawa
---

# Implementing a Diagnostic Rule

Implement the analyzer and its tests from `<spec file>` (e.g., `../Documentation~/rules/TestHelperUI1001.md`),
test-first: Red → Green → Refactor.

## Prerequisites

- If the specification file does not exist or is still a draft, prompt the user to run the `design-diagnostic` skill and stop here.
- Build and test with `dotnet build` / `dotnet test` in the repository root (the `TestHelper.UI.Analyzers.sln` solution). This is not Unity Test Framework, so do not use `/run-tests` or `run_unity_tests`.
- No Unity editor installation is required. Tests run in CI without any local file references: Unity, NUnit, and TestHelper.UI APIs are provided as dummy sources under `TestHelper.UI.Analyzers.Tests/TestData/Dummies/` (see `resources/test-data-conventions.md` in the `analyzer-code-writing-guide` skill).

## Steps

### Step 0: Read the spec and confirm the gaps

1. Read the specification file and make sure you understand: diagnostic ID, Category, Severity, Title/MessageFormat/Description, detection conditions (all must be satisfied), exclusion conditions, Bad/Good examples, and known limitations.
2. **Confirm any ambiguous or unspecified points with `AskUserQuestion` before implementing.** Even a reviewed spec always leaves branches that only surface during implementation (e.g., how to treat several branches that return different concrete types, where to point the suggestion when no matching interface type exists, how to express the expected diagnostic location in tests). Deciding these on your own leads to a spec mismatch later.
3. Reflect minor decisions in the spec file right away; batch the decisions that affect the implementation into Step 5.

### Step 1: Check the test harness prerequisites

The harness `TestHelper.UI.Analyzers.Tests/TestDataVerifier.cs` exists. In the test file, alias
`using Verifier = TestHelper.UI.Analyzers.Tests.TestDataVerifier<NewAnalyzerType>;` and reuse it. Do not modify the harness.

When implementing the first diagnostic in this repository, `TestData/Dummies/` does not exist yet; `TestDataFiles.Dummies` throws until at least
one dummy is added under it. Create the dummies the first diagnostic needs, following the "Dummies" section of `resources/test-data-conventions.md`
in the `analyzer-code-writing-guide` skill. The first Bad fixture that reports an expected diagnostic at the expected location proves the harness
(dummy resolution, the `Sources[0]` path assumption, and `WithLocation` line numbers), so do not trust a green run that has only zero-diagnostic cases.

### Step 2: Skeleton (compilable)

1. Create `TestHelper.UI.Analyzers/<AnalyzerName>.cs`. **Complete `DiagnosticDescriptor` and `SupportedDiagnostics`**
   (`Verifier.Diagnostic()` requires exactly one entry in `SupportedDiagnostics` and throws when it is empty, so this part cannot be left empty).
   Set `helpLinkUri` on the descriptor to the rule's page on the documentation site (see `CLAUDE.md`).
   Leave the body of `Initialize()` and the analysis logic empty.
2. Helpers shared by multiple analyzers go in the `Utilities/` folder (namespace `TestHelper.UI.Analyzers.Utilities`), following the `Analyzer.Utilities` layout in dotnet/roslyn-analyzers.
3. Confirm `dotnet build` passes, then commit.

### Step 3: Test first

1. Break the Bad/Good examples and the exclusion conditions in the spec into independent test cases.
2. Create the fixtures at `TestHelper.UI.Analyzers.Tests/TestData/<DIAGNOSTIC_ID>/<CaseName>.cs`, following the "Fixtures" section
   of `resources/test-data-conventions.md` in the `analyzer-code-writing-guide` skill.
3. **Compute `WithLocation(line, col)` mechanically by re-reading the fixture after writing it.** No eyeballing.
   The skeleton reports nothing, so a wrong location looks identical to a correct one at this stage, and during implementation
   you can no longer tell "analyzer bug" from "wrong expectation".
4. Run `dotnet test` and confirm that **only the cases that should report are red**. Exclusion cases (iterators, overrides,
   interface implementations, and whatever else the spec excludes) correctly pass with nothing reported, because the skeleton reports nothing.
5. Commit the test code (touching it afterwards loses the test-first verifiability).

### Step 4: Implement

1. Read the `analyzer-code-writing-guide` skill (CancellationToken, `ConcurrentBuild`, `EnableConcurrentExecution`, backward compatibility, "why not" comments, handling diagnostics and review feedback).
2. Implement the analyzer. Policy:
   - Compare symbols with `SymbolEqualityComparer.Default` on `OriginalDefinition`. Never match on `.Name` strings (do not copy the old pattern in the sample analyzers)
   - Resolve well-known types by metadata name (`GetTypeByMetadataName`) once per compilation in `RegisterCompilationStartAction`; avoid per-node `GetTypeByMetadataName` calls
   - Never branch on `ContainingAssembly.Name`. In tests the Unity/NUnit/TestHelper.UI types come from dummies compiled into the test assembly, not from `UnityEngine`, `nunit.framework`, or `TestHelper.UI`
   - No code fixes. No localization (Title/MessageFormat/Description are English raw strings)
3. Confirm all tests pass with `dotnet test`. Correcting an off-by-N `WithLocation` value is a fix to a location assertion and does not break test-first.
   If the expected diagnostic arguments (`{0}`/`{1}`/`{2}`...) or the number of cases would need to change, suspect the implementation, not the tests.
4. Commit the production code (including any unavoidable `WithLocation` corrections).

### Step 5: Refactor and finalize the documentation

1. Review the added/changed test files for duplicates and cases that can be merged into `[Theory]`/`[InlineData]`.
2. Run the Claude Code built-in `/simplify` skill (`Skill({skill: "simplify"})`, not a plugin skill) and apply the quality improvements to the changed code.
3. Re-confirm all tests pass with `dotnet test`.
4. If possible, resolve diagnostics at the `warning` or higher severity level on every file added or changed in Steps 1–4, collecting them via `lint_files` (one call with all files) rather than one file at a time.
   By default the tools also return `suggestion`- and `hint`-level results — those are out of scope, with one exception: a `suggestion`-level diagnostic related to performance (execution speed, memory allocation, boxing, etc.) should be considered for fixing too, though it never requires suppression when declined.
   Both `lint_files` and its fallback `get_file_problems` require Rider 2026.2 or later.
   - If the call errors, fall back to `get_file_problems`, called once per file
   - On a `timedOut` or `more` result, retry the outstanding file(s) with `get_file_problems`
   - If a timeout persists, use `AskUserQuestion` to confirm the user is running Rider 2026.2 or later before retrying further
   - Decide each diagnostic per the `analyzer-code-writing-guide` skill (handling diagnostics), and apply all resulting changes as a single set per file
   - When a fix is a rename (naming-convention inspections) or a symbol removal (unused-member findings), apply it via `rename_refactoring` / `safe_delete` so references are updated across the solution
   - Re-confirm all tests pass with `dotnet test` if anything changed<br>
   **Known limitation**: same as item 5 below — if the connected Rider instance has the Unity solution open, the call fails with `Requested files are not part of the current solution`. In that case skip this item.
5. If possible, call `reformat_file` once on every file added or changed in Steps 1–4.<br>
   **Known limitation**: `TestHelper.UI.Analyzers.sln` is meant to be opened in a Rider instance separate from the Unity project.
   If the connected Rider instance has the Unity solution open, the call fails with `Requested files are not part of the current solution`.
   In that case do not force a switch; format by hand to match the conventions of the existing files and skip this item.
6. Add the row to the matching category table under "Roslyn analyzers" in `../README.md` if it is not there yet (normally already added by the `design-diagnostic` skill).
7. Reflect any decisions from Step 0 that are not yet in the spec file (typically under "Notes" as known limitations).
8. Commit the remaining changes.

## Verification

```bash
dotnet build   # no new warnings
dotnet test    # all green
```
