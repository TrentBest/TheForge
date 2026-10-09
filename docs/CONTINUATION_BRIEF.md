# Forge Continuation Brief — Documentation-to-Working-Experience Plan

> **Purpose:** This is The Forge's own continuation / reprimer. Update it as work proceeds so a reset can resume from repository evidence instead of relying on chat history.
>
> **Scope boundary:** Maintain this document in **TheForge**. FSM_COS has its own independently maintained reprimer; do not edit FSM_COS as part of this work.

## Resume immediately

1. Confirm the live branch head and open pull requests before changing files. The initial snapshot was `ca8283b67266d30580a681fdedde1dc6066bb24d`; most recently observed before this brief update was `a54e64d37dcfd0e5f513c1da791aa8875e0d9d4e`. Re-check because another contributor/LLM may have advanced it.
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
- Last observed branch head before this continuation-brief update: `07bb19e50b219290be4221cc191022b3ec543b17`; re-check live head and Actions before resuming.
- Existing pull request: [#9 — feat: establish Forge as provider-neutral Experience authoring](https://github.com/TrentBest/TheForge/pull/9) (verify current status and head before acting)
- Current core targets .NET 8 and contains an Experience model, revisioned in-memory draft, read-only MicroBundle schema inspection, immutable typed field values, schema-value validation, and a minimal external-submission wrapper. Typed values/validation are integrated into revision-safe draft edits; runtime payload encoding remains unavailable without a compatible codec.
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
- [ ] Decide the durable parameter identity contract before a production configuration codec: configuration should be parameter ID + typed override, not display-name strings. MicroBundleDomain 1.0.1 exposes field names but no stable parameter IDs; evaluate whether IDs belong in that domain contract or are supplied by an optional adapter.
- [ ] Evaluate ProtocolAi as an optional name-to-integer identity adapter, without adding a mandatory dependency until the parameter-ID ownership/versioning contract is explicit.


- [x] Introduce an immutable typed field-value model for String, Integer, Float, Boolean, and Object; values do not round-trip through display strings.
- [x] Establish schema-value rules for type matching, inclusive numeric bounds, non-finite floats, nested diagnostic paths, unknown supplied fields, and read-only detached object values. Missing values are allowed; defaults are not silently applied.
- [ ] Define stable field-path identity and decide how duplicate/invalid schema field names are reported.
- [x] Integrate validation-before-acceptance into ForgeExperienceDraft: rejected edits preserve the previous valid state and revision; no-op edits do not advance revision; clearing an edit restores baseline while revision remains monotonic.
- [x] Define `IForgeMicroBundleConfigurationCodec` and an exact-ID/version registry; codecs own payload format, decode behavior, and merge semantics. The draft uses them only when an exact matching codec is supplied.
- [ ] Implement a production codec for a real MicroBundle and prove its emitted payload is accepted by that MicroBundle. Never guess JSON, binary layout, or field order from schema metadata.
- [x] Add tests for culture independence, bounds, nested paths, detached snapshots, unknown fields, rejected edits, no-op edits, identity mismatch, revision preservation, clear-to-baseline, and refusing to compile typed edits without a codec.
- [x] Keep opaque byte-payload editing separate from typed editing; when typed edits exist, require an exact-ID/version codec and refuse silent loss.
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
| `docs/ARCHITECTURE.md` | Are package ownership and current-vs-proposed behavior accurate? | Updated with typed-value milestone; deeper source audit remains |
| `docs/diegetic-experience-authoring.md` | Which in-world authoring ideas are decisions, open questions, or implementable slices? | First pass complete; design-only status is explicit |
| `docs/experience-composition-design.md` | Are portable artifact, graph, validation, and runtime compilation boundaries explicit? | Updated for typed draft validation and lossy compiler limitation; first pass complete |
| `docs/forge-owned-configuration-boundary.md` | Does the draft/typed-value/codec/persistence boundary match code? | Updated with typed values, validation, and revision-safe draft edits; codecs remain open |
| `docs/live-authoring-ontology-and-cloning.md` | What is the smallest safe revision/clone/preview increment? | First pass complete; preview/clone remain proposed |
| `docs/microbundle-assembly-adventure.md` | Which metaphorical assembly stages map to tested contracts? | First pass complete; proposed pipeline is explicit |
| `docs/tooling-provider-contract.md` | Are provider/tool semantics distinguished from runtime MicroBundle providers? | First pass complete; proposed API status is explicit |
| `docs/tooling-provider-first-vertical-slice.md` | Is the read-only inspector accurately described and is the next slice concrete? | Updated: typed value validator and revision-safe draft editing are implemented; codecs remain open |
| `src/**`, `tests/**` | Do all public behaviors and stated claims have relevant tests? | Current core/tests inspected; full claim-by-claim audit remains |
| `.github/workflows/**` | Are build/test jobs active and package publication explicitly gated? | Build/test workflow verified; no publish job exists in this workflow |

## First-pass audit notes (2026-10-09)

- `DOCUMENTATION_INDEX.md`: updated to link this brief; CI passed on the resulting commit.
- `docs/forge-owned-configuration-boundary.md`: updated to reflect typed-value editing, exact-ID/version codec resolution, and codec-backed compilation; production codec and persistence remain open.
- `docs/experience-composition-design.md`: first-pass review correctly labels the portable artifact/nested graph pipeline as proposed and distinguishes it from the currently flat editor model. Current compiler output still only emits `BundleRequest` ID/configuration; descriptor version, dependencies/providers, and ontology are not represented by that legacy runtime manifest. Treat this as a real integration limitation, not as successful end-to-end composition.
- `docs/live-authoring-ontology-and-cloning.md`: updated to distinguish the implemented revision-safe typed field-edit seam from still-proposed preview, published-source cloning, provenance, publication gate, and diegetic vending.
- `docs/experience-composition-design.md`: updated to describe the typed draft-edit seam without overstating the still-missing portable artifact, graph resolution, or codec stages.
- `docs/diegetic-experience-authoring.md`: correctly labeled design exploration; spatial model, authoring lifecycle, nested composition, and publication/trust concerns are not presented as complete implementation.
- `docs/microbundle-assembly-adventure.md`: correctly labeled proposed pipeline; the claw, assembly show, integer-backed mapping, and publication flow are not claimed to exist.
- `docs/tooling-provider-contract.md`: correctly labeled proposed; no public diegetic tooling-provider API is claimed to exist.
- `docs/tooling-provider-first-vertical-slice.md`: updated to record typed values and revision-safe draft editing while distinguishing them from codec support, providers, and spatial behavior.
- `docs/ARCHITECTURE.md`: updated to record typed values and revision-safe draft editing; full line-by-line source verification remains open.
- `README.md`: updated to link the continuation brief and major design documents, expose the lossy legacy compiler limitation, list only current direct package dependencies, and distinguish the new typed validator from not-yet-integrated draft editing.
- `src/TheSingularityWorkshop.Forge/TheSingularityWorkshop.Forge.csproj`: removed unused GUI.Core and FSM_Serialization direct dependencies; current code only uses FSM_COS and MicroBundleDomain.
- Typed value implementation: `aae363f1ef2378bf2e71e9d6fb5c6fc4d504610a`; tests: `b1d41a753c2bcdf7b786f9635a703a9ca48b9157`. CI passed on the test commit via [PR run 37986169800](https://github.com/TrentBest/TheForge/actions/runs/37986169800) and [push run 37986162045](https://github.com/TrentBest/TheForge/actions/runs/37986162045). Subsequent documentation updates are being checked on their exact commit; the workflow is build/test only and contains no publish job.

## Earlier typed-value implementation (2026-10-09)

- Added `src/TheSingularityWorkshop.Forge/ForgeFieldValue.cs`: immutable typed values for String, Integer, Float, Boolean, and nested Object; schema validator emits machine-readable diagnostics with dotted field paths, enforces numeric bounds and finite floats, rejects unknown supplied fields, and deliberately neither fills defaults nor serializes runtime bytes.
- Added `tests/TheSingularityWorkshop.Forge.Tests/ForgeFieldValueTests.cs`: checks detached/read-only object values, duplicate/blank keys, kind mismatch, bounds, culture-independent numeric validation, non-finite floats, nested paths, unknown fields, and omitted values.
- CI passed on `f76a28dced931025b9dcce04aa970eccdb051e03` via [PR run 37986541190](https://github.com/TrentBest/TheForge/actions/runs/37986541190) and [push run 37986537112](https://github.com/TrentBest/TheForge/actions/runs/37986537112), including the revision-safe draft edit implementation and tests. Later documentation commits are still being verified on exact HEAD; check latest status before calling the branch green.
- Integrated typed values into `ForgeExperienceDraft.TrySetFieldValue` with exact schema identity/version matching, validation-before-acceptance, no-op handling, monotonic revision changes, clear-to-baseline support, and an explicit refusal to compile typed edits without a codec. Tests cover accepted/rejected/no-op edits, revision safety, identity mismatch, and the codec boundary.
- Corrected a compile issue found by CI (explicit diagnostic constructors) and removed a test that tried to construct duplicate schema fields, which MicroBundleDomain correctly forbids. The validator retains defensive duplicate handling.
- Added `IForgeMicroBundleConfigurationCodec`, exact-ID/version `ForgeConfigurationCodecRegistry`, and `ForgeExperienceDraft.ToExperience(codecResolver)`. Typed edits now require an exact codec; encode/decode is tested with a test-only fixture codec, not a real MicroBundle implementation. Architecture review also confirmed that MicroBundleDomain 1.0.1 exposes field names but no stable parameter IDs. Next priority is to settle the parameter-ID contract (ID ownership, nesting, version compatibility) and evaluate ProtocolAi as an optional identity adapter before implementing a production codec. Do not mistake the fixture format for a production wire format.

## Latest incremental implementation (2026-10-09)

- `ForgeExperienceDraft.TrySetFieldValue` validates schema-backed edits before mutation; rejection preserves draft values/revision, no-op edits do not advance revision, and clearing an edit can restore baseline without rewinding revision.
- Added `IForgeMicroBundleConfigurationCodec` and `IForgeMicroBundleConfigurationCodecResolver`, plus `ForgeConfigurationCodecRegistry` with exact MicroBundle ID/version matching and duplicate-registration rejection.
- `ToExperience(codecResolver)` now uses a matching codec for typed edits and refuses missing/mismatched codecs rather than silently discarding values. The codec owns its payload format and merge semantics.
- Tests cover exact-version lookup, missing/wrong-version refusal, and a deterministic encode/decode round trip using a **test-only fixture codec**. No production MicroBundle codec exists yet.
- Codec implementation commits: `b847bdc088ee19a73bbb5a08bbb0875f5e4de65b`, `c353825be740a18bfb6642c2f4ccfe615fa6bf20`, `7f97882041327216308279cdd91432a4a617491b`. Documentation has been aligned; check exact-HEAD Actions before calling the slice green.

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


## Direction pivot captured — HeadlessAi and the living Forge (2026-10-09)

- Added [HeadlessAi and the Living Forge](headless-ai-and-living-forge.md) as a **proposed direction note**, and linked it from DOCUMENTATION_INDEX.md. This captures the user's new concept without misrepresenting it as implemented.
- **HeadlessAi** is proposed as a standalone, provider-neutral NuGet package for configured HTTP-agent communication with AI endpoints. It is not an AI model. GPT, Gemini, Claude, and other providers require explicit request/response contract handling; do not assume one universal body format.
- **ProtocolAi and GrammarAi are optional integrations**, not required dependencies. The basic HeadlessAi package must remain useful without either one and must not depend on The Forge or FSM_COS.
- **Horsemen/riders** describe configurable endpoint profiles and agent instances metaphorically. Keep reusable provider profile identity distinct from running agent/session identity, permissions, context, budget, and lifecycle.
- **Living Forge vision:** Hermit is a full-size humanoid guide/agent that explores tools and demonstrates possible parameter changes. It may be playful, but exploratory activity must remain sandboxed and must not silently commit authored changes or mutate a live runtime.
- **Visual scale language:** Minions are 1/3 Hermit size; Titans are 3× Hermit size. These are presentation metaphors, not permissions or measures of intelligence. Dramatic staging should be driven by actual task/impact state, not fake progress.
- **Visible consequence pipeline:** intent → impact analysis → preview/approval → staged work → observable change → verification. Example: changing a distant mountain range's age can dispatch a swarm of small visual workers and a few Titans toward the affected range, with the result grounded in real work states and a verifiable outcome.
- **Safety/ownership rules:** secrets stay outside ordinary config/artifacts/logs; tool permissions are explicit; model output is not executable by default; consequential changes require approval; distinguish proposal, draft edit, accepted authored change, and live runtime mutation.
- **Recommended first HeadlessAi slice:** credential-safe provider profile plus one HTTP adapter, injected transport, fixture-based tests, cancellation/timeouts, structured diagnostics, and secret redaction. Validate a second provider before over-generalizing the abstraction.
- **Recommended first Forge visual slice:** Hermit inspects one real schema parameter, proposes one typed edit, presents impact/preview, and waits for accept/reject. Add theatrical Minion/Titan staging only after task-state mapping is real.
- **Unresolved HeadlessAi decisions:** generic request vs provider-profile/session API; first provider set; stable identity/version; ownership of conversation history and cost accounting; tool authorization; secure secret resolution; host-independent proposal/task contracts; mapping visual worker counts to real work.
- Prior exact-head run for 6e9d7e3097fc3ed3279176b6a03c51492f7c5cfd completed successfully: [GitHub Actions run 37987648451](https://github.com/TrentBest/TheForge/actions/runs/37987648451). The new documentation commits still require exact-head CI verification; do not claim them green until checked.
- Current working branch remains forge/native-experience-authoring; PR #9 remains open against master. No merge, NuGet publication, or FSM_COS edit was performed.
- **Next actions after this pivot:** (1) verify CI on the new documentation head; (2) pause Forge's earlier parameter-ID/codec implementation queue unless the user redirects back to it; (3) start HeadlessAi contract/provider research in a separate short-lived branch or repository only after checking whether a HeadlessAi repository already exists and agreeing the first provider slice.

