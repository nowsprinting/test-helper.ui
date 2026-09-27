# TestHelperUI4010 — IPaginator implementation does not implement IPaginator&lt;TComponent&gt;

Detects a concrete class that implements `IPaginator` but not `IPaginator<TComponent>`. `PaginatorPool.Rent(targetComponent)` selects a paginator by the `TComponent` of its `IPaginator<TComponent>`, so it never selects such a paginator.

| Item     | Value         |
|----------|---------------|
| Category | Extensibility |
| Enabled  | True          |
| Severity | Warning       |
| CodeFix  | False         |

Message: "'{0}' implements IPaginator but not IPaginator&lt;TComponent&gt;: the paginator cannot be rented by its target component. Implement 'IPaginator&lt;TComponent&gt;' instead."

`{0}` is the name of the paginator class.

Severity is Warning, not Error, because the paginator works with `GameObjectFinder`, `PaginatorPool.Rent<T>()`, and `PaginatorPool.Rent(Type)`, which accept any `IPaginator`; it fails only when it is rented by its target component, and the analyzer cannot see which of these the paginator is used with. It is not Suggestion, because renting by the target component is a documented API that throws for such a paginator, and Unity does not show suggestions in the Console.

## Motivation

`PaginatorPool.Rent(targetComponent)` looks up the registered (or pooled) paginator types that implement `IPaginator<TComponent>` whose `TComponent` exactly matches the type of the target component. A paginator that implements only `IPaginator` is never among the candidates, so renting it by its target component throws `InvalidOperationException` ("No paginator for {component} is registered."), even when the paginator is registered and supports the component.

The `IPaginator` documentation and the README require implementing `IPaginator<TComponent>` instead of `IPaginator` directly. `IPaginator` is the non-generic base that exists for callers that do not know the component type statically, such as `GameObjectFinder` and `PaginatorPool`. The type argument also declares, in the type itself, which pageable component the paginator controls.

## Bad

In the examples, `Carousel` is a custom pageable component (a `MonoBehaviour`) of the game.

```csharp
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.UI.Paginators;
using UnityEngine;
using UnityEngine.Scripting;

public class CarouselPaginator : IPaginator   // TestHelperUI4010
{
    private Carousel _carousel;

    public MonoBehaviour TargetComponent
    {
        set
        {
            if (value != null && !(value is Carousel))
            {
                throw new ArgumentException($"TargetComponent must be a Carousel, but was {value.GetType().Name}.");
            }

            _carousel = (Carousel)value;
        }
    }

    [Preserve]
    public CarouselPaginator(Carousel carousel = null)
    {
        TargetComponent = carousel;
    }

    public UniTask ResetAsync(CancellationToken cancellationToken = default) => UniTask.CompletedTask;

    public UniTask<bool> NextPageAsync(CancellationToken cancellationToken = default) => UniTask.FromResult(false);

    public bool HasNextPage() => false;
}
```

## Good

```csharp
public class CarouselPaginator : IPaginator<Carousel>
{
    // (members are the same as Bad)
}
```

## Notes

- The rule applies to non-abstract classes that implement `TestHelper.UI.Paginators.IPaginator` directly, through an interface, or through a base class. Abstract classes are skipped; they are checked through their concrete subclasses.
- A class satisfies the rule when it implements `IPaginator<TComponent>` for any `TComponent`, including through a base class or through an interface that inherits `IPaginator<TComponent>` (e.g., a paginator author's `ICarouselPaginator : IPaginator<Carousel>`). A generic class that implements `IPaginator<T>` with its own type parameter `T` satisfies the rule too.
- An interface that inherits only `IPaginator` (e.g., `ICarouselPaginator : IPaginator`) does not satisfy the rule.
- The diagnostic is reported once at the class identifier (of the first declaration, for a partial class).

If you never rent the paginator by its target component, suppress the diagnostic at the class with `[SuppressMessage]`, or change the severity in `.editorconfig` or `.globalconfig`:

```editorconfig
dotnet_diagnostic.TestHelperUI4010.severity = suggestion
```
