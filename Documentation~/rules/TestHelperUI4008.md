# TestHelperUI4008 — Constructor of IPaginator registered to PaginatorPool is not preserved

Detects a call to `PaginatorPool.Register<T>` whose type argument has a public constructor, which the pool may invoke, that is not preserved from managed code stripping. `PaginatorPool` invokes the constructor via reflection, so the Unity linker cannot see the call and can strip the constructor from the Player.

| Item     | Value         |
|----------|---------------|
| Category | Extensibility |
| Enabled  | True          |
| Severity | Warning       |
| CodeFix  | False         |

Message: "The public constructor of '{0}' is not preserved: managed code stripping can remove the constructor from the Player, and the paginator cannot be rented there. Apply 'Preserve' to the constructor."

`{0}` is the name of the registered type.

Severity is Warning, not Error, because the constructor is removed only in a Player build with a raised managed stripping level and only when no code calls it directly (e.g., `new CarouselPaginator(carousel)`), neither of which the analyzer can see, and a `link.xml` file can preserve it instead. The default level removes no user-written code (Disabled for Mono, Minimal for IL2CPP), and tests run only in the Editor are never affected. It is not Suggestion, because Unity does not show suggestions in the Console, and users who run tests on the Player should notice it before a build.

## Motivation

`PaginatorPool.Rent` creates paginator instances via `ConstructorInfo.Invoke` or `Activator.CreateInstance`. The Unity linker cannot detect these reflection calls, so at higher managed stripping levels it removes a constructor that no code calls directly: according to the [marking rules](https://docs.unity3d.com/Manual/managed-code-stripping-marking-rules.html), public members stop being roots at the High level, and at the Medium level for assemblies with types referenced in a scene. `Rent` then finds no public constructor, or `Activator.CreateInstance` throws `MissingMethodException`.

The [Unity Manual](https://docs.unity3d.com/Manual/managed-code-stripping-preserving.html) describes the `[Preserve]` attribute as a root annotation. Applying `[Preserve]` to a method (including a constructor) preserves the method, its declaring type, and the types of its arguments. Applying `[Preserve]` to a type preserves the type and its default constructor only, so it does not keep a constructor that has parameters, even when all of them have default values. Verified against the Unity 6000.4 Manual.

The same page states that the linker recognizes the attribute by name: "You can define the [Preserve] attribute in any assembly and in any namespace. You can use the PreserveAttribute class, create a subclass of it, or create your own class." The package's paginators and README use `UnityEngine.Scripting.PreserveAttribute`.

## Bad

In the examples, `Carousel`, `PagedDialog`, and `Swiper` are custom pageable components (`MonoBehaviour`s) of the game.

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

    public CarouselPaginator(Carousel carousel = null)
    {
        TargetComponent = carousel;
    }

    public UniTask ResetAsync(CancellationToken cancellationToken = default) => UniTask.CompletedTask;

    public UniTask<bool> NextPageAsync(CancellationToken cancellationToken = default) => UniTask.FromResult(false);

    public bool HasNextPage() => false;
}

public class PagedDialogPaginator : IPaginator<PagedDialog>
{
    // (members other than the constructor are omitted; the implicit default constructor is not preserved)
}

[Preserve]
public class SwiperPaginator : IPaginator<Swiper>
{
    // (members other than the constructor are omitted)

    // [Preserve] on the type keeps only the parameterless constructor
    public SwiperPaginator(Swiper swiper = null)
    {
        TargetComponent = swiper;
    }
}

var pool = new PaginatorPool()
    .Register<CarouselPaginator>()      // TestHelperUI4008
    .Register<PagedDialogPaginator>()   // TestHelperUI4008
    .Register<SwiperPaginator>();       // TestHelperUI4008
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

[Preserve]   // keeps the type and its implicit default constructor
public class PagedDialogPaginator : IPaginator<PagedDialog>
{
    // (members other than the constructor are omitted)
}

var pool = new PaginatorPool()
    .Register<CarouselPaginator>()
    .Register<PagedDialogPaginator>();
```

## Notes

- A constructor is preserved when either of the following holds:
    - The constructor has an attribute whose class is named `PreserveAttribute`, or derives (directly or indirectly) from a class named `PreserveAttribute`, in any namespace.
    - The constructor has no parameters and the class has such an attribute.
- The constructors checked depend on the arguments of the call:
    - Without constructor arguments (`Register<T>()`, `Register<T>(null)`, `Register<T>(default)`, or an empty array such as `Register<T>(new object[0])`): the single public constructor. TestHelperUI4006, TestHelperUI4009, and TestHelperUI4007 take precedence in this order, and this rule is reported only when none of them is.
    - With arguments written in the call (e.g., `Register<T>(0.2f)`): the public constructors that can possibly match the arguments, as TestHelperUI4012 determines. A constructor that certainly does not match is not checked. TestHelperUI4006 and TestHelperUI4012 take precedence.
    - With an array expression whose contents are unknown at compile time (e.g., `Register<T>(args)` with an `object[]` variable): every public constructor. TestHelperUI4006 takes precedence.
- The diagnostic is reported once per call, at `Register<T>`, even when multiple constructors are not preserved. See [TestHelperUI4006](TestHelperUI4006.md) for the calls that are not diagnosed.
- `[Preserve]` on a base class does not preserve the derived class or its constructors, because the attribute is not inherited.
- Non-public constructors are not checked because `PaginatorPool` does not invoke them.
- Preservation by `[assembly: Preserve]` or a `link.xml` file is not recognized; the analyzer cannot see `link.xml`, and `[assembly: Preserve]` is documented to preserve types, not their constructors. If you rely on them, change the severity in `.editorconfig` or `.globalconfig`:

```editorconfig
dotnet_diagnostic.TestHelperUI4008.severity = none
```
