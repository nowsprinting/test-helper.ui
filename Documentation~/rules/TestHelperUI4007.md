# TestHelperUI4007 — Constructor parameter of IPaginator registered without arguments cannot be resolved

Detects a call to `PaginatorPool.Register<T>` without constructor arguments whose type argument has a public constructor parameter without a default value. `PaginatorPool.Rent` fills each parameter only with its default value, and throws `InvalidOperationException` for a parameter without one.

| Item     | Value         |
|----------|---------------|
| Category | Extensibility |
| Enabled  | True          |
| Severity | Error         |
| CodeFix  | False         |

Message: "Cannot resolve required parameter '{0}' of type {1}. Register with explicit constructor arguments or add a default value."

`{0}` is the parameter name, and `{1}` is the name of the parameter type (e.g., `Single`), the same wording as the exception that `Rent` throws.

Severity is Error because the paginator is registered without constructor arguments, so renting it always throws.

## Motivation

When the paginator is registered without constructor arguments (`Register<T>()`), `PaginatorPool.Rent` resolves each parameter of the single public constructor by its default value, and throws `InvalidOperationException` ("Cannot resolve required parameter '{name}' of type {type}. Register with explicit constructor arguments or add a default value.") for a parameter without one. Unlike `OperatorPool`, `PaginatorPool` has no values to inject.

The typical case is a constructor that requires the target component, since a paginator created directly takes it there (e.g., `new UguiScrollRectPaginator(scrollRect)`). `Rent` does not pass the target component to the constructor; it assigns the component via the `TargetComponent` setter after creating the instance, and `Register<T>(args)` must not include it. The built-in paginators give the target component parameter a default value of `null` for this reason, and the README requires the same of every paginator.

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
    private readonly float _pageDelaySeconds;
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
    public CarouselPaginator(Carousel carousel, float pageDelaySeconds)
    {
        TargetComponent = carousel;
        _pageDelaySeconds = pageDelaySeconds;
    }

    public UniTask ResetAsync(CancellationToken cancellationToken = default) => UniTask.CompletedTask;

    public UniTask<bool> NextPageAsync(CancellationToken cancellationToken = default) => UniTask.FromResult(false);

    public bool HasNextPage() => false;
}

var pool = new PaginatorPool()
    .Register<CarouselPaginator>();   // TestHelperUI4007 for carousel
```

## Good

```csharp
public class CarouselPaginator : IPaginator<Carousel>
{
    // (members other than the constructor are the same as Bad)

    [Preserve]
    public CarouselPaginator(Carousel carousel = null, float pageDelaySeconds = 0.2f)
    {
        TargetComponent = carousel;
        _pageDelaySeconds = pageDelaySeconds;
    }
}

var pool = new PaginatorPool()
    .Register<CarouselPaginator>();
```

## Notes

- The rule applies only to calls without constructor arguments: `Register<T>()`, `Register<T>(null)`, `Register<T>(default)`, and an empty array such as `Register<T>(new object[0])`.
- The rule applies only when the type argument has exactly one public constructor, which is the constructor `Rent` resolves parameters for. TestHelperUI4006 (no public constructor, or abstract) and TestHelperUI4009 (multiple public constructors) take precedence; when this rule is reported, TestHelperUI4008 is not reported for the call.
- Only the first parameter that cannot be resolved is reported.
- A `params` array parameter has no default value and is reported. `ref` and `out` parameters cannot have a default value and are reported.
- A parameter with only `[Optional]` and no `[DefaultParameterValue]` is reported: reflection reports no default value for it (`ParameterInfo.HasDefaultValue` is false), so `PaginatorPool` cannot resolve it either. A default value given by `[Optional, DefaultParameterValue(...)]` counts as a default value.
- The diagnostic is reported at `Register<T>` of the call. See [TestHelperUI4006](TestHelperUI4006.md) for the calls that are not diagnosed.
