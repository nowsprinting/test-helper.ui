# TestHelperUI4004 — IOperator with multiple public constructors is registered without arguments

Detects a call to `OperatorPool.Register<T>` without constructor arguments whose type argument declares more than one public constructor. `OperatorPool.Rent` cannot choose a constructor for such an operator and throws `InvalidOperationException`.

| Item     | Value         |
|----------|---------------|
| Category | Extensibility |
| Enabled  | True          |
| Severity | Error         |
| CodeFix  | False         |

Message: "'{0}' has multiple public constructors. Register with explicit constructor arguments."

`{0}` is the name of the registered type.

Severity is Error because renting an operator registered this way always throws; the message is the same as the exception that `Rent` throws.

## Motivation

`OperatorPool` creates operator instances via reflection. When the operator is registered without constructor arguments (`Register<T>()`), `Rent` calls `Type.GetConstructors()` and throws `InvalidOperationException` ("{type} has multiple public constructors. Register with explicit constructor arguments.") when it finds more than one.

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

public class MyClickOperator : IClickOperator
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

var pool = new OperatorPool()
    .Register<MyClickOperator>();   // TestHelperUI4004
```

## Good

Keep only one public constructor:

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

var pool = new OperatorPool()
    .Register<MyClickOperator>();
```

Or register the operator with explicit constructor arguments:

```csharp
var pool = new OperatorPool()
    .Register<MyClickOperator>(100);
```

## Notes

- The rule applies only to calls without constructor arguments: `Register<T>()`, `Register<T>(null)`, `Register<T>(default)`, and an empty array such as `Register<T>(new object[0])`. A call with constructor arguments is diagnosed by TestHelperUI4011 when no public constructor matches them.
- Only public instance constructors are counted, as `Type.GetConstructors()` does. Non-public and static constructors are ignored.
- A type argument without any public constructor is diagnosed by TestHelperUI4001 instead. When this rule is reported, TestHelperUI4002 and TestHelperUI4003 are not reported for the call.
- The diagnostic is reported at `Register<T>` of the call. See [TestHelperUI4001](TestHelperUI4001.md) for the calls that are not diagnosed.
