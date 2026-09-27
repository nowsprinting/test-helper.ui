# <DIAGNOSTIC_ID> — <Title>

One or two sentences describing what this diagnostic detects.

| Item     | Value                                        |
|----------|----------------------------------------------|
| Category | One of the categories defined in the project |
| Enabled  | True                                         |
| Severity | e.g., Warning                                |
| CodeFix  | False                                        |

Add notes on the table here if any, such as the rationale for the chosen severity.

## Motivation

One or two paragraphs on why this rule is needed (the underlying defect or design policy). If it partially overlaps with a rule from an existing OSS analyzer, explain in a subsection why that rule cannot be used instead.

## Bad

```csharp
// Code example that triggers the diagnostic
```

## Good

```csharp
// Code example that does not trigger the diagnostic (recommended)
```

## Notes

Exclusion conditions, known limitations, an `.editorconfig` or `.globalconfig` example for changing the severity, etc. Write only if applicable.
