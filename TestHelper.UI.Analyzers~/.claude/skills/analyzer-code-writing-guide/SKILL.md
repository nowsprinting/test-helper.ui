---
name: analyzer-code-writing-guide
description: Shared conventions for writing code in this repository (Roslyn analyzers). Covers CancellationToken and concurrent execution handling in DiagnosticAnalyzer implementations, backward compatibility, "why not" comments, how to handle diagnostics and review feedback, and how test data (fixtures and dummies under TestHelper.UI.Analyzers.Tests/TestData/) is compiled and written. Always use when creating or editing .cs files in this repository.
license: MIT
metadata:
  author: Koji Hasegawa
---

# Shared Conventions for Analyzer Code

Read this before writing or editing code under `TestHelper.UI.Analyzers/` and `TestHelper.UI.Analyzers.Tests/`.

## Implementing DiagnosticAnalyzer

- In any method that receives `context.CancellationToken`, check for cancellation frequently with `context.CancellationToken.ThrowIfCancellationRequested()`.
- `compilation.Options.ConcurrentBuild` tells whether the compilation is running in a multi-threaded environment. Use it where work can be parallelized.
- Call `context.EnableConcurrentExecution()` in `Initialize(AnalysisContext)`.

## Other Conventions

- Backward compatibility: see the "Backward Compatibility" section in `resources/coding-guideline.md`.
- Recording implementation decisions: see the ""Why Not" Comments" section in `resources/coding-guideline.md`.
- Handling IDE diagnostics, analyzers, and review feedback: see `resources/diagnostics-review-feedback.md`.
- Test data (how fixtures and dummies are compiled, how to write fixtures and dummies): see `resources/test-data-conventions.md`.
