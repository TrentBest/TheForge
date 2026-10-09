# Forge-Owned Configuration and Manifest Boundary

> **Status:** Architecture decision for the provider-neutral Forge authoring core. The draft model, immutable typed field-value model, schema-value validator, and compilation against Forge's currently referenced FSM_COS package are implemented. Durable configuration files and codec-backed payload generation remain future work.

## Decision

**Forge owns the authored Experience, its configuration documents, and the manifest source from which runtime-facing artifacts are produced.** FSM_COS does not become the editor or the owner of Forge project files.

A creator's edits to an Experience belong in Forge even when the resulting configuration will eventually be consumed by another package. Forge is responsible for maintaining the draft, tracking semantic changes, validating what it can validate, and preparing the artifact that crosses the runtime boundary.

Ownership of the authored data does not mean Forge should invent the meaning of every domain field or the byte representation expected by every MicroBundle. Those meanings and representation rules must be explicit contracts.

## Responsibilities by boundary

| Boundary | Owns | Does not own |
|---|---|---|
| **Forge** | Experience drafts; authored configuration documents; composition; manifest source; edit history/revision; authoring validation; artifact preparation | Hidden assumptions about a MicroBundle's private payload encoding; live runtime execution |
| **MicroBundleDomain** | Neutral MicroBundle identity, descriptors, provider/dependency declarations, and inspectable field descriptions | Forge UI, Forge project files, a universal configuration wire format |
| **Domain-specific configuration codec** | Mapping between typed, schema-backed values and the representation a particular MicroBundle actually consumes; validation that requires domain semantics | The Forge draft lifecycle or overall Experience manifest |
| **FSM_COS** | Its runtime manifest contract, composition, dependency resolution, loading/arbitration and runtime-facing responsibilities | Editing Forge project/configuration files or becoming the canonical authoring workspace |
| **FSM_API** | The state-machine definitions, contexts, lifecycle, processing groups, and runtime modification semantics it actually exposes | Forge's authored project model or FSM_COS's composition policy |
| **Host** | Hosting/execution environment and host-specific effects or resources | Silent mutation of the creator's canonical draft |

## Three things that must not be conflated

1. **The authored configuration document** is Forge-owned creator intent. It should preserve typed values, structure, identity, and versioning as the authoring contract requires.
2. **The runtime configuration payload** is the representation a particular MicroBundle expects. It may be bytes, but its encoding cannot safely be inferred from the display text of a schema default.
3. **The FSM_COS runtime manifest** is the machine-oriented composition contract Forge compiles toward. It is an output of authoring, not a replacement for the Forge project or its editable configuration documents.

These are related artifacts, not interchangeable files. Compilation may produce a runtime manifest and configuration payloads without transferring ownership of the authored source to FSM_COS.

## Integration finding: published package versus development source

Forge currently references `TheSingularityWorkshop.FSM_COS` version `0.1.0-alpha.3`. Its existing compiler targets that package's `BundleRequest`-based manifest API and passes opaque configuration bytes through that request. **This is a deliberately limited bridge, not a lossless composition compiler:** `ForgeExperience.Compile()` currently emits only each MicroBundle's numeric ID and opaque configuration. It does not carry the authored descriptor version, dependencies, provider declarations, or the Experience's ontology into the resulting runtime manifest. Until Forge migrates to a released contract that can represent the required identity/version and configuration boundary, callers must not treat this output as proof that the complete authored composition has been preserved or resolved.

The FSM_COS `development` source has since clarified a different boundary: `RuntimeManifest` contains versioned `MicroBundleManifestEntry` roots, while configuration is supplied through a separate configuration-source contract. The manifest describes **which roots and versions** to compose; it is not the configuration document or the configuration payload.

These contracts must not be treated as interchangeable. Until Forge deliberately adopts a compatible released FSM_COS version, its build must continue targeting the package it actually references. When that migration is undertaken, Forge should compile its authored manifest source into the new versioned root entries and adapt configuration delivery separately. It must not silently depend on an unpublished development API, nor force Forge-owned editable configuration into the runtime manifest.

The FSM_COS development branch also records a passed correctness gate for duplicate roots and conflicting requested versions. Forge's authoring validation should report conflicting versions before runtime composition where possible; FSM_COS must still enforce its own runtime correctness rules.

