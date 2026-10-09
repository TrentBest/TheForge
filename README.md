# <img src="docs/assets/section-markers/00-identity.svg" alt="" width="20" height="20"> 00 The Forge

<img src="docs/assets/section-dividers/00-identity.svg" alt="" width="100%">

[![Build](https://img.shields.io/github/actions/workflow/status/TrentBest/TheForge/forge.yml?branch=master&style=flat-square&logo=github)](https://github.com/TrentBest/TheForge/actions)

## <img src="docs/assets/section-markers/01-definition.svg" alt="" width="20" height="20"> 01 Definition

<img src="docs/assets/section-dividers/01-definition.svg" alt="" width="100%">

**The Forge is a provider-neutral authoring Experience for composing, validating, and preparing portable Experiences from reusable MicroBundles.**

### *The Experience that forges Experiences.*

The Forge is the authoring Experience of **The Singularity Workshop**.

This README follows the [Workshop Documentation Standard](https://github.com/TrentBest/TheSingularityWorkshop.FSM_COS/blob/development/DOCUMENTATION_STANDARD.md). The [documentation index](DOCUMENTATION_INDEX.md) provides focused paths into the Forge's theory, architecture, contracts, and implementation status.

It is the place where Experiences and MicroBundles can be selected, composed, configured, visualized, validated, and eventually published — while remaining completely optional as an authoring client.

> **If a creator can imagine an Experience, the Forge should strive to provide the vocabulary and composition system needed to express it.**

The Forge itself is an Experience. That means it participates in the same ecosystem it creates.


### The Forge as a tethered place

The Forge is envisioned as a place-like authoring presence that can be tethered anywhere in the Experience being created. The authored world renders around it; within the Forge, installed MicroBundles may contribute their own tooling representations through optional providers. The Forge hosts those tools under shared spatial and interaction rules rather than hard-coding a custom screen for every capability.

The tether and tool arrangement are authoring-session presentation, not hidden changes to the portable Experience. This spatial/provider model is **proposed design**, not implemented behavior. See [Diegetic Experience Authoring](docs/diegetic-experience-authoring.md#spatial-model-a-tethered-forge-with-a-world-around-it) and the proposed [Tooling Provider Contract](docs/tooling-provider-contract.md).

## <img src="docs/assets/section-markers/06-responsibility-boundary.svg" alt="" width="20" height="20"> 06 Responsibility boundary: authoring is not locked to the Forge

<img src="docs/assets/section-dividers/06-responsibility-boundary.svg" alt="" width="100%">

This is a deliberate design constraint.

A creator may use:

- the Forge UI;
- another editor;
- a domain-specific authoring application;
- a custom program;
- generated data;
- independently published MicroBundles.

The resulting portable artifact should be able to enter the same validation and publication path.

```
             Forge author
                  │
External author ──┼──► Experience artifact
                  │
                  ▼
          validation / classification
                  │
                  ▼
              publication
                  │
                  ▼
               FSM_COS
```

The Forge is therefore an **authoring client**, not an authoring prison.

This is one of the most important architectural decisions in the project.

### Ontology and visual composition

The Forge is where human semantic choices can become machine-oriented composition.

```
[ONTOLOGY]
     │
     ▼
[CAPABILITY MEMBERSHIP]
     │
     ▼
[MICROBUNDLES]
     │
     ▼
[EXPERIENCE]
     │
     ▼
[MANIFEST / EXECUTION STRUCTURE]
     │
     ▼
[FSM_COS]
```

The visual representation should eventually be an operational map of this composition.

Visuals are not decoration here. The Forge exists to expose relationships that are otherwise difficult to see.

The intended interface is therefore a **living composition graph** rather than a conventional collection of property forms.

## <img src="docs/assets/section-markers/07-architecture-ecosystem.svg" alt="" width="20" height="20"> 07 Architecture and ecosystem

<img src="docs/assets/section-dividers/07-architecture-ecosystem.svg" alt="" width="100%">

```
                         ┌─────────────────────────┐
                         │      THE FORGE           │
                         │  Experience Authoring    │
                         │                          │
                         │  ontology                │
                         │  capabilities            │
                         │  MicroBundles            │
                         │  Experiences             │
                         │  configuration           │
                         └────────────┬────────────┘
                                      │
                              portable artifact
                                      │
                                      ▼
                         ┌─────────────────────────┐
                         │       FSM_COS            │
                         │  composition / execution │
                         └────────────┬────────────┘
                                      │
                                RuntimeAssembly
                                      │
                    ┌─────────────────┼─────────────────┐
                    │                 │                 │
                    ▼                 ▼                 ▼
                 AnyApp            WebForge        Server Host
                 native             web             service
                 runtime         presentation        runtime
```

**The Forge authors. FSM_COS composes. The host makes it real.**

### Authored source belongs to Forge

Forge owns the editable Experience, its authored configuration documents, and the manifest source from which runtime-facing artifacts are compiled. FSM_COS consumes the runtime manifest contract; it does not own the Forge project or become the configuration editor. MicroBundleDomain describes neutral schema metadata, while a MicroBundle-specific codec must explicitly define how typed authoring values become the payload that MicroBundle consumes. Forge must never guess that encoding from display text or from schema defaults.

See [Forge-Owned Configuration and Manifest Boundary](docs/forge-owned-configuration-boundary.md) for the implemented/proposed split and acceptance criteria.

For changes to live state-machine behavior, Forge design and implementation must explicitly reference the Workshop's [FSM_API](https://github.com/TrentBest/FSM_API): its runtime-modifiable definitions, POCO contexts, processing groups, instance lifecycle, and deferred structural mutation rules. Forge must not replace these real capabilities with a narrower generic abstraction or imply that editing a draft or compiling a manifest has already changed a live runtime. See [Forge Architecture](docs/ARCHITECTURE.md) and [Diegetic Experience Authoring](docs/diegetic-experience-authoring.md).

## <img src="docs/assets/section-markers/09-core-concepts.svg" alt="" width="20" height="20"> 09 Core concepts: what the Forge creates

<img src="docs/assets/section-dividers/09-core-concepts.svg" alt="" width="100%">

### MicroBundles

A MicroBundle is a reusable capability composition.

The Forge can describe:

- identity and version;
- dependencies;
- providers;
- opaque configuration;
- ontology/classification information;
- relationships to other capabilities.

The runtime implementation remains outside the authoring model.

### Experiences

An Experience is a composition of capabilities.

The Forge can eventually author Experiences that:

- contain MicroBundles;
- contain other Experiences;
- select capabilities by ontology;
- establish or accept a Dynamic Environment;
- define sequential execution structure;
- configure reusable capabilities;
- publish immutable artifacts.

The same machinery can author the Workshop itself.

## <img src="docs/assets/section-markers/10-usage-examples.svg" alt="" width="20" height="20"> 10 External authoring and submission

<img src="docs/assets/section-dividers/10-usage-examples.svg" alt="" width="100%">

The Workshop should be capable of accepting an Experience built somewhere else.

The long-term boundary is:

```
External Application
       │
       ▼
Portable Experience Artifact
       │
       ▼
Submission Service
       │
       ├── validate
       ├── classify
       ├── resolve MicroBundles
       ├── verify integrity
       └── publish
       │
       ▼
Experience Catalog
       │
       ▼
FSM_COS Host
```

This allows the Workshop to become infrastructure for creators rather than a tool that dictates how they must create.

See [Issue #8](https://github.com/TrentBest/TheForge/issues/8).

### Provider-neutral runtime

The Forge does not decide where an Experience runs.

The same published composition can eventually be consumed by:

- **AnyApp** — native desktop runtime;
- **WebForge** — web presentation;
- **server hosts** — services and persistent processes;
- future simulation, research, tooling, or distributed hosts.

FSM_COS remains the host-neutral composition boundary.

## <img src="docs/assets/section-markers/11-verification-development.svg" alt="" width="20" height="20"> 11 Current migration and verification

<img src="docs/assets/section-dividers/11-verification-development.svg" alt="" width="100%">

The original Forge was built heavily around Unity. It contains useful architectural history:

- Experience Builder;
- manifest-driven development;
- provider composition;
- arbitration;
- builder contracts;
- the Singularity Hub;
- visual editor experiments;
- numerous domain-specific builders.

Unity is no longer the intended foundation.

The migration therefore keeps the repository and its history while establishing a provider-neutral .NET authoring core.

The new core consumes Workshop NuGet packages rather than recreating their responsibilities.

Current foundation:

- `TheSingularityWorkshop.FSM_COS`
- `TheSingularityWorkshop.MicroBundleDomain`
- `TheSingularityWorkshop.GUI.Core`
- `TheSingularityWorkshop.FSM_Serialization`

The old Unity implementation can be retired incrementally after the new contracts prove themselves.

### Development

The provider-neutral core targets .NET 8.

CI builds and tests the new Forge core independently of Unity.

The Unity material remains in the repository during migration so useful prior work is not discarded before its concepts have been recovered.

### Current implementation status

The provider-neutral core currently compiles a flat list of MicroBundles into an FSM_COS runtime manifest. It now also has an in-memory `ForgeExperienceDraft` with revisioned semantic edits, baseline-difference tracking, and detached manifest compilation. This is not yet a published-artifact clone: source-version provenance, schema-aware typed editing, validation, revision-bound live preview, and the publication gate remain design and implementation work. Nested Experiences and the complete portable-artifact submission pipeline also remain design work.

See [Experience Composition: Current State and Design Direction](docs/experience-composition-design.md) for the current-state boundary, proposed nested-composition rules, validation flow, acceptance tests, and unresolved decisions. For the larger product questions—including diegetic authoring, portable artifacts, lifecycle, and trust—see [Diegetic Experience Authoring: Questions and Design Principles](docs/diegetic-experience-authoring.md).

## <img src="docs/assets/section-markers/12-documentation-map.svg" alt="" width="20" height="20"> 12 Documentation map and further reading

<img src="docs/assets/section-dividers/12-documentation-map.svg" alt="" width="100%">

- [Diegetic Experience Authoring](docs/diegetic-experience-authoring.md) — product and architecture questions for in-world creation.
- [Experience Composition Design](docs/experience-composition-design.md) — current implementation boundary and proposed nested composition.
- [Live Authoring, Ontology Vending, and Cloning](docs/live-authoring-ontology-and-cloning.md) — implemented draft foundation, proposed preview and clone lifecycle, and the intended diegetic creator experience.
- [Documentation Index](DOCUMENTATION_INDEX.md) — reader-oriented map of the Forge documentation.
- [Workshop Documentation Standard](https://github.com/TrentBest/TheSingularityWorkshop.FSM_COS/blob/development/DOCUMENTATION_STANDARD.md) — shared conventions.

## <img src="docs/assets/section-markers/13-related-projects.svg" alt="" width="20" height="20"> 13 Related projects and Workshop identity

<img src="docs/assets/section-dividers/13-related-projects.svg" alt="" width="100%">

- [FSM_API](https://github.com/TrentBest/FSM_API) — state/process foundation
- [FSM_COS](https://github.com/TrentBest/TheSingularityWorkshop.FSM_COS) — composition and runtime assembly
- [MicroBundleDomain](https://github.com/TrentBest/TheSingularityWorkshop.MicroBundleDomain) — MicroBundle semantic description
- [MicroBundleRepository](https://github.com/TrentBest/TheSingularityWorkshop.MicroBundleRepository) — immutable artifact storage and publication
- [GUI](https://github.com/TrentBest/TheSingularityWorkshop.GUI) — platform-neutral GUI model
- [WebPage / WebForge](https://github.com/TrentBest/WebPage) — web Experience proving ground
- [AnyApp](https://github.com/TrentBest/AnyApp) — native RuntimeAssembly host

### Guiding principle

> **The Workshop should never be the reason a creator cannot build what they imagine.**

The Forge provides a vocabulary and composition system.

It does not define the limits of imagination.


