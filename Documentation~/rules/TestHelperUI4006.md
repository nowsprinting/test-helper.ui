# TestHelperUI4006 — IPaginator implementation has no public constructor

Detects a concrete class implementing `IPaginator` whose constructors are all non-public. `PaginatorPool.Rent` creates paginators only through a public constructor, so it always throws when it has to create an instance of such a paginator.

| Item     | Value         |
|----------|---------------|
| Category | Extensibility |
| Enabled  | True          |
| Severity | Warning       |
| CodeFix  | False         |

Message: "'{0}' has no public constructor: the paginator cannot be rented. Make the constructor public."

`{0}` is the name of the paginator class.

Severity is Warning, not Error, because the paginator fails only when it is rented from `PaginatorPool`, and the analyzer cannot see whether the paginator is used with `PaginatorPool` or only created directly, e.g., by a factory method and passed to `GameObjectFinder.FindByMatcherAsync`. When it is rented, creating an instance throws on every path, with or without registered constructor arguments.

## Motivation

Without registered constructor arguments, `PaginatorPool.Rent` calls `Type.GetConstructors()`, which returns public instance constructors only, and throws `InvalidOperationException` ("{type} has no public constructor.") when the result is empty. With registered arguments, `Rent` calls `Activator.CreateInstance(Type, object[])`, which also binds public constructors only and throws `MissingMethodException`. All the `Rent` overloads, including `Rent(targetComponent)` that selects the paginator by its `IPaginator<TComponent>`, create instances this way.

A private or internal constructor is a natural choice to force construction through a factory method that takes the target component, but it makes the paginator unusable with `PaginatorPool`.

## Bad

In the examples, `Carousel` is a custom pageable component (a `MonoBehaviour`) of the game.

```csharp
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.UI.Paginators;
using UnityEngine;
using UnityEngine.Scripting;

public class CarouselPaginator : IPaginator<Carousel>   // TestHelperUI4006
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

    private CarouselPaginator() { }

    public static CarouselPaginator For(Carousel carousel) => new CarouselPaginator { TargetComponent = carousel };

    public UniTask ResetAsync(CancellationToken cancellationToken = default) => UniTask.CompletedTask;

    public UniTask<bool> NextPageAsync(CancellationToken cancellationToken = default) => UniTask.FromResult(false);

    public bool HasNextPage() => false;
}
```

## Good

```csharp
public class CarouselPaginator : IPaginator<Carousel>
{
    // (members other than the constructor are the same as Bad, without the For method)

    [Preserve]
    public CarouselPaginator(Carousel carousel = null)
    {
        TargetComponent = carousel;
    }
}
```

## Notes

- The rule applies to non-abstract classes that implement `TestHelper.UI.Paginators.IPaginator` directly, through `IPaginator<TComponent>`, or through a base class. Abstract classes are skipped because they cannot be instantiated.
- A class with no explicit constructor has a public compiler-generated default constructor and is not reported.
- Static constructors are not counted.
- Generic class definitions and nested classes are checked like any other class; a closed generic type such as `MyPaginator<int>` is rented through the same constructors.
- The diagnostic is reported once at the class identifier. For a partial class, it is reported at the identifier of the first declaration only.

If you never rent the paginator from `PaginatorPool`, suppress the diagnostic at the class with `[SuppressMessage]`, or change the severity in `.editorconfig` or `.globalconfig`:

```editorconfig
dotnet_diagnostic.TestHelperUI4006.severity = suggestion
```
