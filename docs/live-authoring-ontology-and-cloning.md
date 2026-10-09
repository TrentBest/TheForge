# Live Authoring, Ontology Vending, and the Cloning Machine

> **Status:** Mixed. A first in-memory `ForgeExperienceDraft` now supports revisioned semantic edits and a baseline-difference check. The live preview loop, published-source provenance, cloning workflow, immutable publication policy, dirty-draft gate, and diegetic vending machine remain proposed and are not yet implemented.

## Governing rule

**The Forge facilitates creation; it does not dictate what a creation must look like.** A MicroBundle can be represented as a toy, a full-size character, a component, a schematic, a living specimen, or a technical data view. An Experience can be a world, simulation, interface, service, or composition of capabilities. The underlying artifact—not its presentation—determines what it is.

The Forge's distinctive promise is that the creator can inspect and manipulate an authored thing while seeing the result of the change. A property editor that changes a wheel's size should show the actual wheel changing in the preview. The creator should not have to leave the Forge, rebuild a separate scene, or guess what the final result will look like.

This is a live authoring environment over a structured model, not merely a collection of attractive controls.

## One source of truth, multiple views

Each authoring session should bring together:

- **The editable draft:** the explicit, versioned source of creator intent.
- **The live preview:** a manifestation of the current draft revision, with its source revision clearly identified.
- **The inspector:** schema-driven fields, units, constraints, provenance, dependencies, and diagnostics.
- **The composition view:** how MicroBundles and their configured instances relate within an Experience.
- **The artifact view:** what will be saved, packaged, or published.

A change updates the draft through an explicit command. Validation runs against that change, and the preview reflects it when the relevant preview pipeline succeeds. The interface should distinguish *preview current*, *preview updating*, and *preview stale/failed*. It must never show an old preview as if it represents the latest draft.

Not every field can necessarily update a running simulation instantly. A change may require a rebuild, re-resolution, restart, or isolated preview. The Forge should show which operation is needed and why, while keeping the creator in the same workspace.

### Parts are first-class authoring units

A mech is a summation of its parts, not a picture with a script that pretends to make it functional. Wheels, joints, sensors, power sources, actuators, control systems, armor, and other components should have their own identities and contracts where the domain supports that separation.

Changing a wheel's size should affect the wheel in the preview. Whether it also changes clearance, suspension travel, torque, mass, power draw, collision geometry, or mobility depends on explicit relationships and simulation models—not an arbitrary promise made by the editor. The Forge should expose consequences that its underlying models can actually calculate and label unresolved consequences honestly.

This relocates complexity into explicit, composable data and domain behavior. It does not make complexity disappear. Some Experiences will have simple visual representations; others can use a Singularity Twin with richer structure, instrumentation, and simulation. The Forge should support both without requiring every creator to build a full digital twin.

## The ontology vending machine

Imagine an ontology vending machine with a visible grid of MicroBundle representations. A creator finds an item labeled **H3** in the catalog, then presses the **H** and **3** controls. The machine resolves that selection against actual catalog/ontology data, rotates the appropriate rail, advances the chosen representation, disengages it, and lets it fall into the catch tray.

The prize may appear as a small He-Man toy because a toy is easy to carry and inspect. That scale is a presentation choice, not a limit on the MicroBundle. The creator can expand it to full size, inspect its component structure, or extract a particular part or capability if the artifact contract permits it.

The sequence must be more than theater:

1. **Discover:** show the ontology/catalog identity, grid address, version, provenance, and what is known about the candidate.
2. **Select:** resolve H3 through a real lookup. The grid coordinate is an address in a particular catalog view, not a permanent identity by itself.
3. **Acquire:** create a draft reference or working copy according to the chosen action. Do not silently consume or mutate the published source.
4. **Inspect:** reveal schema-backed metadata, component relationships, capabilities, dependencies, and available variants.
5. **Manipulate:** allow only edits supported by the declared authoring contract, with validation and a live preview.
6. **Compose:** add the selected MicroBundle or configured instance to an Experience draft.
7. **Recover:** make it possible to return, remove, discard, or reacquire the item without losing unrelated work.

The machine's animation should follow the actual lookup and acquisition result. If H3 is missing, ambiguous, unavailable, or incompatible, the interface must say so and offer a useful next action instead of vending a fictional prize.

The ontology is for classification, discovery, and relationships; the grid is one browsing interface. Neither a grid position nor a display name should be treated as an immutable artifact identity.

## The cloning machine: published sources are not edited in place

Published content is an immutable source version. To modify it, the creator uses the **cloning machine** to make an explicit, independent working copy in memory.

The intended workflow is:

1. Choose a published MicroBundle or Experience version.
2. Inspect the source identity, version, provenance, dependencies, and any relevant permissions.
3. Clone it into a new draft with a new draft identity and an explicit link to the source version.
4. Make edits to the clone, never to the published source.
5. Preview and validate the changed draft.
6. Save a draft revision as needed.
7. Package and publish a new immutable version only after the creator explicitly requests publication and all required checks pass.

