<h1 align="center">Sagittaras Game Development Kit</h1>

<p align="center">
    Ready-made solutions for game development. Battle-tested by real games.
</p>

<p align="center">
    <a href="LICENSE"><img alt="GitHub License" src="https://img.shields.io/github/license/sagittaras/gamedev-kit?style=flat-square"></a>
    <a href="https://openupm.com/contributors/sagittaras/"><img alt="OpenUPM" src="https://img.shields.io/badge/OpenUPM-com.sagittaras.gamedevkit-3b84f6?style=flat-square"></a>
    <a href="https://www.nuget.org/profiles/sagittaras"><img alt="NuGet" src="https://img.shields.io/badge/NuGet-Sagittaras-004880?style=flat-square&logo=nuget&logoColor=white"></a>
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

The packages are signed Unity packages on [OpenUPM](https://openupm.com) requiring Unity 2021.3 or newer, and
.NET Standard 2.1 packages on [NuGet](https://www.nuget.org/profiles/sagittaras) for any other .NET project. See
[Installation](.docs/index.md#installation) for adding them to your project — through the Unity
[Package Manager](.docs/index.md#package-manager), [NuGet](.docs/index.md#nuget), or
[without either](.docs/index.md#without-a-package-manager).

Each package name leads to its documentation, and [Docs](.docs/index.md) gives an overview of all of them with examples.

| Package | OpenUPM | NuGet |
| --- | --- | --- |
| [Sagittaras Guard Clauses](.docs/guard-clauses/index.md) | [![openupm](https://img.shields.io/npm/v/com.sagittaras.gamedevkit.guard-clauses?label=openupm&registry_uri=https%3A%2F%2Fpackage.openupm.com&style=flat-square)](https://openupm.com/packages/com.sagittaras.gamedevkit.guard-clauses/) | [![nuget](https://img.shields.io/nuget/v/Sagittaras.GuardClauses?label=nuget&style=flat-square)](https://www.nuget.org/packages/Sagittaras.GuardClauses/) |
| [Sagittaras Dice Rolling](.docs/dices/index.md) | [![openupm](https://img.shields.io/npm/v/com.sagittaras.gamedevkit.dices?label=openupm&registry_uri=https%3A%2F%2Fpackage.openupm.com&style=flat-square)](https://openupm.com/packages/com.sagittaras.gamedevkit.dices/) | [![nuget](https://img.shields.io/nuget/v/Sagittaras.Dices?label=nuget&style=flat-square)](https://www.nuget.org/packages/Sagittaras.Dices/) |
| [Sagittaras Timing API](.docs/timing/index.md) | [![openupm](https://img.shields.io/npm/v/com.sagittaras.gamedevkit.timing?label=openupm&registry_uri=https%3A%2F%2Fpackage.openupm.com&style=flat-square)](https://openupm.com/packages/com.sagittaras.gamedevkit.timing/) | [![nuget](https://img.shields.io/nuget/v/Sagittaras.Timing?label=nuget&style=flat-square)](https://www.nuget.org/packages/Sagittaras.Timing/) |
| [Sagittaras Mediator](.docs/messaging/index.md) | [![openupm](https://img.shields.io/npm/v/com.sagittaras.gamedevkit.messaging?label=openupm&registry_uri=https%3A%2F%2Fpackage.openupm.com&style=flat-square)](https://openupm.com/packages/com.sagittaras.gamedevkit.messaging/) | [![nuget](https://img.shields.io/nuget/v/Sagittaras.Messaging?label=nuget&style=flat-square)](https://www.nuget.org/packages/Sagittaras.Messaging/) |
| [Sagittaras Progression](.docs/progression/index.md) | [![openupm](https://img.shields.io/npm/v/com.sagittaras.gamedevkit.progression?label=openupm&registry_uri=https%3A%2F%2Fpackage.openupm.com&style=flat-square)](https://openupm.com/packages/com.sagittaras.gamedevkit.progression/) | [![nuget](https://img.shields.io/nuget/v/Sagittaras.Progression?label=nuget&style=flat-square)](https://www.nuget.org/packages/Sagittaras.Progression/) |

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
