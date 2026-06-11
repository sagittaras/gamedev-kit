# Frequently Asked Questions

## Why do you distribute packages instead of just sharing the source code?

The source code is available — the repository is open and forks are welcome. But distributed packages offer more than just files to copy.

Our development doesn't happen in Unity alone. The core libraries are pure .NET assemblies, usable outside of Unity by simply referencing the DLL in your `.csproj`. We distribute packages internally via NuGet — a public feed is something we'd like to open up in the future.

For Unity packages, we want to keep a clear domain boundary between the core logic and the Unity layer. This allows us to ship **Editor Tooling** alongside each package — tools that only make sense in the context of the Unity editor, and that raw source files simply can't provide.

And looking ahead — a pure C# core keeps the door open for **Godot** distribution. If we get there, nothing needs to be rearchitected.

## Can I use the packages outside of Unity?

Yes. The core packages are pure .NET assemblies — download the DLL from [GitHub Releases](https://github.com/sagittaras/gamedev-kit/releases) and reference it in your `.csproj` like any other assembly.

We don't have a public NuGet feed yet, but it's on our radar.

## Are packages versioned independently?

No — everything is distributed together through a single release on the main branch. Check [GitHub Releases](https://github.com/sagittaras/gamedev-kit/releases) for the latest version and changelog.

## How do I know when a new version is released?

Watch the repository on GitHub and select **Releases only** — you'll get a notification whenever a new version is published.

## Can I contribute?

Contributions are welcome. A detailed contributing guide is on the way — in the meantime, feel free to open a PR and we'll take it from there.

## I found a bug. Where do I report it?

Open an issue on [GitHub Issues](https://github.com/sagittaras/gamedev-kit/issues). Include the package name, a description of the problem, and ideally a minimal reproduction case.

## Can I use this in a commercial project?

Yes. The project is licensed under the **Apache 2.0 License** — see [LICENSE](../LICENSE) for the full terms.