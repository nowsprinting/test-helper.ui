# TestHelperUI4001 — IOperator registered to OperatorPool has no usable public constructor

Detects a call to `OperatorPool.Register<T>` whose type argument is an interface, is abstract, or has no public constructor. `OperatorPool.Rent` creates operators only through a public constructor, so it always throws when it has to create an instance of such an operator.

| Item     | Value         |
|----------|---------------|
| Category | Extensibility |
| Enabled  | True          |
| Severity | Error         |
| CodeFix  | False         |

Message: "'{0}' {1}"

`{0}` is the name of the registered type, and `{1}` is "is an interface" for an interface, "is an abstract class" for an abstract class, or "has no public constructor" otherwise.

Severity is Error because renting the registered operator throws on every path, with or without registered constructor arguments.

## Motivation

Without registered constructor arguments, `OperatorPool.Rent` calls `Type.GetConstructors()`, which returns public instance constructors only, and throws `InvalidOperationException` ("{type} has no public constructor.") when the result is empty. With registered arguments, `Rent` calls `Activator.CreateInstance(Type, object[])`, which also binds public constructors only and throws `MissingMethodException`. For an abstract class, the constraint `where T : class, IOperator` accepts the type, but creating an instance throws `MemberAccessException`.

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

public class MyClickOperator : IClickOperator
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

var pool = new OperatorPool()
    .Register<MyClickOperator>();   // TestHelperUI4001
```

## Good

```csharp
public class MyClickOperator : IClickOperator
{
    // (members other than the constructor are the same as Bad, without the Instance field)

    [Preserve]
    public MyClickOperator() { }
}

var pool = new OperatorPool()
    .Register<MyClickOperator>();
```

## Notes

- The rule is checked first for every `Register<T>` call, regardless of the arguments. When it is reported, no other rule is reported for the call.
- An interface type argument has no constructor and is reported with "is an interface".
- A class with no explicit constructor has a public compiler-generated default constructor and is not reported.
- Static constructors are not counted.
- The diagnostic is reported at `Register<T>` of the call, also when the call is made through a class derived from `OperatorPool`, through `?.`, or in a fluent chain.
- A call whose type argument is a generic type parameter is not diagnosed, because the actual type is unknown.
- `Rent<T>()` and `Rent(Type)` calls are not diagnosed, so renting an unregistered operator from a pool created with `requireRegistration: false` is not detected.
- An operator declaration that is never registered is not diagnosed. A library that only declares operators gets no diagnostic until a user registers them.
