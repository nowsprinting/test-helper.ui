# TestHelperUI4002 — Constructor parameter of IOperator registered without arguments cannot be resolved

Detects a call to `OperatorPool.Register<T>` without constructor arguments whose type argument has a public constructor parameter without a default value. `OperatorPool.Rent` can fill such a parameter only if the pool holds a value to inject for its type, and throws `InvalidOperationException` otherwise.

| Item     | Value         |
|----------|---------------|
| Category | Extensibility |
| Enabled  | True          |
| Severity | Error         |
| CodeFix  | False         |

Message: "Cannot resolve required parameter '{0}' of type {1}. Register with explicit constructor arguments or add a default value."

`{0}` is the parameter name, and `{1}` is the name of the parameter type (e.g., `Int32`), the same wording as the exception that `Rent` throws.

Severity is Error because the operator is registered without constructor arguments and the parameter is not of a type that the pool can inject, so renting it always throws.

## Motivation

When the operator is registered without constructor arguments (`Register<T>()`), `OperatorPool.Rent` resolves each parameter of the single public constructor in this order:

1. The value passed to the `OperatorPool` constructor, if the parameter type is exactly one of `ILogger`, `ScreenshotOptions`, `IVisualizer`, `Func<GameObject, Vector2>`, `IReachableStrategy`, or `IRandom`, and the value is not null
2. The default value of the parameter
3. Otherwise, throws `InvalidOperationException` ("Cannot resolve required parameter '{name}' of type {type}. Register with explicit constructor arguments or add a default value.")

Every built-in operator gives all constructor parameters default values, so that it works with both `Register<T>()` and `Register<T>(args)`.

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
    public MyClickOperator(int holdMillis, ILogger logger)
    {
        _holdMillis = holdMillis;
        Logger = logger;
    }

    public bool CanOperate(GameObject gameObject) => gameObject != null;

    public UniTask OperateAsync(GameObject gameObject, RaycastResult raycastResult = default,
        CancellationToken cancellationToken = default) => UniTask.CompletedTask;
}

var pool = new OperatorPool(logger: Debug.unityLogger)
    .Register<MyClickOperator>();   // TestHelperUI4002 for holdMillis
```

## Good

Add default values to the parameters:

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

var pool = new OperatorPool()
    .Register<MyClickOperator>();
```

Or register the operator with explicit constructor arguments:

```csharp
var pool = new OperatorPool()
    .Register<MyClickOperator>(100, Debug.unityLogger);
```

## Notes

- The rule applies only to calls without constructor arguments: `Register<T>()`, `Register<T>(null)`, and `Register<T>(default)`.
- The rule applies only when the type argument has exactly one public constructor, which is the constructor `Rent` resolves parameters for. TestHelperUI4001 (no public constructor, or abstract) and TestHelperUI4004 (multiple public constructors) take precedence; when this rule is reported, TestHelperUI4003 is not reported for the call.
- Only the first parameter that cannot be resolved is reported.
- Parameters of the injectable types listed in Motivation are not reported, because whether the pool holds a value for them is unknown at the `Register<T>` call. As a result, registering an operator with an `ILogger` parameter without a default value to a pool created without a logger is not detected.
- A `params` array parameter has no default value and is reported. `ref` and `out` parameters cannot have a default value and are reported.
- A parameter with only `[Optional]` and no `[DefaultParameterValue]` is reported: reflection reports no default value for it (`ParameterInfo.HasDefaultValue` is false), so `OperatorPool` cannot resolve it either. A default value given by `[Optional, DefaultParameterValue(...)]` counts as a default value.
- The diagnostic is reported at `Register<T>` of the call. See [TestHelperUI4001](TestHelperUI4001.md) for the calls that are not diagnosed.
