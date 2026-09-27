# TestHelperUI4006 — IPaginator registered to PaginatorPool has no usable public constructor

Detects a call to `PaginatorPool.Register<T>` whose type argument is abstract or has no public constructor. `PaginatorPool.Rent` creates paginators only through a public constructor, so it always throws when it has to create an instance of such a paginator.

| Item     | Value         |
|----------|---------------|
| Category | Extensibility |
| Enabled  | True          |
| Severity | Error         |
| CodeFix  | False         |

Message: "'{0}' {1}"

`{0}` is the name of the registered type, and `{1}` is "is abstract" for an abstract class, or "has no public constructor" otherwise.

Severity is Error because renting the registered paginator throws on every path, with or without registered constructor arguments.

## Motivation

Without registered constructor arguments, `PaginatorPool.Rent` calls `Type.GetConstructors()`, which returns public instance constructors only, and throws `InvalidOperationException` ("{type} has no public constructor.") when the result is empty. With registered arguments, `Rent` calls `Activator.CreateInstance(Type, object[])`, which also binds public constructors only and throws `MissingMethodException`. For an abstract class, the constraint `where T : class, IPaginator` accepts the type, but creating an instance throws `MemberAccessException`. All the `Rent` overloads, including `Rent(targetComponent)` that selects the paginator by its `IPaginator<TComponent>`, create instances this way.

A private or internal constructor is a natural choice to force construction through a factory method that takes the target component, but it makes the paginator unusable with `PaginatorPool`.

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

    private CarouselPaginator() { }

    public static CarouselPaginator For(Carousel carousel) => new CarouselPaginator { TargetComponent = carousel };

    public UniTask ResetAsync(CancellationToken cancellationToken = default) => UniTask.CompletedTask;

    public UniTask<bool> NextPageAsync(CancellationToken cancellationToken = default) => UniTask.FromResult(false);

    public bool HasNextPage() => false;
}

var pool = new PaginatorPool()
    .Register<CarouselPaginator>();   // TestHelperUI4006
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

var pool = new PaginatorPool()
    .Register<CarouselPaginator>();
```

## Notes

- The rule is checked first for every `Register<T>` call, regardless of the arguments. When it is reported, no other rule is reported for the call.
- An interface type argument has no constructor and is reported with "has no public constructor".
- A class with no explicit constructor has a public compiler-generated default constructor and is not reported.
- Static constructors are not counted.
- The diagnostic is reported at `Register<T>` of the call, also when the call is made through a class derived from `PaginatorPool`, through `?.`, or in a fluent chain.
- A call whose type argument is a generic type parameter is not diagnosed, because the actual type is unknown.
- `Rent<T>()`, `Rent(Type)`, and `Rent(targetComponent)` calls are not diagnosed, so renting an unregistered paginator from a pool created with `requireRegistration: false` is not detected.
- A paginator declaration that is never registered is not diagnosed, including a paginator only created directly with `new`. A library that only declares paginators gets no diagnostic until a user registers them.
