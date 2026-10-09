# MicroBundle Assembly Adventure: Design Direction

> **Status:** Proposed experience and pipeline design. This document does not claim that the claw, package slot, crank, animated assembly, integer-backed registry, or MicroBundle publication pipeline is implemented.

## The governing idea

**Make the work visible, make the result inspectable, and make every dramatic moment correspond to a real operation.**

The Forge's package assembler should feel like a place in which a creator turns authored parts into a finished MicroBundle. It should be playful without making the artifact mysterious, and computationally honest without reducing the process to a progress bar.

The assembler is a diegetic presentation over a portable authoring and packaging model. A command-line tool, automated author, or conventional editor must be able to perform the same operations without the claw, room, crank, or animation.

## The assembly floor

### 1. Living workbench

New and unfinished creations appear in the Forge workspace as living, manipulable objects. Their motion and presence communicate that they are still being authored or arranged.

The workspace is not the package itself. Moving, focusing, or posing an object is presentation/session state unless the user explicitly performs an authoring operation. The portable draft records intentional composition changes, not incidental animation.

### 2. Claw mode: select a prize

The creator switches the workspace into **claw mode**. A claw reaches into the workspace, picks up a selected MicroBundle part or other eligible authored item, and carries it to the package slot.

This should be actionable rather than a decorative cutscene:

- The creator chooses the candidate; the claw's target is visible before pickup.
- Eligibility and missing requirements can be inspected before the move.
- The pickup operation creates or adds an explicit draft reference; it does not silently consume or mutate the source.
- The user can cancel before the operation commits.
- Failed placement returns the item safely and explains the reason.

The claw is one interaction adapter for a real authoring command. The same command must be available through accessible controls and non-diegetic interfaces.

### 3. The package slot: alive becomes inspectable

When an item is accepted into the draft package, it transitions from its lively workspace presentation into a flat, screen-facing package inventory. This is a change of presentation, not a claim that the source object has been destroyed or that the package has been published.

Each placed item becomes an inspectable entry showing relevant facts from the underlying model, such as:

- identity and version;
- source/provenance;
- dependencies and dependency status;
- providers or capabilities contributed;
- configuration status;
- validation findings;
- inclusion or exclusion from the current draft.

The displayed fields must come from actual schema and artifact data. The UI must not infer authoritative metadata from an object's appearance.

The package inventory is the user's review surface. Items can be inspected, reordered where ordering is meaningful, removed from the draft, or returned to the workbench. Every action has a defined effect on the draft.

### 4. Pull the publish crank

The crank is the explicit commit gesture: **“I have reviewed this package; prepare the artifact.”**

Pulling it starts a real pipeline. It must not mean “publish to a public registry” unless a separate destination and authorization explicitly make that the operation. At minimum, distinguish:

1. **Prepare** — freeze or snapshot the selected draft revision for this run.
2. **Resolve** — discover and verify required dependencies and versions.
3. **Validate** — check identities, schema/configuration, dependency compatibility, required fields, and other domain rules.
4. **Normalize** — produce a canonical representation where the contract defines one.
5. **Register/map** — resolve eligible repeated string identifiers to stable integer-backed identities using an explicit registry contract.
6. **Encode** — produce the artifact's required representation and include the mapping/version metadata needed to interpret it.
7. **Verify** — check the produced output against its declared structure and integrity expectations.
8. **Package** — write the portable MicroBundle artifact and its manifest/metadata.
9. **Deliver** — place it at the chosen destination or hand it to a separate publication service.
10. **Report** — show the actual result, artifact identity, version, diagnostics, and any next action.

The pipeline is a proposed sequence, not a claim that every stage already exists. The actual stages should be driven by the artifact contract and implementation. If a stage is not required for a particular bundle, do not invent work merely to animate it.

## Turn computation into a show

The assembly show should be driven by observable work emitted from the pipeline. Each stage can expose meaningful events such as:

- items and dependencies discovered;
- validations completed, passed, or failed;
- identifiers looked up, newly registered, reused, or left as strings;
- bytes or records encoded and verified;
- output artifacts produced;
- elapsed time and stage duration;
- actionable warnings, errors, and recovery choices.

