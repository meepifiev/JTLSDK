# AGENT.md

## Purpose

Rules for the agent working on the JTL SDK repository. The package lives in `Packages/com.jtlstudio.sdk`; everything else in the project exists to develop, test and demo it.

## Source of truth

- `Docs/Specification.md` defines names, module contracts, behaviour, platform mappings and the toolkit. Code follows the specification; when they disagree, the specification is either fixed first or the deviation is written into its decision log (section 17).
- The public API is the static facade `JTLSDK` and the module interfaces. Changing a public signature requires updating the specification in the same change.

## Project facts

- Minimum Unity 2021.3.18f1. Development on 2021.3.45f2. Do not use APIs that appeared later.
- Only WebGL and the Editor are targets.
- No external dependencies in the runtime: no UniTask, no Newtonsoft, no DOTween. Async is callback-based.
- The runtime has exactly one MonoBehaviour (the hidden `JTLSDK` object) and exactly one static class (`JTLSDK`). Everything else is instance-based.
- Platform code compiles always; `[DllImport]` bodies are gated by `JTLSDK_<PLATFORM> && UNITY_WEBGL && !UNITY_EDITOR`. `.jslib` files get define constraints through `PluginImporter`.
- The JS bridge is TypeScript in `Bridge~`, built into `Runtime/Plugins/WebGL/jtlsdk.jspre`. Never edit the generated file by hand.

## Layers

```text
JTLSDK facade -> module interfaces -> common services -> provider interfaces -> platform providers -> bridge
```

- Common services hold every rule that is the same on all platforms.
- Providers hold only platform calls and platform-specific settings.
- Prototype providers (Editor) go through the same services as real platforms.

## Git

- Commit after each completed step with a message that names the step. Push to `main`.
- Never commit `.DS_Store`, `Library`, `Logs`, `UserSettings`, imported samples under `Assets/Samples`.

## Before answering

- Say which files changed and which checks ran (compilation in batch mode, EditMode tests).
- Do not claim something is tested when it is not.
