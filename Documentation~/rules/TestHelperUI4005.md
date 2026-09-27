# TestHelperUI4005 — IOperator implementation does not implement a sub-interface

Detects a concrete class that implements `IOperator` but no sub-interface of it (e.g., `IClickOperator`). The sub-interface is what represents the kind of operation the operator performs.

| Item     | Value         |
|----------|---------------|
| Category | Extensibility |
| Enabled  | True          |
| Severity | Suggestion    |
| CodeFix  | False         |

Message: "'{0}' implements IOperator but no sub-interface: callers cannot refer to the operator by the kind of operation. Implement a sub-interface such as 'IClickOperator'."

`{0}` is the name of the operator class.

Severity is Suggestion because `OperatorPool` and monkey testing accept any `IOperator` implementation, so the operator works; the rule only encourages the design policy of the package. Suggestions appear in the IDE; raise the severity to `warning` (see the end of Notes) to report them in the compiler output as well.

## Motivation

The package's `IOperator` documentation and README require every operator to implement a sub-interface of `IOperator` to represent the kind of operator. `IOperator` itself declares only the members for monkey testing (`CanOperate`, `OperateAsync`, and the setters for the logger, screenshot options, and visualizer), and says nothing about what the operation is.

The sub-interface is the type that callers depend on. Test code holds an operator as `IToggleOperator` or `ITextInputOperator` so that the implementation for another UI framework can be swapped in, and a sub-interface declares the overloads specific to its kind of operation, such as the text to input or the destination to drop at. An operator that implements `IOperator` directly can be referred to only by its concrete class.

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

public class MyClickOperator : IOperator   // TestHelperUI4005
{
    public ILogger Logger { private get; set; }
    public ScreenshotOptions ScreenshotOptions { private get; set; }
    public IVisualizer Visualizer { private get; set; }

    [Preserve]
    public MyClickOperator() { }

    public bool CanOperate(GameObject gameObject) => gameObject != null;

    public UniTask OperateAsync(GameObject gameObject, RaycastResult raycastResult = default,
        CancellationToken cancellationToken = default) => UniTask.CompletedTask;
}
```

## Good

```csharp
public class MyClickOperator : IClickOperator
{
    // (members are the same as Bad)
}

// A sub-interface defined by the operator author also satisfies the rule
public interface ILongPressOperator : IOperator
{
}

public class MyLongPressOperator : ILongPressOperator
{
    // (members are the same as Bad)
}
```

## Notes

- The rule applies to non-abstract classes that implement `TestHelper.UI.Operators.IOperator` directly, through a sub-interface, or through a base class. Abstract classes are skipped; they are checked through their concrete subclasses.
- A sub-interface is any interface, including one defined by the operator author, that inherits `IOperator` directly or indirectly (e.g., `IToggleOperator : IClickOperator`). A sub-interface implemented by a base class counts.
- Interfaces that do not inherit `IOperator`, such as `IScreenPointCustomizable`, do not count.
- The diagnostic is reported once at the class identifier (of the first declaration, for a partial class).

To change the severity, add the following to `.editorconfig` or `.globalconfig`:

```editorconfig
dotnet_diagnostic.TestHelperUI4005.severity = warning
```
