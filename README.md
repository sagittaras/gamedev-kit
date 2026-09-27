<h1 align="center">Sagittaras Game Development Kit</h1>

<p align="center">
    Ready-made solutions for game development. Battle-tested by real games.
</p>

<p align="center">
    <a href="LICENSE"><img alt="GitHub License" src="https://img.shields.io/github/license/sagittaras/gamedev-kit?style=flat-square"></a>
    <a href="https://openupm.com/contributors/sagittaras/"><img alt="OpenUPM" src="https://img.shields.io/badge/OpenUPM-com.sagittaras.gamedevkit-3b84f6?style=flat-square"></a>
    <a href="https://github.com/sagittaras/gamedev-kit/actions/workflows/release.yml"><img alt="GitHub Actions Workflow Status" src="https://img.shields.io/github/actions/workflow/status/sagittaras/gamedev-kit/release.yml?style=flat-square"></a>
    <a href="https://github.com/sponsors/sagittaras"><img alt="Static Badge" src="https://img.shields.io/badge/GitHub%20Sponsors-Support-ea4aaa?style=flat-square&logo=githubsponsors&logoColor=white"></a>
</p>

<p align="center">
    <a href="https://bsky.app/profile/sagittaras.bsky.social"><img alt="Bluesky followers" src="https://img.shields.io/bluesky/followers/sagittaras.bsky.social?style=flat-square&label=Follow%20@sagittaras.bsky.social&color=%232d5d83"></a>
    <a href="https://mastodon.gamedev.place/@sagittaras"><img alt="Mastodon Follow" src="https://img.shields.io/mastodon/follow/110701089198897448?domain=mastodon.gamedev.place&style=flat-square&label=Follow%20@sagittaras&color=%236364FF"></a>
</p>

---

Every indie developer knows the situation. Problems and solutions that are often repeated from project to project.
And over time you realize that other people are solving the same problems. And these solutions are often rewritten
for each next project.

Our own development is no exception. Whether it is Spellborn, Vectro Blast, our our Game Jam projects. We have gradually
grown our library of ready-made solutions. Approaches to architecture, utilities, or systems that we are just reusing. We believe
we can share those solutions with other developers.

**Game Development Kit** is where we share these solutions. Whether you are looking for something ready-made, want to see how
others approach certain problems, or just looking for inspiration on your own library development.

We believe that game development deserves much more openness! 💙

## Getting Started

