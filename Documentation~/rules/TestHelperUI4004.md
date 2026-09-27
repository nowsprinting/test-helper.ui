# TestHelperUI4004 — IOperator implementation has multiple public constructors

Detects a concrete class implementing `IOperator` that declares more than one public constructor. `OperatorPool.Rent` cannot choose a constructor for such an operator and throws `InvalidOperationException` unless the operator is registered with explicit constructor arguments.

| Item     | Value         |
|----------|---------------|
| Category | Extensibility |
| Enabled  | True          |
| Severity | Warning       |
| CodeFix  | False         |

Message: "'{0}' has multiple public constructors: the operator cannot be rented unless it is registered with constructor arguments. Keep only one public constructor."

`{0}` is the name of the operator class.

Severity is Warning, not Error, because registering the operator with explicit constructor arguments via `OperatorPool.Register<T>(args)` is a documented way to use such an operator, and the analyzer cannot see the registration.

## Motivation

`OperatorPool` creates operator instances via reflection. When the operator is registered without constructor arguments (`Register<T>()`), or is rented from a pool created with `requireRegistration: false` without being registered, `Rent` calls `Type.GetConstructors()` and throws `InvalidOperationException` ("{type} has multiple public constructors. Register with explicit constructor arguments.") when it finds more than one.

The pool deliberately does not pick one of them (e.g., the first, or the one with the most parameters): `GetConstructors()` does not guarantee the order, and managed code stripping on the Player can remove some of them, so the chosen constructor could silently differ between the Editor and the Player. An operator with a single public constructor whose parameters have default values works with both `Register<T>()` and `Register<T>(args)`.

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

public class MyClickOperator : IClickOperator   // TestHelperUI4004
{
    private readonly int _holdMillis;

    public ILogger Logger { private get; set; }
    public ScreenshotOptions ScreenshotOptions { private get; set; }
    public IVisualizer Visualizer { private get; set; }

    [Preserve]
    public MyClickOperator() : this(100) { }

    [Preserve]
    public MyClickOperator(int holdMillis)
    {
        _holdMillis = holdMillis;
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
    private readonly int _holdMillis;

    // (members other than the constructor are the same as Bad)

    [Preserve]
    public MyClickOperator(int holdMillis = 100)
    {
        _holdMillis = holdMillis;
    }
}
```

## Notes

- The rule applies to non-abstract classes that implement `TestHelper.UI.Operators.IOperator` directly, through a sub-interface (e.g., `IClickOperator`), or through a base class. Abstract classes are skipped because they cannot be instantiated.
- Only constructors declared in the class are counted, as `Type.GetConstructors()` does. Non-public constructors are ignored.
- The diagnostic is reported once at the class identifier.
- An operator without any public constructor is diagnosed by TestHelperUI4001 instead.

If you register the operator with explicit constructor arguments, suppress the diagnostic at the class with `[SuppressMessage]`, or change the severity in `.editorconfig` or `.globalconfig`:

```editorconfig
dotnet_diagnostic.TestHelperUI4004.severity = suggestion
```
