# TestHelperUI4009 — IPaginator with multiple public constructors is registered without arguments

Detects a call to `PaginatorPool.Register<T>` without constructor arguments whose type argument declares more than one public constructor. `PaginatorPool.Rent` cannot choose a constructor for such a paginator and throws `InvalidOperationException`.

| Item     | Value         |
|----------|---------------|
| Category | Extensibility |
| Enabled  | True          |
| Severity | Error         |
| CodeFix  | False         |

Message: "'{0}' has multiple public constructors. Register with explicit constructor arguments."

`{0}` is the name of the registered type.

Severity is Error because renting a paginator registered this way always throws; the message is the same as the exception that `Rent` throws.

## Motivation

`PaginatorPool` creates paginator instances via reflection. When the paginator is registered without constructor arguments (`Register<T>()`), `Rent` calls `Type.GetConstructors()` and throws `InvalidOperationException` ("{type} has multiple public constructors. Register with explicit constructor arguments.") when it finds more than one.

The pool deliberately does not pick one of them (e.g., the first, or the one with the most parameters): `GetConstructors()` does not guarantee the order, and managed code stripping on the Player can remove some of them, so the chosen constructor could silently differ between the Editor and the Player. A paginator with a single public constructor whose parameters have default values works with both `Register<T>()` and `Register<T>(args)`, and with direct creation by `new`.

## Bad

In the examples, `Carousel` is a custom pageable component (a `MonoBehaviour`) of the game.

```csharp
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.UI;
using TestHelper.UI.Paginators;
using UnityEngine;
using UnityEngine.Scripting;

public class CarouselPaginator : IPaginator<Carousel>
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

var pool = new PaginatorPool()
    .Register<CarouselPaginator>();   // TestHelperUI4009
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

var pool = new PaginatorPool()
    .Register<CarouselPaginator>();
```

## Notes

- The rule applies only to calls without constructor arguments: `Register<T>()`, `Register<T>(null)`, and `Register<T>(default)`. A call with constructor arguments is diagnosed by TestHelperUI4012 when no public constructor matches them.
- Only public instance constructors are counted, as `Type.GetConstructors()` does. Non-public and static constructors are ignored.
- A type argument without any public constructor is diagnosed by TestHelperUI4006 instead. When this rule is reported, TestHelperUI4007 and TestHelperUI4008 are not reported for the call.
- The diagnostic is reported at `Register<T>` of the call. See [TestHelperUI4006](TestHelperUI4006.md) for the calls that are not diagnosed.
