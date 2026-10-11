# Forge-Time Compilation and Prepared Runtime Artifacts

> Status: architectural specification / proposed direction. This document does not claim that the Forge compiler, MetaDev driver, prepared artifact format, or GPU memory shelves are implemented.

## 1. Purpose

The Forge is the editor-time driver for constructing Experiences and MicroBundles. It should perform expensive, repeatable, deterministic reasoning before deployment and emit artifacts that runtime can load directly.

MetaDev is an editor-time capability driven by the Forge. It is not expected to be independently alive before the Forge has a usable authoring/preparation path and a proper driver for MetaDev.

The central principle is:

**Resolve what can be resolved during Forge time. Keep runtime decisions limited to changing facts and work that genuinely must happen live.**

## 2. Lifecycle and artifact boundary

```text
AUTHORING INPUTS
  Experience intent, MicroBundle descriptors, defaults, explicit overrides
                            |
                            v
FORGE / EDITOR TIME
  1. Discover available capabilities and versions
  2. Validate dependencies and configuration schemas
  3. Apply MicroBundle defaults, then explicit overrides
  4. Resolve compatible contributions and policies
  5. Construct FSM stage sequences and processing groups
  6. Precompute finite, bounded outcome spaces where useful
  7. Validate data layouts, assumptions, and references
  8. Emit a versioned prepared artifact + diagnostics
                            |
                            v
PREPARED ARTIFACT
  manifest + resolved settings + prepared FSM plans
  lookup/outcome buffers + layout metadata + provenance
                            |
                            v
RUNTIME
  load/validate artifact -> allocate/load resources -> execute
  observe live inputs -> invalidate affected outputs -> reuse valid work
```

The artifact is the contract between construction and execution. Runtime must not silently reinterpret it using a different set of defaults or rerun forge-time arbitration. If a required version, capability, or assumption is unavailable, fail clearly or use an explicitly declared fallback.

## 3. Configuration semantics

Configuration should be ergonomic for both minimal and advanced manifests:

1. Each MicroBundle declares its supported configuration and developer-chosen defaults.
2. The Forge validates the supplied configuration against that contract.
3. Omitted values resolve to the declared defaults.
4. Explicitly supplied values override only the corresponding defaults.
5. The prepared artifact records the resolved values and enough identity/version information to know which contract produced them.

A minimal manifest must remain valid when every selected capability has usable defaults. Requiring users to restate every default creates noise and makes configuration brittle.

The artifact may include both source intent and resolved configuration for diagnostics, but runtime should consume the resolved form. Avoid duplicating large default documents into every source manifest.

## 4. Precompute finite outcome spaces

Precomputation is appropriate when the outcome space is bounded, expensive to evaluate repeatedly, and stable under known inputs.

For example, if a feature has about 2,000 possible outcomes, Forge time may calculate those outcomes and write a compact indexed buffer. Runtime loads the buffer and performs cheap lookup rather than repeating the expensive reasoning.

A precomputation descriptor should identify:

- the input dimensions and their bounds;
- the algorithm/version used to generate outcomes;
- the output schema and index/key mapping;
- any assumptions or external facts on which the table depends;
- validation checks and reference results;
- a deterministic content/version identity;
- the invalidation conditions that make the table stale.

Do not blindly enumerate an enormous Cartesian product. The Forge should estimate the space, record memory cost, compare precompute-versus-live alternatives, and reject or partition infeasible work. If inputs are unbounded or continuously changing, precompute reusable subproblems and leave live-dependent decisions to runtime.

## 5. Prepared FSM pipelines

FSM_API is the existing FSM substrate. The Forge prepares meaningful stateful sequences and runtime groups; it must not invent a parallel state machine system.

A bucket or capability may contribute an ordered stage sequence. Optional stages and alternative sequences are resolved during Forge time. Runtime executes the selected prepared sequence.

Illustrative stages might be preprocessing, processing, optional particle work, postprocessing, and finalization. They are examples, not a universal pipeline. A lighting or shadow bucket may have a different sequence from a geometry bucket. A day/night Experience may prepare separate sequences and select between already prepared alternatives according to an explicit runtime input; the expensive composition itself is not repeated every frame.

Runtime invalidation is distinct from Forge-time arbitration. A changed source revision may invalidate dependent outputs, but it should not trigger recomposition of the entire capability graph.

## 6. Structured runtime data and memory shelves

High-volume simulation data should not default to one managed object per entity. Class-based graphs can generate allocation and garbage-collection pressure, particularly when many objects change each frame. Where profiling supports it, use compact structured arrays, stable identifiers/indices, bulk updates, and contiguous buffers that can be consumed by CPU jobs or uploaded to a GPU.

A possible memory-shelf model:

- request a bounded runtime memory pool within OS, process, device, and application budgets;
- divide the pool into explicitly described shelves/regions with clear ownership and lifetime;
- align regions to the requirements of their actual consumers (page, cache, or GPU alignment are different constraints and must not be conflated);
- track dirty/alive/valid state in compact sidecar bitsets or equivalent metadata, rather than adding managed bookkeeping objects to every slot;
- batch uploads and updates; avoid copying unchanged shelves;
- define growth, exhaustion, reclamation, and device-loss behavior;
- expose measurements for committed/resident memory, fragmentation, transfer volume, and synchronization cost.

