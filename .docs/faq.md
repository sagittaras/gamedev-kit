# Frequently Asked Questions

## Why do you distribute packages instead of just sharing the source code?

The source code is available — the repository is open and forks are welcome. But distributed packages offer more than just files to copy.

Our development doesn't happen in Unity alone. The core libraries are pure .NET assemblies, usable outside of Unity by simply referencing the DLL in your `.csproj`. We distribute packages internally via NuGet — a public feed is something we'd like to open up in the future.

For Unity packages, we want to keep a clear domain boundary between the core logic and the Unity layer. This allows us to ship **Editor Tooling** alongside each package — tools that only make sense in the context of the Unity editor, and that raw source files simply can't provide.

And looking ahead — a pure C# core keeps the door open for **Godot** distribution. If we get there, nothing needs to be rearchitected.

## Can I use the packages outside of Unity?

Yes. The core packages are pure .NET assemblies — download the package archive from [GitHub Releases](https://github.com/sagittaras/gamedev-kit/releases), extract it and reference the DLL in your `.csproj` like any other assembly.

We don't have a public NuGet feed yet, but it's on our radar.

## Are packages versioned independently?

Yes. Every package has its own version and its own releases, tagged `<package>/<version>` (e.g. `dices/1.1.3`) with its own changelog. All releases are cut from the main branch. The [Latest Packages](https://github.com/sagittaras/gamedev-kit/releases/tag/latest) release gathers the newest version of every package in one place, under download links that never change.

The version is computed by [Nerdbank.GitVersioning](https://github.com/dotnet/Nerdbank.GitVersioning): `major.minor` comes from the package's own `version.json` and the patch number is the git height — the number of commits touching the package since `major.minor` last changed. Commits to a kit package it depends on count too, so e.g. a fix in `Sagittaras.GuardClauses` also bumps `Sagittaras.Dices`, whose archive bundles it. Patch numbers of consecutive releases therefore aren't sequential (e.g. `1.1.4` may follow `1.1.1`); a higher number is always newer.

Releases up to `1.2.1` predate this model — back then all packages were released together under one shared version.

## How do I know when a new version is released?

Watch the repository on GitHub and select **Releases only** — you'll get a notification whenever a new version is published.

## Can I contribute?

Contributions are welcome. A detailed contributing guide is on the way — in the meantime, feel free to open a PR and we'll take it from there.

## I found a bug. Where do I report it?

Open an issue on [GitHub Issues](https://github.com/sagittaras/gamedev-kit/issues). Include the package name, a description of the problem, and ideally a minimal reproduction case.

## Can I use this in a commercial project?

Yes. The project is licensed under the **Apache 2.0 License** — see [LICENSE](../LICENSE) for the full terms.