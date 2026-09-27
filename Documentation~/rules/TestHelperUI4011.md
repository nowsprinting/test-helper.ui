# TestHelperUI4011 — No public constructor of IOperator matches the arguments of OperatorPool.Register

Detects a call to `OperatorPool.Register<T>` with constructor arguments that no public constructor of the type argument can accept. `OperatorPool.Rent` creates the operator with `Activator.CreateInstance` and the registered arguments, so it throws `MissingMethodException`.

| Item     | Value         |
|----------|---------------|
| Category | Extensibility |
| Enabled  | True          |
| Severity | Error         |
| CodeFix  | False         |

Message: "No public constructor of '{0}' matches the arguments"

`{0}` is the name of the registered type.

Severity is Error because the rule reports only when the arguments certainly do not match any public constructor, so renting the operator always throws.

## Motivation

When the operator is registered with constructor arguments (`Register<T>(args)`), `OperatorPool.Rent` passes them to `Activator.CreateInstance(Type, object[])`, which binds only public constructors by the number and runtime types of the arguments. The arguments are stored as `object[]`, so the compiler does not check them against the constructors, and a mismatch such as a wrong argument order, an extra argument, or a `string` for an `int` parameter shows up only when the operator is rented.

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
    public MyClickOperator(int holdMillis = 100, ILogger logger = null)
    {
        _holdMillis = holdMillis;
        Logger = logger;
    }

    public bool CanOperate(GameObject gameObject) => gameObject != null;

    public UniTask OperateAsync(GameObject gameObject, RaycastResult raycastResult = default,
        CancellationToken cancellationToken = default) => UniTask.CompletedTask;
}

var pool = new OperatorPool()
    .Register<MyClickOperator>("500");   // TestHelperUI4011
```

## Good

```csharp
var pool = new OperatorPool()
    .Register<MyClickOperator>(500, Debug.unityLogger);
```

## Notes

- The rule applies only to calls whose arguments are known at compile time: arguments written in the call (e.g., `Register<T>(500)`) or an array created in the call (e.g., `Register<T>(new object[] { 500 })`). A call without constructor arguments, including an empty array such as `new object[0]`, is diagnosed by TestHelperUI4002 and TestHelperUI4004, and a call with an array expression whose contents are unknown at compile time (e.g., an `object[]` variable) is not checked by this rule.
- TestHelperUI4001 (no public constructor, or abstract) takes precedence. When this rule is reported, TestHelperUI4003 is not reported for the call.
- Reproducing the binding rules of the runtime is avoided because they differ between Mono and IL2CPP. The rule reports only when every public constructor certainly does not match:
    - A constructor whose last parameter is a `params` array can always match.
    - Any other constructor can match when the number of arguments is not more than the number of parameters, and no argument is certainly incompatible with its parameter.
    - An argument is certainly incompatible when it is the `null` literal and the parameter is a non-nullable value type, or when its static type is a struct, an enum, or a sealed class (so the runtime type is the static type) and there is no identity, implicit reference, boxing, implicit numeric, or implicit nullable conversion to the parameter type. User-defined implicit conversions do not count, because reflection does not call `op_Implicit`.
    - An argument whose static type is an interface, a non-sealed class, or `object` can always match, because its runtime type is unknown.
    - A nullable value-type argument (e.g., `int?`) can always match, because it is boxed to its underlying value or to null.
    - An enum argument for an integral parameter can always match, because the runtime binders differ on it.
- As a result, some calls that throw at runtime are not reported, e.g., an interface-typed argument that the parameter does not accept, or fewer arguments than parameters (`Activator.CreateInstance` does not fill omitted parameters with their default values). Pass an argument for every constructor parameter, as in Good.
- The diagnostic is reported at `Register<T>` of the call. See [TestHelperUI4001](TestHelperUI4001.md) for the calls that are not diagnosed.