This is a design direction, not a claim that a universal memory lake or allocator exists. The OS does not promise that an application can safely acquire all apparently free memory. Use explicit budgets, graceful degradation, and measurements.

SingularityWarehouse remains a related conceptual predecessor for structured data and cross-application mapping, but its persistent data fabric is not automatically the runtime allocator. Keep these responsibilities separate until a concrete shared contract proves useful.

## 7. Example: directional skylanes for drone simulation

A useful stress case is a large swarm moving through vertically stacked, unidirectional lanes. Each lane has discrete cells and a separate slipstream for entry/exit; a hover lane holds a drone until a safe transition is possible.

### State and transition model

A drone moving in a lane can advance only when the target cell satisfies the required following distance. A drone changing direction exits the lane through a slipstream, decelerates, waits in a hover cell, and enters a different lane only when that lane's direction and occupancy permit it.

The transition must check both the target lane cell(s) and the slipstream segment. The expensive part is coordination, not the arithmetic of moving a drone forward.

### Safety invariants

- A lane cell has at most one owner at a time.
- A transition reserves its destination before releasing the source, or uses an equivalent atomic protocol that cannot create duplicate occupancy.
- A drone never enters a blocked slipstream or violates the required separation.
- Direction changes occur only through the declared transition path.
- If a reservation fails, the drone remains in a valid waiting state; it does not partially transition.
- Simulation update order does not permit two drones to observe the same empty cell and both claim it.

### Data-oriented representation

Candidate arrays include lane identifiers, cell indices, direction, velocity/state, destination intent, and compact occupancy/reservation state. Use stable indices and batch-oriented passes. GPU implementation is an option, not an assumption: first make the state model deterministic and testable, then compare CPU and GPU implementations against the same reference cases.

### Where Forge-time work helps

Forge can validate lane topology, direction rules, legal transition links, cell geometry, following-distance policies, and lookup tables for static lane relationships. It cannot precompute live occupancy or decide which moving drone owns a cell at a future time without a simulation model. Those are live state and require runtime coordination.

This separation lets the Forge compile the static structure while runtime resolves changing traffic safely.

## 8. MetaDev: Forge-owned development evidence

MetaDev should be invoked and driven by the Forge as an editor-time capability. Its first useful work is not speculative device scaling; it is reliable evidence collection and repeatable experiments.

Candidate measurements:

- entity/slot counts and resize events;
- memory allocation, resident/committed memory, and garbage-collection activity;
- per-stage and per-horizon timing;
- cache hits, invalidations, dirty-slot counts, and bytes uploaded;
- CPU/GPU utilization and synchronization stalls where available;
- output quality, numerical error, and deterministic reference comparisons;
- device/runtime capabilities and test-machine identity.

The Forge can then compare measured profiles and generate candidate deployment profiles. Linear algebra or ratio-based projection may help estimate initial settings, but it must be validated on real devices. CPU and GPU bottlenecks, memory bandwidth, thermal limits, synchronization, and workload shape can make scaling nonlinear.

Do not make MetaDev a required dependency of runtime packages. Do not create an autonomous MetaDev lifecycle before the Forge driver exists.

## 9. Validation and acceptance criteria

A first end-to-end vertical slice should prove:

1. A minimal Experience manifest loads with MicroBundle defaults and no duplicated default configuration.
2. An explicit override changes only the supplied field.
3. Forge time emits a deterministic, versioned prepared artifact from the same inputs.
4. A bounded outcome table can be generated, loaded, indexed, and compared with a simple reference implementation.
5. Runtime does not rerun forge-time arbitration when executing the artifact.
6. A changed input invalidates only the dependent prepared output.
7. An unsupported artifact version or missing required capability produces a clear diagnostic or declared fallback.
8. A simple skylane model rejects conflicting cell reservations deterministically.
9. Repeated runs of the same deterministic fixture produce equivalent state and output.
10. Memory and timing measurements are reported; performance claims are not inferred solely from architectural intent.

## 10. Non-goals and boundaries

- Do not make Unity, WPF, DirectX, Vulkan, WebGPU, or any single host a requirement of the Forge core.
- Do not publish or merge packages as a side effect of this architectural work.
- Do not put per-pixel arithmetic into FSM_API.
- Do not rerun capability arbitration every frame.
- Do not assume every possible outcome should be enumerated.
- Do not assume all free system memory can be reserved safely.
- Do not conflate persistent Warehouse data with transient runtime memory.
- Do not delete Revit/Inventor integrations to make a portable core.
- Do not treat MetaDev measurements as proof of linear scalability.
- Do not claim the lost Unity prototype has already been reconstructed.

## 11. Suggested implementation sequence

1. Inspect and isolate the existing Forge contracts from host/editor dependencies.
2. Define a small, versioned prepared-artifact schema and deterministic validation.
3. Build one minimal source-authored MicroBundle and Experience artifact without relying on the completed Forge UI.
4. Add one bounded precomputation example with reference tests.
5. Add a deterministic CPU skylane model as a coordination test fixture before any GPU port.
6. Add measured structured-buffer and shelf prototypes behind explicit interfaces.
7. Introduce the Forge-owned MetaDev driver only after the Forge can execute the first vertical slice.
8. Use measured evidence to choose optimizations and device profiles.
