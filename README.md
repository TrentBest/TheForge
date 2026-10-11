# TheForge

### *Where Logic Meets Construction*

**TheForge is the authoring environment for The Singularity Workshop.** Its long-term responsibility is to let people and tools define, inspect, validate, compose, and preview Experiences and MicroBundles—then prepare the artifacts that runtime systems need.

The Forge is not the runtime itself, not the MicroBundle repository, and not a second FSM framework. It is an editor-time construction environment. It should become a first-class Experience whose available authoring tools are supplied by capabilities rather than hard-coded into one monolithic editor.

> **Spend computation while constructing. Deliver prepared work to runtime.**

## The architectural direction

```text
Authoring capabilities + Experience intent + configuration
                         |
                         v
                    THE FORGE
       discover -> validate -> compose -> prepare
                         |
                         v
          Prepared, versioned runtime artifacts
       manifest + defaults/overrides + FSM plans
       bounded outcome tables + data-layout metadata
                         |
                         v
             Runtime assembly / host
       execute prepared work; track changing inputs
       reuse valid outputs; invalidate affected work
```

This is the target architecture, not a claim that every stage is implemented today. The repository contains earlier editor and builder experiments; they are starting material to evaluate, not a reason to preserve outdated architectural boundaries.

### Forge time and runtime are different jobs

**Forge time** is where we can afford expensive, deterministic work: dependency and capability resolution, configuration validation, pipeline construction, compatibility checks, precomputation of bounded outcomes, and generation of packed runtime artifacts.

**Runtime** consumes those prepared results. It should not repeatedly arbitrate the same capabilities or rebuild plans that could have been prepared by the Forge. Runtime still responds to genuinely changing inputs, tracks validity, and performs the work that cannot be known in advance.

A manifest may omit optional configuration. In that case, a MicroBundle's declared defaults remain effective. Explicit configuration overrides only the values it supplies; omitted fields do not become accidental zeroes or mandatory manifest boilerplate.

### MetaDev is driven by the Forge

MetaDev is an intended editor-time capability, **not an independent live system before it has a proper Forge driver**. The Forge must exist as a working authoring and preparation environment first. It can then drive MetaDev to collect measurements, compare alternatives, inspect output, and help prepare artifacts.

Development and test evidence may eventually include memory use, workload quantities, resize events, timing, quality, and device capabilities. Those measurements could inform candidate device profiles and scaling projections. Projection is a hypothesis to validate against actual results—not permission to guess that a faster device scales linearly in every dimension.

## Prepared outcomes and data-oriented runtime artifacts

When a decision space is finite and manageable, the Forge can calculate its possible outcomes once and store a compact lookup table or buffer. A system with roughly 2,000 relevant outcomes may be better served by preparing those outcomes in advance than by repeating the same expensive reasoning at runtime.

That is a workload-specific optimization, not a universal rule. The Forge should record the assumptions and version of each prepared artifact, and runtime must detect when its inputs no longer match those assumptions. Unbounded or rapidly changing problems still need runtime computation.

Prepared data should be designed for its consumer. For high-volume simulation or GPU work, prefer contiguous, structured data and bulk transfer over a heap of tiny per-entity objects that creates garbage-collection pressure. Page-aligned shelves and external validity/dirty tracking are promising design directions; alignment, memory limits, and GPU transfer behavior must be measured on each target.

## Where FSM_API fits

FSM_API remains the Workshop's reusable state-machine substrate. The Forge can prepare meaningful stateful work sequences and runtime processing groups using its existing capabilities. We should not create a parallel FSM engine, nor schedule every arithmetic operation or pixel as an individual FSM.

A prepared pipeline may contain optional stages or different preconfigured sequences. The Forge resolves which sequence applies; runtime executes it and invalidates only results affected by changed inputs.

## A simulation example: coordinated skylanes

The previous drone-swarm work demonstrates why the data layout and the model must be designed together. A skylane can be modeled as a collection of directional cells with a slipstream for entry/exit and a hover cell where a drone waits for a safe opening.

- A drone enters a directional lane only when the destination cell is available at the required separation.
- Direction changes leave the lane through a slipstream, decelerate, and wait in the hover lane for a compatible opening in the new direction.
- Before a transition, the coordinator checks the destination and the relevant slipstream cells; occupancy must not be claimed by two drones.
- High-volume state should be stored in compact structured arrays suitable for bulk simulation and GPU access, not as one garbage-collected object graph per drone.

The expensive coordination rules should be explicit, testable, and optimized around cell occupancy and safe reservation. Precomputation can help with finite policies and transition lookup, but live occupancy remains dynamic and cannot simply be replaced by a static table.

This example is a design reference for rebuilding the lost capability in modular form; it is not a claim that the current repository already implements the swarm.

## Architectural boundaries

| Component | Responsibility |
| --- | --- |
| **TheForge** | Editor-time authoring, validation, composition, precomputation, prepared artifacts |
| **MicroBundleDomain** | Describes bundle identity, dependencies, providers, and configuration contracts |
| **MicroBundleRepository** | Stores and delivers immutable artifacts |
| **FSM_API** | Reusable FSM definitions, transitions, groups, and execution primitives |
| **FSM_COS** | Composes prepared capabilities into a runtime assembly |
| **Renderer** | Observer-aware visual representation, Event Horizons, buckets, and invalidation |
| **Visualize / host adapters** | Bulk presentation and platform-specific device operations |
| **SingularityWarehouse** | Portable information/data fabric; not automatically the runtime memory allocator |

The Warehouse's historical class-based and structured-data experiments are valuable design evidence. Its persistent application-data responsibilities must not be conflated with a high-throughput runtime memory pool. Host-specific Revit/Inventor work should be isolated behind adapters and preserved for later—not deleted to make the core portable.

Lower-level packages must not depend upward on TheForge or FSM_COS merely to remain useful.

## Bootstrap without circular dependency

The Forge must eventually author MicroBundles, but the first MicroBundles cannot require a completed Forge in order to exist. The initial bootstrap path remains source-authored, tested, and packaged. The Forge later becomes a richer front end for the same artifact contracts; it does not replace the contracts, repository, or runtime composer.

## Current status and next milestones

This README describes architectural intent and separates it from implementation status. The immediate sequence is:

1. Establish a usable, platform-independent Forge preparation pipeline.
2. Define a versioned prepared-artifact contract and deterministic validation.
3. Prove one bounded precomputation case end-to-end, including invalidation when inputs change.
4. Add structured, bulk-friendly runtime data layouts only where measurements justify them.
5. Give MetaDev a Forge-owned editor-time driver, then use it to gather evidence and validate scaling hypotheses.
6. Reintroduce specialized host integrations through adapters without making a host SDK a prerequisite for the core.

The goal is not to move all work into the editor. It is to **move repeatable decisions to construction time and leave runtime with prepared work and genuinely live decisions**.
