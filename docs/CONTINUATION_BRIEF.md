# Forge Continuation Brief — Documentation-to-Working-Experience Plan

> **Purpose:** This is The Forge's own continuation / reprimer. Update it as work proceeds so a reset can resume from repository evidence instead of relying on chat history.
>
> **Scope boundary:** Maintain this document in **TheForge**. FSM_COS has its own independently maintained reprimer; do not edit FSM_COS as part of this work.

## Resume immediately

1. Confirm the live branch head and open pull requests before changing files. This brief was created against `forge/native-experience-authoring` at `ca8283b67266d30580a681fdedde1dc6066bb24d`; re-check because another contributor/LLM may have advanced it.
2. Work only in `TrentBest/TheForge` unless the user explicitly redirects the task. Do not make further FSM_COS changes.
3. Review the documentation inventory and source/tests together. Treat prose as a claim to verify, not proof that a feature exists.
4. Choose one small, observable vertical slice at a time. Implement it in the provider-neutral .NET core first, add tests, then connect it to a host/UI.
5. Keep progress and unresolved choices here. Report repository, branch, commit/PR, evidence, and the next step.
6. Do not merge a PR or publish a NuGet package without the user's explicit approval. Keep any package-publishing workflow safely disabled by default; never publish as a side effect of documentation work.

## Product north star

**The Forge authors. FSM_COS composes. The host makes it real.**

The Forge is itself an Experience: a provider-neutral authoring environment for assembling, configuring, inspecting, validating, and eventually publishing portable Experiences from versioned MicroBundles. It must not become a mandatory authoring client. External tools and programmatic authors must be able to submit portable artifacts through the same validation path.

The intended authoring experience is visual and alive, eventually diegetic/tethered, but the first proof must be a coherent, testable vertical slice—not a renderer-first rewrite.

## Ownership boundaries

- **Forge:** authored Experience and drafts; typed authoring values; composition intent; authoring configuration documents; revision/history; diagnostics; portable artifact preparation; authoring UI/session state.
- **MicroBundleDomain:** neutral MicroBundle identity, descriptors, dependency/provider declarations, and field/schema descriptions. No Forge UI, host geometry, renderer, or runtime-composition dependency.
- **MicroBundleRepository:** immutable artifact storage, identity/integrity, retrieval and publication concerns.
- **FSM_COS:** runtime manifest, version/dependency resolution, arbitration/composition, and RuntimeAssembly under its released contract. Do not modify FSM_COS in this workstream.
- **FSM_API:** real runtime FSM definitions, contexts, lifecycle, processing groups, and deferred mutation semantics; use its actual public API when implementing live behavior changes.
- **Host (WebPage/AnyApp/other):** execution loop, presentation, platform facilities, and manifestation of RuntimeAssembly.

Do not blur authored source, runtime configuration payload, runtime manifest, repository artifact, or host presentation. Do not imply that a draft edit or successful manifest compilation changed a live runtime.

## Repository snapshot at time of writing

