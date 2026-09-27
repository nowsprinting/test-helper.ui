# TestHelperUI3002 — OperatorPool.RentAll is called without a list

Detects a call to `OperatorPool.RentAll` that does not pass a list to store the rented operators. Without a list, `RentAll` allocates a new one on every call.

| Item     | Value       |
|----------|-------------|
| Category | Performance |
| Enabled  | True        |
| Severity | Suggestion  |
| CodeFix  | False       |

Message: "'OperatorPool.RentAll' is called without a list: a new list is allocated on every call. Pass a list to reuse."

Severity is Suggestion, not Warning, because a single allocation is small and matters only when the call is repeated, e.g., once per frame or once per iteration of a monkey testing loop; the analyzer cannot see how often the call runs. A one-off call, such as in the Arrange step of a test, is fine. Suggestions appear in the IDE; raise the severity to `warning` (see the end of Notes) to report them in the compiler output as well.

## Motivation

`OperatorPool.RentAll(List<IOperator> operators = null)` stores the rented operators in `operators` after clearing it, and returns the same instance. When `operators` is null, it allocates a new `List<IOperator>` for each call. The operators themselves are pooled, but the list holding them is not, so a caller that rents all operators repeatedly produces GC garbage on every call even after the pool is warmed up.

Passing a list owned by the caller and reused across calls avoids the allocation. The package's own `InteractableComponentsFinder` rents operators this way for each lookup in monkey testing.

## Bad

```csharp
using TestHelper.UI;
using TestHelper.UI.Operators;
using UnityEngine;

public class OperatorSelector : MonoBehaviour
{
    private readonly OperatorPool _pool = new OperatorPool().Register<UguiClickOperator>();

    private void Update()
    {
        var operators = _pool.RentAll();   // TestHelperUI3002
        foreach (var iOperator in operators)
        {
            // (use the operator)
            _pool.Return(iOperator);
        }
    }
}
```

## Good

```csharp
using System.Collections.Generic;
using TestHelper.UI;
using TestHelper.UI.Operators;
using UnityEngine;

public class OperatorSelector : MonoBehaviour
{
    private readonly OperatorPool _pool = new OperatorPool().Register<UguiClickOperator>();
    private readonly List<IOperator> _operators = new List<IOperator>();

    private void Update()
    {
        _pool.RentAll(_operators);
        foreach (var iOperator in _operators)
        {
            // (use the operator)
            _pool.Return(iOperator);
        }
    }
}
```

`RentAll` clears the passed list before storing the operators. Do not pass a list that an enumeration still in progress depends on, e.g., a list shared by a nested or interleaved call of the same method; take the list out of the shared field while it is in use, as `InteractableComponentsFinder` does.

## Notes

- The rule applies to invocations of `TestHelper.UI.OperatorPool.RentAll(List<IOperator>)`, including a call through a subclass of `OperatorPool` and a conditional access such as `pool?.RentAll()`.
- The diagnostic is reported when the `operators` argument is omitted, or when it is a constant null value, such as `null`, `default`, or `operators: null`. These allocate a new list just like the omitted argument.
- An argument that is not a constant, such as a variable or a field that may be null at runtime, is not reported, because the analyzer cannot see its value.
- The diagnostic is reported at the invocation expression.

To change the severity, add the following to `.editorconfig` or `.globalconfig`:

```editorconfig
dotnet_diagnostic.TestHelperUI3002.severity = warning
```
