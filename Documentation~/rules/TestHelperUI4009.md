# TestHelperUI4009 — IPaginator implementation has multiple public constructors

Detects a concrete class implementing `IPaginator` that declares more than one public constructor. `PaginatorPool.Rent` cannot choose a constructor for such a paginator and throws `InvalidOperationException` unless the paginator is registered with explicit constructor arguments.

| Item     | Value         |
|----------|---------------|
| Category | Extensibility |
| Enabled  | True          |
| Severity | Warning       |
| CodeFix  | False         |

Message: "'{0}' has multiple public constructors: the paginator cannot be rented unless it is registered with constructor arguments. Keep only one public constructor."

`{0}` is the name of the paginator class.

Severity is Warning, not Error, because registering the paginator with explicit constructor arguments via `PaginatorPool.Register<T>(args)` is a documented way to use such a paginator, and the analyzer cannot see the registration, nor whether the paginator is rented from `PaginatorPool` at all.

## Motivation

`PaginatorPool` creates paginator instances via reflection. When the paginator is registered without constructor arguments (`Register<T>()`), or is rented from a pool created with `requireRegistration: false` without being registered, `Rent` calls `Type.GetConstructors()` and throws `InvalidOperationException` ("{type} has multiple public constructors. Register with explicit constructor arguments.") when it finds more than one.

The pool deliberately does not pick one of them (e.g., the first, or the one with the most parameters): `GetConstructors()` does not guarantee the order, and managed code stripping on the Player can remove some of them, so the chosen constructor could silently differ between the Editor and the Player. A paginator with a single public constructor whose parameters have default values works with both `Register<T>()` and `Register<T>(args)`, and with direct creation by `new`.

## Bad

In the examples, `Carousel` is a custom pageable component (a `MonoBehaviour`) of the game.

```csharp
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.UI.Paginators;
using UnityEngine;
using UnityEngine.Scripting;

public class CarouselPaginator : IPaginator<Carousel>   // TestHelperUI4009
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
    public CarouselPaginator() { }

    [Preserve]
    public CarouselPaginator(Carousel carousel)
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
    // (members other than the constructor are the same as Bad)

    [Preserve]
    public CarouselPaginator(Carousel carousel = null)
    {
        TargetComponent = carousel;
    }
}
```

## Notes

- The rule applies to non-abstract classes that implement `TestHelper.UI.Paginators.IPaginator` directly, through `IPaginator<TComponent>`, or through a base class. Abstract classes are skipped because they cannot be instantiated.
- Only constructors declared in the class are counted, as `Type.GetConstructors()` does. Non-public constructors are ignored.
- The diagnostic is reported once at the class identifier (of the first declaration, for a partial class).
- A paginator without any public constructor is diagnosed by TestHelperUI4006 instead.

If you register the paginator with explicit constructor arguments, or never rent it from `PaginatorPool`, suppress the diagnostic at the class with `[SuppressMessage]`, or change the severity in `.editorconfig` or `.globalconfig`:

```editorconfig
dotnet_diagnostic.TestHelperUI4009.severity = suggestion
```