Every package is a Unity package on **[OpenUPM](https://openupm.com)**, versioned and released on its own. Unity's
Package Manager then takes care of updates and of the dependencies between the packages. The packages are signed by
Sagittaras Games and require Unity 2021.3 or newer.

| Package | Unity package | Version |
| --- | --- | --- |
| Sagittaras.GuardClauses | [com.sagittaras.gamedevkit.guard-clauses](https://openupm.com/packages/com.sagittaras.gamedevkit.guard-clauses/) | [![openupm](https://img.shields.io/npm/v/com.sagittaras.gamedevkit.guard-clauses?label=openupm&registry_uri=https%3A%2F%2Fpackage.openupm.com&style=flat-square)](https://openupm.com/packages/com.sagittaras.gamedevkit.guard-clauses/) |
| Sagittaras.Dices | [com.sagittaras.gamedevkit.dices](https://openupm.com/packages/com.sagittaras.gamedevkit.dices/) | [![openupm](https://img.shields.io/npm/v/com.sagittaras.gamedevkit.dices?label=openupm&registry_uri=https%3A%2F%2Fpackage.openupm.com&style=flat-square)](https://openupm.com/packages/com.sagittaras.gamedevkit.dices/) |
| Sagittaras.Timing | [com.sagittaras.gamedevkit.timing](https://openupm.com/packages/com.sagittaras.gamedevkit.timing/) | [![openupm](https://img.shields.io/npm/v/com.sagittaras.gamedevkit.timing?label=openupm&registry_uri=https%3A%2F%2Fpackage.openupm.com&style=flat-square)](https://openupm.com/packages/com.sagittaras.gamedevkit.timing/) |
| Sagittaras.Messaging | [com.sagittaras.gamedevkit.messaging](https://openupm.com/packages/com.sagittaras.gamedevkit.messaging/) | [![openupm](https://img.shields.io/npm/v/com.sagittaras.gamedevkit.messaging?label=openupm&registry_uri=https%3A%2F%2Fpackage.openupm.com&style=flat-square)](https://openupm.com/packages/com.sagittaras.gamedevkit.messaging/) |
| Sagittaras.Progression | [com.sagittaras.gamedevkit.progression](https://openupm.com/packages/com.sagittaras.gamedevkit.progression/) | [![openupm](https://img.shields.io/npm/v/com.sagittaras.gamedevkit.progression?label=openupm&registry_uri=https%3A%2F%2Fpackage.openupm.com&style=flat-square)](https://openupm.com/packages/com.sagittaras.gamedevkit.progression/) |

Install a package with [openupm-cli](https://github.com/openupm/openupm-cli):

```bash
openupm add com.sagittaras.gamedevkit.dices
```

Or add the OpenUPM scoped registry once in **Project Settings → Package Manager → Scoped Registries** — name
`OpenUPM`, URL `https://package.openupm.com`, scope `com.sagittaras.gamedevkit` — and install the packages from
**My Registries** in the Package Manager window.

### Without the Package Manager

Every version also has its own **[GitHub Release](https://github.com/sagittaras/gamedev-kit/releases)**, tagged
`<package>/<version>` (e.g. `dices/1.1.3`). Besides the Unity package, it carries a zip archive with the compiled
`.dll`, its `.pdb` (debug symbols) and `.xml` (IntelliSense documentation), plus the DLLs of the kit packages it
depends on — reference them from a plain .NET project, or extract the archive under `Assets/Plugins/` in Unity.

> Don't install the same package both ways — an assembly from OpenUPM must not also sit in `Assets/Plugins/`,
> otherwise Unity reports duplicate assemblies.

## What's Inside

<a href="./.docs/guard-clauses/index.md"><img src="https://img.shields.io/badge/Sagittaras.GuardClauses-darkgreen?style=flat-square" alt="Sagittaras.GuardClauses"/></a>
<a href="./.docs/dices/index.md"><img src="https://img.shields.io/badge/Sagittaras.Dices-blueviolet?style=flat-square" alt="Sagittaras.Dices"/></a>
<a href="./.docs/timing/index.md"><img src="https://img.shields.io/badge/Sagittaras.Timing-blue?style=flat-square" alt="Sagittaras.Timing"/></a>
<a href="./.docs/messaging/index.md"><img src="https://img.shields.io/badge/Sagittaras.Messaging-crimson?style=flat-square" alt="Sagittaras.Messaging"/></a>
<a href="./.docs/progression/index.md"><img src="https://img.shields.io/badge/Sagittaras.Progression-orange?style=flat-square" alt="Sagittaras.Progression"/></a>

See [Docs](.docs/index.md) for what's inside and how to use it.

## What's Comming

We are creating the Development Kit as a central place for all our packages. We gradually select them from our
projects, clean them up and further develop them. However, this process is primarily based on the needs of our
own development.

Below you will find an overview of the packages we are currently working on or plan to share -
without a fixed release schedule.

### 🔲 Grid

Coordinate system, cell properties, entity movement tracking. Even though we run the entire Vectro Blast on it,
it is a highly portable abstraction that anyone can use!

### ✅ Condition System

A general-purpose constraint system for validating conditions in the context of the current state of an entity.

It was created for validating spells in Spellborn — it turned out that the applicability without domain binding is endless.

### 📜 Scripting System

Are you splitting your project into domain-specific packages and looking for a way to open up certain places for external scripting?

We were looking for it too, and you don't have to! Define hook-points, prepare a base-script — the framework takes care of the rest.

## License

Apache 2.0 License. See [LICENSE](LICENSE) for details.

---

## FAQ
See [FAQ](.docs/faq.md) in documentation.
