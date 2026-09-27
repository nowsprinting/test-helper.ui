# TestHelperUI4008 — Public constructor of IPaginator implementation is not preserved

Detects a public constructor of a concrete class implementing `IPaginator` that is not preserved from managed code stripping. `PaginatorPool` invokes the constructor via reflection, so the Unity linker cannot see the call and can strip the constructor from the Player.

| Item     | Value         |
|----------|---------------|
| Category | Extensibility |
| Enabled  | True          |
| Severity | Warning       |
| CodeFix  | False         |

Message: "The public constructor of '{0}' is not preserved: managed code stripping can remove the constructor from the Player, and the paginator cannot be rented there. Apply 'Preserve' to the constructor."

`{0}` is the name of the paginator class.

Severity is Warning, not Error, because the constructor is removed only in a Player build with a raised managed stripping level and only when no code calls it directly (e.g., `new CarouselPaginator(carousel)`), neither of which the analyzer can see. The default level removes no user-written code (Disabled for Mono, Minimal for IL2CPP), and tests run only in the Editor are never affected. It is not Suggestion, because Unity does not show suggestions in the Console, and authors who run tests on the Player should notice it before a build.

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

    public CarouselPaginator(Carousel carousel = null)   // TestHelperUI4008
    {
        TargetComponent = carousel;
    }

    public UniTask ResetAsync(CancellationToken cancellationToken = default) => UniTask.CompletedTask;

    public UniTask<bool> NextPageAsync(CancellationToken cancellationToken = default) => UniTask.FromResult(false);

    public bool HasNextPage() => false;
}

public class PagedDialogPaginator : IPaginator<PagedDialog>   // TestHelperUI4008; the implicit default constructor is not preserved
{
    // (members other than the constructor are omitted)
}

[Preserve]
public class SwiperPaginator : IPaginator<Swiper>
{
    // (members other than the constructor are omitted)

    public SwiperPaginator(Swiper swiper = null)   // TestHelperUI4008; [Preserve] on the type keeps only the parameterless constructor
    {
        TargetComponent = swiper;
    }
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

[Preserve]   // keeps the type and its implicit default constructor
public class PagedDialogPaginator : IPaginator<PagedDialog>
{
    // (members other than the constructor are omitted)
}
```

## Notes

- The rule applies to non-abstract classes that implement `TestHelper.UI.Paginators.IPaginator` directly, through `IPaginator<TComponent>`, or through a base class. Abstract classes are skipped because they cannot be instantiated.
- A constructor is preserved when either of the following holds:
    - The constructor has an attribute whose class is named `PreserveAttribute`, or derives (directly or indirectly) from a class named `PreserveAttribute`, in any namespace.
    - The constructor has no parameters and the class has such an attribute.
- Every public constructor is checked; when a class has multiple public constructors, each unpreserved one gets its own diagnostic (TestHelperUI4009 is reported separately).
- The diagnostic is reported at the constructor identifier. For a class with no explicit constructor, the compiler-generated default constructor cannot take an attribute, so the diagnostic is reported at the class identifier (of the first declaration, for a partial class); apply `[Preserve]` to the class, or declare the constructor explicitly and apply `[Preserve]` to it.
- `[Preserve]` on a base class does not preserve the derived class or its constructors, because the attribute is not inherited.
- Non-public constructors are not checked because `PaginatorPool` does not invoke them.
- Preservation by `[assembly: Preserve]` or a `link.xml` file is not recognized; the analyzer cannot see `link.xml`, and `[assembly: Preserve]` is documented to preserve types, not their constructors. If you rely on them, or never rent the paginator from `PaginatorPool`, change the severity in `.editorconfig` or `.globalconfig`:

```editorconfig
dotnet_diagnostic.TestHelperUI4008.severity = none
```
