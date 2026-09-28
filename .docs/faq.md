# Frequently Asked Questions

- **[Distribution](#distribution)** — [compiled packages](#why-do-you-distribute-compiled-packages-instead-of-source-code),
  [outside of Unity](#can-i-use-the-packages-outside-of-unity)
- **[Unity](#unity)** — [NuGetForUnity](#can-i-install-the-nuget-packages-in-unity-with-nugetforunity),
  [package signature](#unity-says-the-package-is-signed-by-an-organization-i-dont-belong-to-is-that-a-problem)
- **[Versions & Releases](#versions--releases)** — [independent versions](#are-packages-versioned-independently),
  [missing older versions](#why-are-older-versions-missing-on-openupm-or-nuget),
  [new releases](#how-do-i-know-when-a-new-version-is-released)
- **[Community & License](#community--license)** — [contributing](#can-i-contribute),
  [bug reports](#i-found-a-bug-where-do-i-report-it), [commercial use](#can-i-use-this-in-a-commercial-project)

## Distribution

### Why do you distribute compiled packages instead of source code?

The source code is available — the repository is open and forks are welcome. But a distributed package offers more than files to copy: in Unity, each package is a **signed** Unity package on OpenUPM, so Unity can verify where it comes from, the Package Manager resolves its dependencies on other kit packages, and you see when an update is out. You don't lose the source either — the debug symbols carry Source Link, so your IDE steps into the package's code straight from GitHub.

Our development doesn't happen in Unity alone. The core libraries are pure .NET assemblies, compiled once and usable outside of Unity as NuGet packages — the very same assemblies the Unity packages carry.

The Unity package is a thin layer over that core: engine-specific parts live next to it, never inside it, so tooling for the Unity editor can grow per package without touching the core. And looking ahead — a pure C# core keeps the door open for **Godot** distribution. If we get there, nothing needs to be rearchitected.

### Can I use the packages outside of Unity?

Yes. The core packages are pure .NET Standard 2.1 assemblies, published on [NuGet](https://www.nuget.org/profiles/sagittaras) — add them with `dotnet add package Sagittaras.<Package>` and NuGet brings in the kit packages they depend on. If you'd rather not use NuGet, download the package archive from [GitHub Releases](https://github.com/sagittaras/gamedev-kit/releases), extract it and reference the DLL in your `.csproj` like any other assembly.

## Unity

### Can I install the NuGet packages in Unity with NuGetForUnity?

Yes, [NuGetForUnity](https://github.com/GlitchEnzo/NuGetForUnity) works — the NuGet packages carry the same assemblies as the Unity packages, and it resolves their dependencies on other kit packages as well. Keep in mind what you get, though: a NuGet package holds only the pure .NET core. Anything Unity-specific — editor tooling, runtime integrations with Unity — ships solely in the Unity packages on OpenUPM, as the packages grow their Unity layer. You also give up the Unity package signature, so Unity can't verify where the package comes from.

We recommend OpenUPM for Unity projects. If you go with NuGetForUnity, install each kit package from one source only — the same assembly from both NuGet and OpenUPM makes Unity report duplicate assemblies.

### Unity says the package is signed by an organization I don't belong to. Is that a problem?

No. Since Unity 6.3, the Package Manager checks package signatures, and our packages are signed by our Unity organization, so Unity can tell that a package really comes from us and hasn't been changed since. Only members of that organization see it as signed by their own organization — everyone else gets this notice, which is expected for any package signed by someone else.

The organization is named **Sagittaras Games**, but it started out as **Zechy** — so the first signed versions carry that name instead: GuardClauses `1.1.2`, Dices `1.1.2`, Messaging `1.1.2`, Progression `1.1.3` and Timing `1.1.3`. It is the same organization under its earlier name, and every later version is signed as Sagittaras Games.

## Versions & Releases

### Are packages versioned independently?

Yes. Every package has its own version and its own releases, tagged `<package>/<version>` (e.g. `dices/1.1.2`) with its own changelog. All releases are cut from the main branch, and each one publishes the same version to NuGet and OpenUPM.

The version is computed by [Nerdbank.GitVersioning](https://github.com/dotnet/Nerdbank.GitVersioning): `major.minor` comes from the package's own `version.json` and the patch number is the git height — the number of commits touching the package since `major.minor` last changed. Commits to a kit package it depends on count too, so e.g. a fix in `Sagittaras.GuardClauses` also bumps `Sagittaras.Dices`, which depends on it. Patch numbers of consecutive releases therefore aren't sequential (e.g. `1.1.4` may follow `1.1.1`); a higher number is always newer.

Releases up to `1.2.1` predate this model — back then all packages were released together under one shared version.

### Why are older versions missing on OpenUPM or NuGet?

The Unity packages and the NuGet feed came later than the packages themselves. Versions released before a package got its Unity package, or before the NuGet feed started, are available only as zip archives on [GitHub Releases](https://github.com/sagittaras/gamedev-kit/releases); every release since then is on OpenUPM and NuGet too.

### How do I know when a new version is released?

In Unity, the Package Manager window shows when a newer version of an installed package is available on OpenUPM. In a .NET project, your IDE's NuGet window lists the updates, or run `dotnet list package --outdated`. You can also watch the repository on GitHub and select **Releases only** — you'll get a notification whenever a new version of any package is published.

## Community & License

### Can I contribute?

Contributions are welcome. A detailed contributing guide is on the way — in the meantime, feel free to open a PR and we'll take it from there.

### I found a bug. Where do I report it?

Open an issue on [GitHub Issues](https://github.com/sagittaras/gamedev-kit/issues). Include the package name, a description of the problem, and ideally a minimal reproduction case.

### Can I use this in a commercial project?

Yes. The project is licensed under the **Apache 2.0 License** — see [LICENSE](../LICENSE) for the full terms.