- Repository: [TrentBest/TheForge](https://github.com/TrentBest/TheForge)
- Default branch: `master`
- Active working branch: `forge/native-experience-authoring`
- Observed branch head at the latest documentation/dependency cleanup: `48a608b2a7956a09e00c75bc93da81a75b094223` (verify live head before resuming)
- Existing pull request: [#9 — feat: establish Forge as provider-neutral Experience authoring](https://github.com/TrentBest/TheForge/pull/9) (verify current status and head before acting)
- Current core targets .NET 8 and contains an Experience model, revisioned in-memory draft, read-only MicroBundle schema inspection, and a minimal external-submission wrapper.
- The Forge core currently has direct package references to FSM_COS `0.1.0-alpha.3` and MicroBundleDomain `1.0.1`. Unused GUI.Core and FSM_Serialization references were removed from the core project because no current source uses them; add them back only when a tested capability requires their APIs. Do not target an unpublished API/package.
- At the time this brief was created, the branch was focused on a provider-neutral authoring core; do not assume a browser/desktop Forge UI is already connected to it.
- The repository includes a legacy Unity project. Preserve useful design history, but do not treat Unity as the new runtime foundation or let legacy files obscure the provider-neutral path.

## Systematic work queue

Keep statuses honest: **Implemented** requires code and relevant tests; **Partial** means a working subset with explicit limitations; **Proposed** is design only; **Decision required** means a semantic choice blocks safe implementation; **Deferred** is intentionally later.

### P0 — Make the repository's story trustworthy

- [x] Re-check live branch/PR state before the first Forge documentation commits: working branch was `forge/native-experience-authoring`; PR #9 targets `master`. Re-check again before each later write.
- [ ] Read every current documentation file and compare every implementation-status claim against source and tests.
- [ ] Make the README, documentation index, architecture note, composition design, configuration boundary, and vertical-slice documents agree on current contracts and status.
- [x] Add this continuation brief and link it from the documentation index.
- [ ] Record decisions and unresolved questions rather than silently inventing behavior.
- [x] Confirm `.github/workflows/forge.yml` runs restore/build/test for the .NET 8 test project and has no package-publish job. Exact-commit CI passed for the documentation commits (PR run [37985504308](https://github.com/TrentBest/TheForge/actions/runs/37985504308), commit `0121b20aded4e83eafa1124f0803bd0f89179a31`).

### P1 — Complete a useful, testable authoring core

- [ ] Introduce a typed, detached field-value model for the currently supported schema kinds (String, Integer, Float, Boolean, Object); do not edit through formatted display strings.
- [ ] Define field-path identity and deterministic handling for nested objects, unknown fields, missing values, defaults, and bounds.
- [ ] Implement validation-before-acceptance: rejected edits leave the previous valid draft and revision unchanged.
- [ ] Define an explicit codec interface tied to the intended MicroBundle identity/version. Never guess JSON, binary layout, or field order from schema metadata.
- [ ] Add tests for culture independence, bounds, malformed values, nested paths, detached snapshots, and rejected edits.
- [ ] Keep opaque byte-payload editing separate from typed editing when no compatible codec exists.
- [ ] Define a portable Forge project/Experience artifact format and round-trip persistence only after its schema/version and compatibility semantics are explicit.
- [ ] Preserve provenance and diagnostics through parse → validate → resolve → compile.

### P2 — Prove the composition path end to end

- [ ] Define stable artifact identity/version and explicit resolution policy; never silently substitute versions.
- [ ] Validate duplicate entries, conflicting versions, dependencies, and nested-Experience graph cycles with actionable paths.
- [ ] Keep portable authored artifact separate from the FSM_COS runtime manifest.
- [ ] Migrate the Forge compiler only when the intended released FSM_COS contract is actually available; adapt versioned roots and configuration delivery as separate concerns if the published contract requires it. Do not modify FSM_COS to make Forge fit.
- [ ] Demonstrate one deterministic path: author MicroBundle/Experience → validate → save artifact → publish/store immutable artifact → retrieve and verify identity/hash → compile/compose → observe RuntimeAssembly at a host.
- [ ] Use a small fixture capability and test every boundary. Avoid a broad multi-package migration without a passing vertical-slice proof.

### P3 — Make the Forge an actual usable Experience

- [ ] Identify the intended first host and the smallest runnable entry point; prefer an existing host/Workshop path rather than inventing a second architecture.
- [ ] Connect a minimal non-spatial composition inspector to the provider-neutral core.
- [ ] Show Experience identity, selected MicroBundles, dependencies, schema inspection, configuration/edit validity, and compilation diagnostics from real model state—not hard-coded marketing copy.
- [ ] Provide a clear unsupported/error state when schemas, codecs, artifacts, or providers are absent/incompatible.
- [ ] Add optional tooling-provider discovery only after the non-spatial authoring/edit seam is proven.
- [ ] Prototype the diegetic/tethered Forge after core authoring semantics are sound; keep tether, transforms, selection, focus, and other session presentation separate from portable Experience content.
- [ ] Connect live preview only with explicit revision scoping and a safe host/FSM_API lifecycle. Do not claim preview or live mutation before it is demonstrated.
- [ ] Later: external submissions, durable publication workflow, permissions/security, provenance, provider sandboxing, spatial tool placement, and multi-host portability.

## Documentation audit inventory

Audit in small batches, then update this table as each file is checked against source/tests.

| File | Main question | Initial status |
|---|---|---|
| `README.md` | Does the front door clearly separate vision, current implementation, and next working path? | First pass updated; deeper review remains |
| `DOCUMENTATION_INDEX.md` | Does every major document have a useful entry and status? | Updated with continuation brief; first pass complete |
| `docs/ARCHITECTURE.md` | Are package ownership and current-vs-proposed behavior accurate? | First pass complete; deeper source audit remains |
| `docs/diegetic-experience-authoring.md` | Which in-world authoring ideas are decisions, open questions, or implementable slices? | First pass complete; design-only status is explicit |
| `docs/experience-composition-design.md` | Are portable artifact, graph, validation, and runtime compilation boundaries explicit? | Updated for lossy compiler limitation; first pass complete |
| `docs/forge-owned-configuration-boundary.md` | Does the draft/typed-value/codec/persistence boundary match code? | Updated for lossy compiler limitation; first pass complete |
| `docs/live-authoring-ontology-and-cloning.md` | What is the smallest safe revision/clone/preview increment? | First pass complete; preview/clone remain proposed |
| `docs/microbundle-assembly-adventure.md` | Which metaphorical assembly stages map to tested contracts? | First pass complete; proposed pipeline is explicit |
| `docs/tooling-provider-contract.md` | Are provider/tool semantics distinguished from runtime MicroBundle providers? | First pass complete; proposed API status is explicit |
| `docs/tooling-provider-first-vertical-slice.md` | Is the read-only inspector accurately described and is the next slice concrete? | First pass complete; inspector is distinguished from editing |
| `src/**`, `tests/**` | Do all public behaviors and stated claims have relevant tests? | Current core/tests inspected; full claim-by-claim audit remains |
| `.github/workflows/**` | Are build/test jobs active and package publication explicitly gated? | Build/test workflow verified; no publish job exists in this workflow |

## First-pass audit notes (2026-10-09)

- `DOCUMENTATION_INDEX.md`: updated to link this brief; CI passed on the resulting commit.
- `docs/forge-owned-configuration-boundary.md`: first-pass review matches current code: the draft and compiler target the currently pinned FSM_COS package, schema inspection is read-only, and typed editing, codecs, persistence, and migration to a newer manifest/configuration-source contract remain unimplemented.
- `docs/experience-composition-design.md`: first-pass review correctly labels the portable artifact/nested graph pipeline as proposed and distinguishes it from the currently flat editor model. Current compiler output still only emits `BundleRequest` ID/configuration; descriptor version, dependencies/providers, and ontology are not represented by that legacy runtime manifest. Treat this as a real integration limitation, not as successful end-to-end composition.
- `docs/live-authoring-ontology-and-cloning.md`: first-pass status accurately identifies the in-memory draft as implemented while preview, cloning from published repository artifacts, provenance, publication gate, and diegetic vending remain proposed.
- `docs/diegetic-experience-authoring.md`: correctly labeled design exploration; spatial model, authoring lifecycle, nested composition, and publication/trust concerns are not presented as complete implementation.
- `docs/microbundle-assembly-adventure.md`: correctly labeled proposed pipeline; the claw, assembly show, integer-backed mapping, and publication flow are not claimed to exist.
- `docs/tooling-provider-contract.md`: correctly labeled proposed; no public diegetic tooling-provider API is claimed to exist.
- `docs/tooling-provider-first-vertical-slice.md`: accurately distinguishes the implemented read-only schema inspector from proposed editing/provider/spatial behavior.
- `docs/ARCHITECTURE.md`: first-pass status separates present core from future work; full line-by-line source verification remains open.
- `README.md`: updated to link the continuation brief and all major design documents, to expose the lossy legacy manifest compilation limitation, and to list only current direct package dependencies.
- `src/TheSingularityWorkshop.Forge/TheSingularityWorkshop.Forge.csproj`: removed unused GUI.Core and FSM_Serialization direct dependencies; current code only uses FSM_COS and MicroBundleDomain.
- CI: exact commit `db42b436fc3beca26dafe8c414bd28aa6f02b267` passed on [PR run 37985803397](https://github.com/TrentBest/TheForge/actions/runs/37985803397) and [push run 37985797306](https://github.com/TrentBest/TheForge/actions/runs/37985797306). The workflow is build/test only and contains no publish job. The next documentation update will trigger a fresh run.

## Acceptance rule for every increment

A slice is not complete until it has:
1. a clearly bounded responsibility and explicit owner;
2. code in the correct layer;
3. tests that fail before and pass after the change;
4. a build/test result on the exact commit;
5. documentation updated to say what is implemented and what remains;
6. no accidental dependency inversion, package publication, or unapproved merge.

Prefer one coherent, reviewable change over many speculative abstractions. Keep the default branch stable; do not merge work merely because it exists.

## Resume report format

At the end of each work session, update this brief with:
- current branch and exact HEAD;
- commits/PRs touched;
- files changed and observable behavior;
- exact CI/build/test evidence (or explicitly say not verified);
- newly discovered risks or decisions;
- next one to three actions, ordered by dependency.

## Workshop footer

<p align="center"><em>The Singularity Workshop — Tools for the curious, the bold, and the systemically inclined.</em><br><strong>Because state shouldn't be a mess.</strong></p>
