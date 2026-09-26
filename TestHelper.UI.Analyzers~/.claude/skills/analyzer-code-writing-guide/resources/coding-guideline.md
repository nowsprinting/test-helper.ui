# Coding Guidelines

## Backward Compatibility

Do NOT maintain backward compatibility unless explicitly requested. Break things boldly — backward compatibility layers accumulate over time, increasing maintenance cost and making the codebase harder to evolve.
The analyzer dll has no public API for users, so delete unused members outright — no `[Obsolete]` stage.
Diagnostic IDs and default severities are user-facing, though: users configure them in `.editorconfig` / `.globalconfig`. Never reuse or renumber an ID that has shipped.

## "Why Not" Comments

Add a comment whenever a non-obvious implementation choice was made — especially when a natural or standard approach was tried and rejected.
The goal is to prevent future readers (human or AI) from rediscovering the same dead end.

**Triggers that require a "why not" comment:**

- A standard API or language feature is avoided because it misbehaves in a specific environment
  (e.g., `withTimeout` deadlocks on the IntelliJ platform test JVM EDT → use `java.util.Timer`)
- A less-efficient or more verbose pattern is chosen over a simpler one for correctness reasons
- A seemingly redundant guard, indirection, or workaround exists due to a framework constraint
