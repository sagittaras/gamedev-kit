<h1 align="center">Sagittaras Game Development Kit</h1>

<p align="center">
    Ready-made solutions for game development. Battle-tested by real games.
</p>

<p align="center">
    <a href="LICENSE">
        <img alt="GitHub License" src="https://img.shields.io/github/license/sagittaras/gamedev-kit?style=flat-square">
    </a>
    <a href="https://github.com/sagittaras/gamedev-kit/releases">
        <img alt="GitHub Release" src="https://img.shields.io/github/v/release/sagittaras/gamedev-kit?style=flat-square">
    </a>
    <a href="https://github.com/sagittaras/gamedev-kit/actions/workflows/release.yml">
        <img alt="GitHub Actions Workflow Status" src="https://img.shields.io/github/actions/workflow/status/sagittaras/gamedev-kit/release.yml?style=flat-square">
    </a>
    <a href="https://github.com/sponsors/sagittaras">
        <img alt="Static Badge" src="https://img.shields.io/badge/GitHub%20Sponsors-Support-ea4aaa?style=flat-square&logo=githubsponsors&logoColor=white">
    </a>
</p>

<p align="center">
    <a href="https://bsky.app/profile/sagittaras.bsky.social">
        <img alt="Bluesky followers" src="https://img.shields.io/bluesky/followers/sagittaras.bsky.social?style=flat-square&label=Follow%20@sagittaras.bsky.social&color=%232d5d83">
    </a>
    <a href="https://mastodon.gamedev.place/@sagittaras">
        <img alt="Mastodon Follow" src="https://img.shields.io/mastodon/follow/110701089198897448?domain=mastodon.gamedev.place&style=flat-square&label=Follow%20@sagittaras&color=%236364FF">
    </a>
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

Each package is distributed as a compiled DLL via **[GitHub Releases](https://github.com/sagittaras/gamedev-kit/releases)**.
To add a package to your Unity project:

1. Download the desired `.dll` from the latest release.
2. Place it in your project under `Assets/Plugins/`.
3. Unity will automatically detect and reference the assembly.

> If you use multiple packages, place all DLLs in the same `Assets/Plugins/` folder.

> **Check dependencies.** Some packages require other packages from this kit to function. Before importing
> a package, review its dependencies in the [documentation](.docs/index.md) and include all required DLLs.

## What's Inside

See [Docs](.docs/index.md) for what's inside and how to use it.

## What's Comming

We are creating the Development Kit as a central place for all our packages. We gradually select them from our
projects, clean them up and further develop them. However, this process is primarily based on the needs of our
own development.

Below you will find an overview of the packages we are currently working on or plan to share -
without a fixed release schedule.

### ⏱ Timing API

`elapse += Time.deltaTime`, and the follows if. Every cooldown, every interval, every animation. And how many times
have you repeated this timing method?

We did a lot, so we prepared a small but powerful API with two types that save us from this repetition.

### 🔲 Grid

Coordinate system, cell properties, entity movement tracking. Even though we run the entire Vectro Blast on it,
it is a highly portable abstraction that anyone can use!

### 🔬 Messaging — Mediator / Pub-Sub

Unified implementation of the Mediator pattern with best-practices set for game dev.

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