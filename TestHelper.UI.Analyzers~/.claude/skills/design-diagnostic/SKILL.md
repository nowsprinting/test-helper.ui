---
name: design-diagnostic
description: Creates a specification file for a new diagnostic rule (../Documentation~/rules/<DIAGNOSTIC_ID>.md). Use when asked to add, specify, or propose a diagnostic rule.
argument-hint: "[spec]"
license: MIT
metadata:
  author: Koji Hasegawa
---

# Creating a Diagnostic Rule Specification File

## Steps

### 1. Assign a diagnostic ID

`TestHelperUI` + four digits. The first digit is fixed per category: 1 Usage, 2 Alternative, 3 Performance, 4 Extensibility (see "Diagnostics" in `CLAUDE.md`). Refer to the category tables under "Roslyn analyzers" in ../README.md for the existing IDs, and confirm with the user before using a number range for a new category.

### 2. Verify against primary sources

If the diagnostic targets APIs of the Unity engine or other packages, follow [resources/unity-references.md](resources/unity-references.md) to consult primary sources (official documentation and source code) and confirm signatures and behavior before writing the spec.

### 3. Create the specification file

Create `../Documentation~/rules/<DIAGNOSTIC_ID>.md` based on `assets/TEMPLATE.md` in this skill.

- Write everything in English
- CodeFix is always False (project-wide policy: no code fixes are provided)
- Follow the terminology below
- Link to the Unity Manual without a version number: `https://docs.unity3d.com/Manual/...`, not `https://docs.unity3d.com/6000.4/Documentation/Manual/...`
- In Markdown (`../Documentation~/rules/*.md` and ../README.md), write `<` and `>` outside backticks as `&lt;` and `&gt;`, e.g. the title `Task&lt;TResult&gt; is not supported ...`. Bare angle brackets are rendered as HTML tags in the browser and disappear
- Do not mention the host Unity project, e.g. "Unity 6000.4 (the version used by the host project)". The package is distributed independently; state only the package and version you verified against

#### Terminology

Follow the vocabulary in `CONTEXT.md` (e.g., operator, matcher, paginator, reachable, interactable, operator author).

| Use                                                              | Instead of                                   |
|------------------------------------------------------------------|----------------------------------------------|
| Apply `[Preserve]` to the constructor. (operation)               | place `[Preserve]` on, put `[Preserve]` on   |
| `[Preserve]` keeps the constructor from being stripped. (effect) | `[Preserve]` is present on the constructor   |

- Title and message: no backticks. Quote identifiers with single quotes, like Roslyn: `Type '{0}' owns disposable field(s)`
- Title and message name the APIs or types they target (e.g. "NameMatcher and PathMatcher are slow at runtime"), not a category such as "slow matchers"
- When the title enumerates the APIs or types it targets, the message outputs the one actually used in the code as `'{0}'`, e.g. `'{0}' is slow at runtime.` for `NameMatcher`
- The message states the consequence, especially for Error and Warning severity: one clause naming what happens, not the mechanism, e.g. "the operator cannot be rented", "managed code stripping can remove the constructor from the Player". Keep the details for the Motivation section
- When an alternative exists, the message ends with it as an imperative sentence, e.g. `Use 'GameObjectFinder' instead.` The title stays short and names only the problem

#### Choosing the default severity

Code that always leads to a runtime error, whatever the project settings and however the code is used, is an **Error**.
Code that leads to a runtime error only under conditions the analyzer cannot see is a **Warning**, e.g., an operator that `OperatorPool` cannot create (the analyzer cannot see whether the operator is used with `OperatorPool` or registered with constructor arguments), or a constructor that managed code stripping can remove (the analyzer cannot see the Player build settings).
Code that works but is slower or less reliable than the package's alternative is also a **Warning**.
Code that works and only deviates from the package's design policy is a **Suggestion**.

### 4. Update README.md

Add a row linking to the new file to the table of the corresponding category under "Roslyn analyzers" in ../README.md.
