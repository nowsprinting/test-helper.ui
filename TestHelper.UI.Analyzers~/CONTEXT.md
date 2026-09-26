# UI Test Helper Analyzers

Roslyn analyzers that diagnose code using the UI Test Helper package (`TestHelper.UI`). The vocabulary follows the package's [README](../README.md); the [Unity Manual](https://docs.unity3d.com/Manual/) is used for engine concepts the README does not define.

## Language

### Finding GameObjects

**GameObjectFinder**:
The class that finds a `GameObject` by name, path, or matcher, polling until it appears or the timeout elapses.
_Avoid_: finder API, search helper

**Matcher**:
An `IGameObjectMatcher` implementation passed to `GameObjectFinder.FindByMatcherAsync`. Its `ComponentType` narrows the candidate `GameObjects`, and `IsMatch` decides whether a candidate matches. Built-in: `ComponentMatcher`, `ButtonMatcher`, `ToggleMatcher`.
_Avoid_: filter, predicate

**Paginator**:
An `IPaginator` implementation that navigates pageable or scrollable UI components (e.g., `ScrollRect`) so that `GameObjectFinder` can find objects outside the viewport.
_Avoid_: pager, scroller

**Reachable**:
A `GameObject` is reachable when the user can hit it: `IReachableStrategy.IsReachable` returns true (by default, a raycast at its pivot or a fallback point hits it).
_Avoid_: visible, clickable

**Interactable**:
A component is interactable when the `IsInteractable` function returns true (by default, a uGUI-compatible component whose `interactable` property is true).
_Avoid_: enabled, active

### Operating GameObjects

**Operator**:
An `IOperator` implementation that performs one kind of operation (click, drag and drop, text input, ...) on a `GameObject` via `OperateAsync`. Every operator implements a sub-interface of `IOperator` (e.g., `IClickOperator`) that represents its kind.
_Avoid_: action, command, driver

**OperatorPool**:
The class that registers operator types, creates instances via reflection on their public constructor, and rents and returns them.

**Operator author**:
A user who implements a custom operator, matcher, or paginator to support a UI framework the built-ins do not cover. The Extensibility category targets this user.
_Avoid_: extender, plugin developer

### Monkey testing

**Monkey testing**:
Operating randomly selected interactable `GameObjects` with randomly selected operators, run by `Monkey.Run` with a `MonkeyConfig`.
_Avoid_: random testing, fuzzing

**Annotation component**:
A component in the `TestHelper.UI.Annotations` assembly attached to a `GameObject` to control the reachable strategy or monkey testing (e.g., `IgnoreAnnotation`, `DropAnnotation`).
_Avoid_: marker, tag

### Where code runs

**Runtime**:
Code compiled into a Player or run in Play mode, including Play mode tests. `TestHelper.UI` is a runtime assembly and does not depend on the Unity Test Framework.
_Avoid_: in-game, production code

**Player**:
A built executable of the project. Managed code stripping applies here, so constructors called only via reflection must be annotated with `[UnityEngine.Scripting.Preserve]`.
_Avoid_: player build, device

### Analyzer outcomes

**Unity standard API**:
An API in `UnityEngine` or `UnityEngine.*` that the package offers a replacement for (e.g., `GameObject.Find` vs. `GameObjectFinder`). The Alternative category targets these.