## Typed editing without a guessed serializer

MicroBundleDefinition and MicroBundleField provide useful editor-time metadata: field kinds, defaults, numeric bounds, and nested children. Forge's MicroBundleSchemaInspector projects that information into a detached, read-only view. Its DefaultValueDisplay is formatted text for inspection; it is not a typed value store or a serialization contract. Forge now also has an immutable ForgeFieldValue model and ForgeFieldValueValidator for String, Integer, Float, Boolean, and nested Object values. The validator checks kind compatibility, numeric bounds, nested paths, and unknown supplied fields. Missing fields remain omitted; defaults are not silently injected. The draft now accepts typed field edits through `TrySetFieldValue`, validating before acceptance and advancing its revision only for a changed, valid value. It requires an exact MicroBundle ID/version schema match. `ToExperience()` refuses to compile a draft that contains typed edits until a compatible codec exists, so values cannot be silently discarded.

The next authoring implementation should therefore stay Forge-owned and separate these responsibilities:

1. Keep editable values typed in the draft rather than round-tripping through formatted display strings. **The detached value model now exists; wiring it into the draft is the next increment.**
2. Validate a proposed edit before accepting it. A rejected edit must preserve the last valid draft and must not advance its revision. The validator now exists, but revision-safe edit acceptance is not yet implemented.
3. Let Forge own the document, edit lifecycle, and artifact preparation.
4. Use an explicit codec/adapter when a schema-backed value must become a MicroBundle's runtime payload. The codec must define validation, encoding, decoding (where supported), and compatibility/version behavior.
5. Refuse to claim that a typed edit is ready for runtime use when no compatible codec exists. Opaque payload editing remains available as a separate, honest capability.
6. Keep Forge's manifest compiler pinned to the actual referenced FSM_COS package contract. Treat the newer development manifest/configuration-source contract as a deliberate compatibility migration, not an implicit assumption.

The schema description alone does not supply enough information to invent the codec. In particular, a generic binary package provides byte-stream primitives, not automatic semantics for every schema. Forge must not silently assume JSON, a universal binary layout, or a field order that the consuming MicroBundle never declared.

## Implementation status

- **Implemented, limited:** ForgeExperience owns the editor-side model and emits a minimal manifest against the FSM_COS API version currently referenced by Forge. The output currently omits descriptor versions, dependencies/providers, and ontology; it is not a lossless composition artifact.
- **Implemented:** ForgeExperienceDraft provides revisioned in-memory edits to Experience name, ontology, composition, and opaque configuration payloads.
- **Implemented:** MicroBundleSchemaInspector exposes detached read-only field metadata, including nested fields and bounds.
- **Not implemented:** a durable Forge project/configuration file format and round-trip persistence workflow.
- **Not implemented:** a typed editable-value tree, typed validation pipeline, or codec-backed conversion from typed values to a MicroBundle's runtime bytes.
- **Not implemented:** migration to the newer FSM_COS development manifest and separate configuration-source contract.
- **Not implemented:** a revision-scoped live preview pipeline or publication workflow.

Do not describe these future capabilities as complete merely because the schema can be inspected or a manifest can be compiled.

## Acceptance criteria for the next increment

- [ ] Forge can represent a field edit as a typed value without converting it to display text first.
- [ ] Supported field kinds have explicit type and range validation, including nested object structure.
- [ ] Invalid edits leave the last valid draft and its revision intact.
- [ ] The codec contract is explicit and associated with the intended MicroBundle identity/version.
- [ ] Encode/decode round trips are tested for codecs that support both directions; unsupported or malformed payloads fail visibly.
- [ ] Culture changes do not alter numeric encoding or validation results.
- [ ] A missing or incompatible codec is reported as a limitation, not hidden behind a guessed format.
- [ ] Runtime-manifest compilation remains a separate step from saving the Forge-owned source document.
- [ ] Any migration to the newer FSM_COS manifest contract is explicit and targets a compatible package version.
- [ ] No change requires FSM_COS to own or edit Forge project files.

---

<p align="center"><em>The Singularity Workshop — Tools for the curious, the bold, and the systemically inclined.</em><br><strong>Because state shouldn't be a mess.</strong></p>