The clone should preserve the source's semantic contents and record what it was derived from. It should not blindly copy transient session state, runtime handles, host-specific resources, or secrets. Each category needs explicit copy semantics. References to dependencies may remain pinned references when that is the contract; they must not accidentally become mutable shared copies.

### A real dirty-draft gate

A newly cloned draft is **not ready to publish**. The creator must make at least one meaningful, accepted authoring change before publication can be requested. Merely opening an inspector, moving a toy for presentation, or changing a camera does not count.

The authoring model should track:

- the exact source version and source revision;
- the draft's current revision;
- semantic changes accepted into the draft;
- whether the draft differs meaningfully from its cloned baseline;
- validation results scoped to a particular draft revision;
- preview results scoped to a particular draft revision;
- the artifact identity/version proposed for publication.

The publish control remains unavailable until a qualifying semantic change exists, the current draft passes required validation, and the preview/validation state is not stale where preview is required. If the creator reverts all semantic edits to the clone baseline, the draft becomes clean again and the gate closes.

This rule prevents accidental republishing of an unchanged copy and makes cloning an intentional act of creation rather than a shortcut around provenance. If a legitimate use case requires republishing an identical artifact under a different identity, that should be a separately named, policy-controlled operation—not a loophole in the normal edit-and-publish flow.

## Live preview is a contract, not a screenshot

A preview is useful only when it corresponds to an identifiable draft revision and an actual interpretation of that revision.

Each preview result should record, at minimum:

- draft identity and revision;
- preview mode and relevant host/runtime version;
- dependencies and versions used for resolution;
- validation and unresolved requirements;
- whether it is isolated or can affect external state;
- completion/failure status and diagnostics.

When a new edit arrives, the interface can immediately show the draft change and mark any previous preview as stale. The relevant preview path then updates or rebuilds. Fast operations should finish fast; expensive operations can be animated through real stage events, with the creator free to inspect other details while work proceeds.

A preview is not a publication, and a successful preview is not proof that every target host will behave identically. Host differences and unsupported capabilities must be visible.

## Where complexity belongs

The Forge does not hard-code the desired behavior of every possible creation. It provides a coherent path to declare, inspect, compose, validate, preview, and package the data and capabilities that define that creation.

- **Domain packages** define their own schemas and rules.
- **MicroBundleDomain** defines MicroBundle concepts and contracts it actually supports.
- **Ontology/catalog services** support discovery and classification without making a display address the artifact's identity.
- **ProtocolAi** is relevant to registered semantic integer identities and their meaning; it is not merely any string-to-integer conversion.
- **GrammarAi** is relevant when those identities are arranged into a valid compact instruction grammar.
- **FSM_COS** handles its actual composition/runtime-manifest responsibilities.
- **FSM_API** is the specific reference for real live FSM definition and lifecycle changes, using its public runtime-modification capabilities and safe deferred structural mutation.
- **The host** supplies the actual preview/runtime environment and its boundaries.

The Forge orchestrates these contracts and makes their work understandable. It should not duplicate their domain rules, fabricate simulation results, or imply a live runtime change when it only changed a draft.

## Actionable acceptance criteria

The first implementation should prove the authoring loop before investing in elaborate vending/cloning animations:

- [ ] A creator can select a schema-backed part and see its real editable fields.
- [ ] A valid edit changes the draft revision and the corresponding preview result.
- [ ] An invalid edit produces an actionable diagnostic and preserves the last valid draft.
- [ ] A stale preview is visibly marked and cannot be mistaken for the current draft.
- [ ] Published source artifacts are read-only in the normal authoring flow.
- [ ] Cloning records the exact source version and creates an independent draft.
- [ ] A clone cannot enter the normal publish flow until a meaningful semantic change is accepted.
- [ ] Reverting all semantic changes closes the publish gate again.
- [ ] H3 resolves through real catalog/ontology data; missing or ambiguous selections do not create fictional artifacts.
- [ ] Assembly and preview visuals are driven by real pipeline events and outcomes, never artificial progress or delay.
- [ ] The same authoring operations remain available without the diegetic vending machine or cloning machine.
- [ ] Building or publishing an artifact is not conflated with changing a live FSM or running an Experience.

The first revisioned in-memory draft model is implemented, but it is deliberately narrower than this full workflow: it has no published artifact/version provenance, typed schema-field editing, validation service, preview host, clone-from-published operation, or publication gate. The schema inspector remains read-only, and the vending-machine presentation is not implemented.

---

## Related design notes

- [MicroBundle Assembly Adventure](microbundle-assembly-adventure.md) — the claw, package slot, assembly crank, honest processing show, and artifact handoff.
- [Diegetic Experience Authoring](diegetic-experience-authoring.md) — the broader authoring model, tool/world relationship, and portable artifact boundary.
- [Experience Composition](experience-composition-design.md) — the current core and proposed nested composition model.

---

<p align="center"><em>The Singularity Workshop — Tools for the curious, the bold, and the systemically inclined.</em><br><strong>Because state shouldn't be a mess.</strong></p>
