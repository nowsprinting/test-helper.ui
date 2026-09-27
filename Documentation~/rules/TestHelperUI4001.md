# TestHelperUI4001 — IOperator implementation has no public constructor

Detects a concrete class implementing `IOperator` whose constructors are all non-public. `OperatorPool.Rent` creates operators only through a public constructor, so it always throws for such an operator.

| Item     | Value         |
|----------|---------------|
| Category | Extensibility |
| Enabled  | True          |
| Severity | Warning       |
| CodeFix  | False         |

Message: "'{0}' has no public constructor: the operator cannot be rented. Make the constructor public."

`{0}` is the name of the operator class.

Severity is Warning, not Error, because the operator fails only when it is rented from `OperatorPool`, and the analyzer cannot see whether the operator is used with `OperatorPool` or only created directly, e.g., by a factory method in test code. When it is rented, renting throws on every path, with or without registered constructor arguments.

## Motivation

Without registered constructor arguments, `OperatorPool.Rent` calls `Type.GetConstructors()`, which returns public instance constructors only, and throws `InvalidOperationException` ("{type} has no public constructor.") when the result is empty. With registered arguments, `Rent` calls `Activator.CreateInstance(Type, object[])`, which also binds public constructors only and throws `MissingMethodException`.

A private or internal constructor is a natural choice for an operator author who wants to force construction through a factory method or a singleton, but it makes the operator unusable with `OperatorPool`, and therefore with monkey testing.

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

public class MyClickOperator : IClickOperator   // TestHelperUI4001
{
    public static readonly MyClickOperator Instance = new MyClickOperator();

    public ILogger Logger { private get; set; }
    public ScreenshotOptions ScreenshotOptions { private get; set; }
    public IVisualizer Visualizer { private get; set; }

    private MyClickOperator() { }

    public bool CanOperate(GameObject gameObject) => gameObject != null;

    public UniTask OperateAsync(GameObject gameObject, RaycastResult raycastResult = default,
        CancellationToken cancellationToken = default) => UniTask.CompletedTask;
}
```

## Good

```csharp
public class MyClickOperator : IClickOperator
{
    // (members other than the constructor are the same as Bad, without the Instance field)

    [Preserve]
    public MyClickOperator() { }
}
```

## Notes

- The rule applies to non-abstract classes that implement `TestHelper.UI.Operators.IOperator` directly, through a sub-interface (e.g., `IClickOperator`), or through a base class. Abstract classes are skipped because they cannot be instantiated.
- A class with no explicit constructor has a public compiler-generated default constructor and is not reported.
- Static constructors are not counted.
- The diagnostic is reported once at the class identifier.