The interface can turn those events into machines engaging, parts moving, mappings lighting up, dependency paths connecting, and the artifact taking shape. Prefer counts and facts over invented percentages. If total work is not knowable up front, show completed stages and concrete counters instead of pretending the percentage is exact.

**Never add artificial delay to make the show feel substantial.** If the work completes before the creator has stood up to make coffee, the machinery can perform a brief completion flourish while the result is already available. Animation must not block the artifact, lie about progress, or hold up a user's next action.

The user can inspect diagnostics during the show. A failure should stop or isolate the affected stage, preserve the last valid draft and any safe intermediate results, and explain how to recover. Retrying should not silently duplicate registrations or publish a second artifact.

## String identifiers, integer-backed mapping, ProtocolAi, and GrammarAi

The Workshop should distinguish three things that are easy to conflate:

1. **A string is domain data.** Names, descriptions, arbitrary user text, and values that are not registered vocabulary must remain strings when the domain requires them.
2. **An integer-backed mapping is an identity/registry contract.** It maps a registered identifier to a stable integer and supports reverse lookup, versioning, collision avoidance, and persistence. It must not rely on process-local ordering or a hash value alone as if that were a stable assigned identity.
3. **ProtocolAi and GrammarAi have different jobs.** In the Workshop's current design, ProtocolAi supplies the meaning of registered integer identifiers—what a number represents—while GrammarAi defines how those identifiers are arranged into a valid compact instruction. ProtocolAi is not simply “any string-to-int conversion,” and GrammarAi is not the registry.

The existing ProtocolAi/GrammarAi implementation in the Workshop's Architect project is a useful conceptual reference: it prefers registered integers where mappings exist, keeps genuine string values available, and states that an LLM must never invent an integer. Its current tables are a small seed vocabulary, not proof of a production-ready, globally stable MicroBundle registry. The eventual extensible mapping and stable registration responsibility described there belongs with the Workshop's planned DataWarehouse/registry contract.

For MicroBundle assembly, establish these rules before encoding artifacts:

- define the identifier namespaces and their ownership;
- define whether an ID is local to one artifact, registry, protocol version, or global catalog;
- persist the authoritative mapping and its version/namespace;
- support deterministic reverse lookup and conflict detection;
- preserve unknown or true string values rather than coercing them into invented integers;
- define migration and compatibility behavior when vocabulary or schema evolves;
- verify that an artifact can be decoded independently of the process that created it.

Integer-backed representation is a measured optimization and an explicit data contract, not a license to erase human-readable source data. The original semantic identity must remain inspectable.

## Where the runtime belongs: FSM_API and FSM_COS

MicroBundle packaging and live FSM modification are separate concerns.

- **Forge** owns the draft, authoring intent, validation feedback, and the diegetic assembly experience.
- **MicroBundleDomain** owns its domain contracts; the Forge must not invent missing domain guarantees.
- **ProtocolAi / GrammarAi** inform semantic integer vocabulary and compact instruction grammar where those contracts apply.
- **FSM_COS** owns runtime composition and manifest responsibilities according to its actual contracts.
- **FSM_API** is the specific reference for live FSM definitions, POCO contexts, processing groups, lifecycle, and safe deferred structural mutation.
- **The host** supplies execution and deployment facilities.

Building a MicroBundle artifact does not, by itself, mean a live FSM has changed, an Experience is running, or a remote catalog accepted the package. Those are separate, observable outcomes. If an authored operation requests a live structural change, use FSM_API's public API and reflect its deferred application honestly.

## Finish line and user trust

When assembly succeeds, the flat package inventory becomes a clear artifact summary:

- exact artifact name, version, and identity;
- included contents and dependency resolution;
- validation and integrity result;
- encoding/registry metadata needed to interpret it;
- destination and delivery state;
- elapsed time and a concise record of the actual work performed.

“Built locally,” “saved,” “handed off,” and “published to a registry” are different states. Use the exact state reached. A failed delivery must not erase a successfully built local artifact, and a successful local build must not be described as public publication.

The adventure is the presentation. The portable artifact, validation rules, and truthful lifecycle are the product.
