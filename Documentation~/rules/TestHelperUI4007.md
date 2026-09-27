# TestHelperUI4007 — Constructor parameter of IPaginator implementation has no default value

Detects a parameter without a default value in the public constructor of a concrete class implementing `IPaginator`. When the paginator is registered without constructor arguments, `PaginatorPool.Rent` fills each parameter only with its default value, and throws `InvalidOperationException` for a parameter without one.

| Item     | Value         |
|----------|---------------|
| Category | Extensibility |
| Enabled  | True          |
| Severity | Warning       |
| CodeFix  | False         |

Message: "Parameter '{0}' of the '{1}' constructor has no default value: the paginator cannot be rented unless it is registered with constructor arguments. Add a default value."

`{0}` is the parameter name, and `{1}` is the name of the paginator class.

Severity is Warning, not Error, because the paginator works when it is registered with explicit constructor arguments via `PaginatorPool.Register<T>(args)`, or when it is only created directly with `new`; the analyzer cannot see either.

## Motivation

When the paginator is registered without constructor arguments (`Register<T>()`), or is rented from a pool created with `requireRegistration: false` without being registered, `PaginatorPool.Rent` resolves each parameter of the single public constructor by its default value, and throws `InvalidOperationException` ("Cannot resolve required parameter '{name}' of type {type}. Register with explicit constructor arguments or add a default value.") for a parameter without one. Unlike `OperatorPool`, `PaginatorPool` has no values to inject.

The typical case is a constructor that requires the target component, since a paginator created directly takes it there (e.g., `new UguiScrollRectPaginator(scrollRect)`). `Rent` does not pass the target component to the constructor; it assigns the component via the `TargetComponent` setter after creating the instance, and `Register<T>(args)` must not include it. The built-in paginators give the target component parameter a default value of `null` for this reason, and the README requires the same of every paginator.

## Bad

In the examples, `Carousel` is a custom pageable component (a `MonoBehaviour`) of the game.

```csharp
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
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
    public CarouselPaginator(Carousel carousel, float pageDelaySeconds)   // TestHelperUI4007 at carousel and pageDelaySeconds
    {
        TargetComponent = carousel;
        _pageDelaySeconds = pageDelaySeconds;
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
    public CarouselPaginator(Carousel carousel = null, float pageDelaySeconds = 0.2f)
    {
        TargetComponent = carousel;
        _pageDelaySeconds = pageDelaySeconds;
    }
}
```

## Notes

- The rule applies to non-abstract classes that implement `TestHelper.UI.Paginators.IPaginator` directly, through `IPaginator<TComponent>`, or through a base class. Abstract classes are skipped because they cannot be instantiated.
- The rule applies only when the class has exactly one public constructor, which is the constructor `Rent` resolves parameters for. A class with multiple public constructors is diagnosed by TestHelperUI4009 instead.
- A `params` array parameter has no default value and is reported. `ref` and `out` parameters cannot have a default value and are reported.
- A parameter with only `[Optional]` and no `[DefaultParameterValue]` is reported: reflection reports no default value for it (`ParameterInfo.HasDefaultValue` is false), so `PaginatorPool` cannot resolve it either. A default value given by `[Optional, DefaultParameterValue(...)]` counts as a default value.
- The diagnostic is reported at each parameter identifier.

If you always register the paginator with explicit constructor arguments, or never rent it from `PaginatorPool`, suppress the diagnostic with `[SuppressMessage]`, or change the severity in `.editorconfig` or `.globalconfig`:

```editorconfig
dotnet_diagnostic.TestHelperUI4007.severity = suggestion
```
