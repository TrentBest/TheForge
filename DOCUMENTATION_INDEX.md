# The Forge Documentation Index

The README is the front door. Use this index to move from orientation to the specific question you need answered.

## Choose a path

| If you want to... | Start here |
|---|---|
| Understand what The Forge is and what it owns | [README](README.md) |
| Explore what diegetic (in-world) authoring should mean, and the product and architecture questions it raises | [Diegetic Experience Authoring: Questions and Design Principles](docs/diegetic-experience-authoring.md) |
| Understand how MicroBundles could optionally contribute tools to a relocatable, tethered Forge, and how tooling differs from runtime behavior and configuration schemas | [Forge Tooling Provider Contract: Design Direction](docs/tooling-provider-contract.md) |
| Understand the smallest testable prototype before committing to a spatial renderer or public provider API | [Tooling Provider: First Vertical Slice](docs/tooling-provider-first-vertical-slice.md) |
| Understand the current provider-neutral core and proposed nested Experience composition | [Experience Composition: Current State and Design Direction](docs/experience-composition-design.md) |
| Understand the Workshop's shared documentation conventions | [Documentation Standard](https://github.com/TrentBest/TheSingularityWorkshop.FSM_COS/blob/development/DOCUMENTATION_STANDARD.md) |
| Understand the Forge-to-FSM_COS boundary | [README architecture overview](README.md) and the composition design note |
| Inspect the current implementation | Browse [src](src/) and [tests](tests/) on this branch |
| Understand the historical Unity implementation during migration | Browse the legacy project areas; treat them as history unless current source and tests establish otherwise |

## Status discipline

- **Implemented** means current code provides the behavior and relevant tests cover it.
- **Partial** means behavior exists with named limitations.
- **Proposed** means a design recommendation, not implementation.
- **Decision required** means a semantic or product choice remains open.
- **Deferred** means intentionally outside the current work.

The README and design notes distinguish current behavior from the intended architecture. When they disagree with source or tests, record and resolve the discrepancy rather than treating prose as proof.

---

<p align="center"><em>The Singularity Workshop — Tools for the curious, the bold, and the systemically inclined.</em><br><strong>Because state shouldn't be a mess.</strong></p>
