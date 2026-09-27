# TestHelperUI4012 — No public constructor of IPaginator matches the arguments of PaginatorPool.Register

Detects a call to `PaginatorPool.Register<T>` with constructor arguments that no public constructor of the type argument can accept. `PaginatorPool.Rent` creates the paginator with `Activator.CreateInstance` and the registered arguments, so it throws `MissingMethodException`.

| Item     | Value         |
|----------|---------------|
| Category | Extensibility |
| Enabled  | True          |
| Severity | Error         |
| CodeFix  | False         |

Message: "No public constructor of '{0}' matches the arguments"

`{0}` is the name of the registered type.

Severity is Error because the rule reports only when the arguments certainly do not match any public constructor, so renting the paginator always throws.

## Motivation

When the paginator is registered with constructor arguments (`Register<T>(args)`), `PaginatorPool.Rent` passes them to `Activator.CreateInstance(Type, object[])`, which binds only public constructors by the number and runtime types of the arguments. The arguments are stored as `object[]`, so the compiler does not check them against the constructors, and a mismatch shows up only when the paginator is rented.

A typical mistake is to omit the target component parameter from the arguments. `Rent` assigns the target component via the `TargetComponent` setter after creating the instance, so `Register<T>(args)` must not include the component itself, but it still has to fill the constructor parameter in its position, e.g., with `null`.

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
    public CarouselPaginator(Carousel carousel = null, float pageDelaySeconds = 0.2f)
    {
        TargetComponent = carousel;
        _pageDelaySeconds = pageDelaySeconds;
    }

    public UniTask ResetAsync(CancellationToken cancellationToken = default) => UniTask.CompletedTask;

    public UniTask<bool> NextPageAsync(CancellationToken cancellationToken = default) => UniTask.FromResult(false);

    public bool HasNextPage() => false;
}

var pool = new PaginatorPool()
    .Register<CarouselPaginator>(0.5f);   // TestHelperUI4012
```

## Good

```csharp
var pool = new PaginatorPool()
    .Register<CarouselPaginator>(null, 0.5f);
```

## Notes

- The rule applies only to calls with arguments written in the call (e.g., `Register<T>(null, 0.5f)`). A call without constructor arguments is diagnosed by TestHelperUI4007 and TestHelperUI4009, and a call with an array expression (e.g., an `object[]` variable) is not checked by this rule.
- TestHelperUI4006 (no public constructor, or abstract) takes precedence. When this rule is reported, TestHelperUI4008 is not reported for the call.
- Reproducing the binding rules of the runtime is avoided because they differ between Mono and IL2CPP. The rule reports only when every public constructor certainly does not match:
    - A constructor whose last parameter is a `params` array can always match.
    - Any other constructor can match when the number of arguments is not more than the number of parameters, and no argument is certainly incompatible with its parameter.
    - An argument is certainly incompatible when it is the `null` literal and the parameter is a non-nullable value type, or when its static type is a struct, an enum, or a sealed class (so the runtime type is the static type) and there is no identity, implicit reference, boxing, implicit numeric, or implicit nullable conversion to the parameter type. User-defined implicit conversions do not count, because reflection does not call `op_Implicit`.
    - An argument whose static type is an interface, a non-sealed class, or `object` can always match, because its runtime type is unknown.
    - An enum argument for an integral parameter can always match, because the runtime binders differ on it.
- As a result, some calls that throw at runtime are not reported, e.g., an interface-typed argument that the parameter does not accept, or fewer arguments than parameters (`Activator.CreateInstance` does not fill omitted parameters with their default values). Pass an argument for every constructor parameter, as in Good.
- The diagnostic is reported at `Register<T>` of the call. See [TestHelperUI4006](TestHelperUI4006.md) for the calls that are not diagnosed.
