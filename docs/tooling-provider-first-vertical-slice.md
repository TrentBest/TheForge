# Forge Tooling Provider: First Vertical Slice

> **Status:** The read-only schema-inspection adapter, immutable typed field-value model, schema-value validator, revision-safe draft edits, and exact-ID/version codec-backed payload path are implemented and tested. The codec round trip currently uses a test-only fixture; a production codec, provider API, and spatial Forge remain future work.

## Purpose

Prove that Forge can host optional MicroBundle authoring tools without hard-coding domain-specific UI and without introducing renderer dependencies into MicroBundleDomain. Start in a conventional, non-spatial host. A spatial Forge can later present the same contributions inside its tethered authoring space.

## Participants

1. **Schema-only capability:** provides a MicroBundle description/schema but no bespoke authoring tool. A generic inspector handles supported fields.
2. **Tool-enabled capability:** provides a schema and an optional domain-specific authoring contribution.
3. **Tool host:** discovers contributions, checks compatibility and permissions, manages lifecycle, and routes edits through a narrow authoring boundary.
4. **Authoring model:** stores deliberate, validated changes to Experience content. The typed-value validator now establishes a narrow schema-checking seam, but it is not yet connected to revisioned draft edits.
5. **Session state:** stores selection and presentation preferences separately from the portable Experience.

The existing MicroBundleDomain description contract can inform generic inspection. It is not, by itself, a spatial representation or interaction contract. The exact provider API remains undecided until these participants can be tested against real package types.

## Acceptance checks

| Scenario | Required evidence |
|---|---|
| Capability has no bespoke tool | It remains usable; generic inspection is available when its schema supports it |
| Capability has a bespoke tool | The host discovers it without a domain-specific Forge code branch |
| Provider is incompatible or unavailable | The host reports the state without disabling unrelated authoring |
| Requested permission is denied | The denied operation cannot proceed and the failure is visible |
| Provider proposes an invalid edit | The authoring boundary rejects it and preserves the prior valid model |
| Creator selects, moves, or closes a tool | Portable Experience data remains unchanged |
| Creator accepts a configuration edit | The model records the validated change and its origin |
| Spatial presentation is unavailable | A non-spatial fallback or explicit unsupported state is available |

These checks do not prove spatial tethering, cross-host portability, sandboxing, or publication security. Those need separate evidence.

## Spatial follow-up

Only after the non-spatial seam is demonstrated:

1. Define a host-neutral tether target and resolution status.
2. Begin with one stable host-supported anchor type.
3. Keep tether, relative transform, tool placement, selection, and focus in separately versioned session state.
4. Make unresolved targets visible; never silently attach the Forge to an unrelated location.
5. Let providers request layout constraints while the Forge retains placement and collision control.
6. Verify that relocating the Forge cannot mutate Experience identity, composition, or configuration.
7. Exercise the same tool through a non-spatial fallback.

## Dependency rule

MicroBundleDomain defines capability and description semantics. It must not depend on Forge, FSM_COS, or a renderer. Forge consumes domain contracts and hosts optional tooling. A concrete host renders the Forge. Tooling presence is not permission to access arbitrary data or perform privileged operations.

## Verified MicroBundleDomain contract findings

The current `master` source of `TheSingularityWorkshop.MicroBundleDomain` confirms the following:

- `MicroBundleDefinition` is an editor-time description with a human-readable name, a `MicroBundleDescriptor`, and recursively inspectable `MicroBundleField` entries.
- `MicroBundleFieldKind` currently defines `String`, `Integer`, `Float`, `Boolean`, and `Object`. Object fields can contain child fields; numeric fields can declare inclusive minimum and maximum bounds.
- Field defaults are exposed as `object?`; the schema does not currently provide a complete portable value-type, validation, serialization, or edit-application contract. A generic inspector must not assume that a declared default alone defines all validation rules.
- `MicroBundleDescriptor` exposes identity, version, dependencies, and opaque provider identifiers. The domain preserves provider IDs without assigning them presentation or tooling semantics.
- `IMicroBundle` is an executable runtime contract. Forge authoring tools should not require loading or executing an `IMicroBundle` merely to inspect its `MicroBundleDefinition`.
- The domain schema has no host geometry, spatial anchor, rendering, input, lifecycle, permission, or authoring-session contract. Those remain Forge/host concerns unless a separately justified domain-neutral contract is designed.

The source and tests demonstrate schema construction, nested fields, bounds, duplicate-name rejection, descriptor providers, and runtime contract behavior. They do **not** yet demonstrate generic editing of serialized configuration, safe application of edits to a portable Experience, or a bespoke authoring-provider lifecycle.

**Design consequence:** the first vertical slice should adapt `MicroBundleDefinition` and `MicroBundleField` into a read-only generic inspector first. Define a separate validated edit model before adding write controls. Keep bespoke tools as an optional Forge-side contribution; do not reinterpret `MicroBundleProvider` as a tool-provider contract without an explicit compatibility design.

## Implemented first step: read-only schema inspection

The Forge core now exposes `MicroBundleSchemaInspector.Inspect(MicroBundleDefinition)`. It produces a detached inspection snapshot containing the definition's name, identity, version, dependency IDs, provider IDs, and recursively nested field metadata.

- Field kinds and inclusive numeric bounds are preserved.
- Default values are projected to invariant display text; they are not treated as editable values or serialized configuration.
- The returned collections are read-only snapshots.
- Inspection accepts the editor-time definition and does not load or execute an `IMicroBundle`.
- Automated tests cover identity and descriptor projection, nested fields, defaults and bounds, read-only collection behavior, and null input.

This is deliberately an inspection model, not a write path. It does not validate or deserialize the opaque configuration bytes held by a `ForgeMicroBundle`, prove that runtime configuration matches the schema, apply edits, discover bespoke tools, enforce provider permissions, or implement spatial tethering. The current domain contract does not provide enough information to safely infer those behaviors.

## Decision gate

Do not finalize public provider interfaces until a small prototype establishes:

- the exact description/schema types available in the current MicroBundleDomain package;
- whether the generic inspector can read those types without executing a capability;
- how accepted edits are validated and attributed;
- how provider compatibility, failure, and permission denial are represented;
- how the same authoring action works without spatial presentation.

---

<p align="center"><em>The Singularity Workshop — Tools for the curious, the bold, and the systemically inclined.</em><br><strong>Because state shouldn't be a mess.</strong></p>
