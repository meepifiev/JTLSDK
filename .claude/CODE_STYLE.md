# CODE_STYLE.md

## Principle

Code reads by names and structure. If a class, method or field name does not say why it exists, improve the name before implementing. Over-engineering is a defect: patterns only where they simplify maintenance, testing or extension.

## Naming

- `PascalCase`: classes, structs, interfaces, enums, properties, methods, events, public and protected members, `static`, `readonly`, `const` fields.
- `camelCase`: parameters and locals.
- `_camelCase`: private and internal fields.
- No abbreviations: `Initialize`, not `Init`; `configuration`, not `config`.
- Interfaces start with `I`. Enum members without prefixes.
- Namespaces in `PascalCase` with dots, matching the folder under `Runtime` or `Editor`: `JTLStudio.SDK`, `JTLStudio.SDK.Providers`, `JTLStudio.SDK.Editor.Toolkit`.

## Types

- Always write the access modifier.
- One file, one main type; the file name equals the type name.
- `sealed` is not used.
- Role suffixes (`Service`, `Provider`, `View`) only where the type plays that role.
- Public fields only in data-only structs.

## Members

- Auto-properties when there is no logic; expression-bodied properties for read-only access to a field.
- Methods start with a verb: `Create`, `Show`, `Load`, `Apply`, `Reset`.
- Static methods are not used, except in the `JTLSDK` facade, `[MonoPInvokeCallback]` trampolines of the bridge, and editor entry points that Unity requires to be static (`[InitializeOnLoad]`, `[MenuItem]`). An entry point only creates an instance and hands over to it.
- Member order: constants and static fields, serialized fields, fields, delegates, constructors, events, properties, Unity lifecycle, public methods, private methods, event handlers. Within a group: `public`, `internal`, `protected`, `private`.

## Events and callbacks

- C# `event` for notifications; never public `Action` fields.
- `-ed` for finished actions (`Loaded`, `Changed`), handlers start with `On`.
- Subscribe and unsubscribe symmetrically.
- Callbacks passed into methods are named `on<Result>` (`onResult`, `onLoaded`) and are invoked exactly once.

## Layout

- Braces on their own lines, always, even for one-line bodies.
- Explicit negation: `if (flag == false)`, not `if (!flag)`.
- A blank line separates conditionals and loops from neighbouring blocks.
- `var` only when the type is obvious on the right; explicit types preferred.
- Magic numbers go into `const` fields; `0` and `1` are allowed inline.

## Unity

- MonoBehaviour is a view, binding or lifecycle adapter; logic lives in plain classes.
- No `FindObjectOfType`, `GameObject.Find`, `Camera.main` in runtime code.
- Serialized fields are private with `[SerializeField]`.
- ScriptableObjects hold authoring-time configuration only, never runtime state.
- Editor UI is UI Toolkit (UXML and USS). IMGUI only where UI Toolkit has no equivalent.

## Errors

- Invalid arguments and states throw (`ArgumentNullException`, `ArgumentOutOfRangeException`, `InvalidOperationException`) with `nameof(...)` only.
- Expected platform failures are results (`AdResult.Failed`, `PurchaseResult.Failed`), never exceptions and never silent defaults.
- Logging goes through the SDK logger with the configured level; no `Debug.Log` sprinkled in code.

## Comments

- No comments in code. Intent is expressed by names and structure. Public XML documentation is written only when the API is frozen for a release.

## Forbidden

- `sealed`, `Manager`, `Controller` as dumping grounds, god objects, public mutable fields, magic numbers, hidden singletons, copy-paste logic, `while (true)` without a visible exit, `async void`.
