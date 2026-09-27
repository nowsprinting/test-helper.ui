# TestHelperUI4003 — Public constructor of IOperator implementation is not preserved

Detects a public constructor of a concrete class implementing `IOperator` that is not preserved from managed code stripping. `OperatorPool` invokes the constructor only via reflection, so the Unity linker cannot see the call and strips the constructor from the Player.

| Item     | Value         |
|----------|---------------|
| Category | Extensibility |
| Enabled  | True          |
| Severity | Warning       |
| CodeFix  | False         |

Message: "The public constructor of '{0}' is not preserved: managed code stripping can remove the constructor from the Player, and the operator cannot be rented there. Apply 'Preserve' to the constructor."

`{0}` is the name of the operator class.

Severity is Warning, not Error, because the constructor is removed only in a Player build with a raised managed stripping level, which the analyzer cannot see. The default level removes no user-written code (Disabled for Mono, Minimal for IL2CPP), and tests run only in the Editor are never affected. It is not Suggestion, because Unity does not show suggestions in the Console, and operator authors who run monkey tests on the Player should notice it before a build.

## Motivation

`OperatorPool.Rent` creates operator instances via `ConstructorInfo.Invoke` or `Activator.CreateInstance`. The Unity linker cannot detect these reflection calls, so at higher managed stripping levels it removes a constructor that no code calls directly: according to the [marking rules](https://docs.unity3d.com/Manual/managed-code-stripping-marking-rules.html), public members stop being roots at the High level, and at the Medium level for assemblies with types referenced in a scene. `Rent` then finds no public constructor, or `Activator.CreateInstance` throws `MissingMethodException`.

The [Unity Manual](https://docs.unity3d.com/Manual/managed-code-stripping-preserving.html) describes the `[Preserve]` attribute as a root annotation. Applying `[Preserve]` to a method (including a constructor) preserves the method, its declaring type, and the types of its arguments. Applying `[Preserve]` to a type preserves the type and its default constructor only, so it does not keep a constructor that has parameters, even when all of them have default values. Verified against the Unity 6000.4 Manual.

The same page states that the linker recognizes the attribute by name: "You can define the [Preserve] attribute in any assembly and in any namespace. You can use the PreserveAttribute class, create a subclass of it, or create your own class." The package's operators and README use `UnityEngine.Scripting.PreserveAttribute`.

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

    public MyClickOperator(int holdMillis = 100)   // TestHelperUI4003
    {
        _holdMillis = holdMillis;
    }

    public bool CanOperate(GameObject gameObject) => gameObject != null;

    public UniTask OperateAsync(GameObject gameObject, RaycastResult raycastResult = default,
        CancellationToken cancellationToken = default) => UniTask.CompletedTask;
}

public class MyHoverOperator : IHoverOperator   // TestHelperUI4003; the implicit default constructor is not preserved
{
    // (members other than the constructor are omitted)
}

[Preserve]
public class MyToggleOperator : IToggleOperator
{
    // (members other than the constructor are omitted)

    public MyToggleOperator(ILogger logger = null)   // TestHelperUI4003; [Preserve] on the type keeps only the parameterless constructor
    {
        Logger = logger;
    }
}
```

## Good

```csharp
public class MyClickOperator : IClickOperator
{
    // (members other than the constructor are the same as Bad)

    [Preserve]
    public MyClickOperator(int holdMillis = 100)
    {
        _holdMillis = holdMillis;
    }
}

[Preserve]   // keeps the type and its implicit default constructor
public class MyHoverOperator : IHoverOperator
{
    // (members other than the constructor are omitted)
}
```

## Notes

- The rule applies to non-abstract classes that implement `TestHelper.UI.Operators.IOperator` directly, through a sub-interface (e.g., `IClickOperator`), or through a base class. Abstract classes are skipped because they cannot be instantiated.
- A constructor is preserved when either of the following holds:
    - The constructor has an attribute whose class is named `PreserveAttribute`, or derives from a class named `PreserveAttribute`, in any namespace.
    - The constructor has no parameters and the class has such an attribute.
- Every public constructor is checked; when a class has multiple public constructors, each unpreserved one gets its own diagnostic (TestHelperUI4004 is reported separately).
- The diagnostic is reported at the constructor identifier. For a class with no explicit constructor, the compiler-generated default constructor cannot take an attribute, so the diagnostic is reported at the class identifier; apply `[Preserve]` to the class, or declare the constructor explicitly and apply `[Preserve]` to it.
- Non-public constructors are not checked because `OperatorPool` does not invoke them.
- Preservation by `[assembly: Preserve]` or a `link.xml` file is not recognized; the analyzer cannot see `link.xml`, and `[assembly: Preserve]` is documented to preserve types, not their constructors. If you rely on them, change the severity in `.editorconfig` or `.globalconfig`:

```editorconfig
dotnet_diagnostic.TestHelperUI4003.severity = none
```
