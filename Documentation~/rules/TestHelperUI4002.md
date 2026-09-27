# TestHelperUI4002 — Constructor parameter of IOperator implementation has no default value

Detects a parameter without a default value in the public constructor of a concrete class implementing `IOperator`. When the operator is registered without constructor arguments, `OperatorPool.Rent` can fill such a parameter only if the pool holds a value to inject for its type, and throws `InvalidOperationException` otherwise.

| Item     | Value         |
|----------|---------------|
| Category | Extensibility |
| Enabled  | True          |
| Severity | Warning       |
| CodeFix  | False         |

Message: "Parameter '{0}' of the '{1}' constructor has no default value: the operator cannot be rented unless the pool injects the value or the operator is registered with constructor arguments. Add a default value."

`{0}` is the parameter name, and `{1}` is the name of the operator class.

Severity is Warning, not Error, because the operator works when it is registered with explicit constructor arguments via `OperatorPool.Register<T>(args)`, or when the pool is created with a value to inject for the parameter type; the analyzer cannot see either.

## Motivation

When the operator is registered without constructor arguments (`Register<T>()`), `OperatorPool.Rent` resolves each parameter of the single public constructor in this order:

1. The value passed to the `OperatorPool` constructor, if the parameter type is exactly one of `ILogger`, `ScreenshotOptions`, `IVisualizer`, `Func<GameObject, Vector2>`, `IReachableStrategy`, or `IRandom`, and the value is not null
2. The default value of the parameter
3. Otherwise, throws `InvalidOperationException` ("Cannot resolve required parameter '{name}' of type {type}. Register with explicit constructor arguments or add a default value.")

All of these values are optional in the `OperatorPool` constructor, and the pool that `MonkeyConfig` creates by default holds none of them. A parameter without a default value therefore makes the operator depend on how the caller configures the pool. Every built-in operator gives all constructor parameters default values, and the README requires the same of paginators.

## Bad

```csharp
using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.UI;
using TestHelper.UI.Operators;
using TestHelper.UI.Visualizers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Scripting;

public class MyClickOperator : IClickOperator
{
    private readonly int _holdMillis;

    public ILogger Logger { private get; set; }
    public ScreenshotOptions ScreenshotOptions { private get; set; }
    public IVisualizer Visualizer { private get; set; }

    [Preserve]
    public MyClickOperator(int holdMillis, ILogger logger)   // TestHelperUI4002 at holdMillis and logger
    {
        _holdMillis = holdMillis;
        Logger = logger;
    }

    public bool CanOperate(GameObject gameObject) => gameObject != null;

    public UniTask OperateAsync(GameObject gameObject, RaycastResult raycastResult = default,
        CancellationToken cancellationToken = default) => UniTask.CompletedTask;
}
```

## Good

```csharp
public class MyClickOperator : IClickOperator
{
    // (members other than the constructor are the same as Bad)

    [Preserve]
    public MyClickOperator(int holdMillis = 100, ILogger logger = null)
    {
        _holdMillis = holdMillis;
        Logger = logger ?? Debug.unityLogger;
    }
}
```

## Notes

- The rule applies to non-abstract classes that implement `TestHelper.UI.Operators.IOperator` directly, through a sub-interface (e.g., `IClickOperator`), or through a base class. Abstract classes are skipped because they cannot be instantiated.
- The rule applies only when the class has exactly one public constructor, which is the constructor `Rent` resolves parameters for. A class with multiple public constructors is diagnosed by TestHelperUI4004 instead.
- Parameters of the injectable types are reported too, because the pool injects them only when the caller passes a non-null value to the `OperatorPool` constructor.
- A `params` array parameter has no default value and is reported. `ref` and `out` parameters cannot have a default value and are reported.
- A parameter with only `[Optional]` and no `[DefaultParameterValue]` is reported: reflection reports no default value for it (`ParameterInfo.HasDefaultValue` is false), so `OperatorPool` cannot resolve it either. A default value given by `[Optional, DefaultParameterValue(...)]` counts as a default value.
- The diagnostic is reported at each parameter identifier.

If you always register the operator with explicit constructor arguments, or always create the pool with the values to inject, suppress the diagnostic with `[SuppressMessage]`, or change the severity in `.editorconfig` or `.globalconfig`:

```editorconfig
dotnet_diagnostic.TestHelperUI4002.severity = suggestion
```
