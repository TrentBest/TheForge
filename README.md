# <img src="docs/assets/section-markers/00-identity.svg" alt="" width="20" height="20"> The Forge

<img src="docs/assets/section-dividers/00-identity.svg" alt="" width="100%">

[![Build](https://img.shields.io/github/actions/workflow/status/TrentBest/TheForge/forge.yml?branch=forge/native-experience-authoring&style=flat-square&logo=github)](https://github.com/TrentBest/TheForge/actions)

## 01 The problem

Creating software often means stitching together tools, capabilities, configuration, and runtime behavior without a clear view of how they fit. The Forge makes that composition visible and authorable.

## 02 What it is

**The Forge is an Experience that creates Experiences and MicroBundles.**

It belongs to The Singularity Workshop, and participates in the same Experience ecosystem it helps creators build. A creator can discover and compose reusable MicroBundles, create nested Experiences, configure capabilities, inspect relationships, and prepare portable artifacts.

## 03 Why it is different

**The Forge is fully diegetic.** It is a place within the authored Experience—not a conventional editor floating outside the world. The authored world remains around the creator; Forge tools are encountered and used in-world. The Forge can be tethered to a chosen place in an Experience rather than dictating where the Experience must happen.

The diegetic environment is the product requirement, not a decorative skin or optional future mode. The current provider-neutral .NET core is groundwork for the authoring model; it does **not** yet implement the full in-world environment or interaction loop. Documentation distinguishes implemented behavior from intended experience.

## 04 See it in a minute

```text
                 THE FORGE
             an Experience itself
                      │
          ┌───────────┴───────────┐
          ▼                       ▼
     MicroBundles             Experiences
   reusable capabilities    composed worlds
          └───────────┬───────────┘
                      ▼
             portable Experience
                      │
                      ▼
                   FSM_COS
                      │
                      ▼
             host makes it real
```

**The Forge authors. FSM_COS composes. The host makes it real.**

## How to explore the work

- [Documentation index](DOCUMENTATION_INDEX.md) — a guided map of the design, contracts, and implementation.
- [Diegetic Experience Authoring](docs/diegetic-experience-authoring.md) — the in-world product model and its design constraints.
- [Experience Composition](docs/experience-composition-design.md) — current implementation and nested-composition direction.
- [Forge-Owned Configuration Boundary](docs/forge-owned-configuration-boundary.md) — authored values, codecs, and runtime manifest boundaries.
- [Continuation brief](docs/CONTINUATION_BRIEF.md) — current work order, guardrails, and evidence.
- [Architecture](docs/ARCHITECTURE.md) — ownership boundaries across Forge, MicroBundleDomain, MicroBundleRepository, FSM_API, FSM_COS, and hosts.
- [MicroBundleRepository](https://github.com/TrentBest/TheSingularityWorkshop.MicroBundleRepository) — artifact storage and retrieval.
- [MicroBundleDomain](https://github.com/TrentBest/TheSingularityWorkshop.MicroBundleDomain) — neutral MicroBundle definitions.
- [FSM_API](https://github.com/TrentBest/FSM_API) — the state-machine substrate.
- [FSM_COS](https://github.com/TrentBest/TheSingularityWorkshop.FSM_COS) — runtime composition and assembly.

---

<p align="center"><em>The Singularity Workshop — Tools for the curious, the bold, and the systemically inclined.</em><br><strong>Because state shouldn't be a mess.</strong></p>
