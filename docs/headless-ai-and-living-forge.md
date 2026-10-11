# HeadlessAi and the Living Forge
## Direction note — 2026-10-09

> **Status: Proposed architecture and experience direction.** This records a product insight, not an implementation claim. HeadlessAi does not yet exist as a released package, and the Hermit/minion/Titan experience is not implemented.

## The central idea

The Forge should use AI to reduce cognitive friction and ambiguity while remaining a real, inspectable authoring system. AI can help a person explore possibilities, explain consequences, draft changes, and identify unclear intent; it must not become an opaque authority that silently changes the authored Experience.

Two layers work together:

1. **HeadlessAi** is a standalone, provider-neutral .NET package for configuring and making HTTP requests to AI endpoints. It is an agent/transport integration layer, not an AI model.
2. **The Forge's living, diegetic experience** is a host-facing manifestation in which agent activity and authored-world changes can be expressed visually, spatially, and theatrically.

HeadlessAi must remain useful without The Forge. The Forge may consume it; it must not own or absorb it.

## Naming and vocabulary

- **AI** (capital A, capital I): the broad field/technology, and colloquial reference to the model when appropriate.
- **Ai** (capital A, lowercase i): a Workshop naming convention for technology that touches AI but is not itself an AI model. Existing examples include ProtocolAi and GrammarAi; HeadlessAi follows that convention.
- **HeadlessAi**: the proposed package name for the headless HTTP-agent capability.
- **Horsemen / riders**: metaphorical names for configured endpoint agents and additional agents created from a configured agent type. These are not provider-specific protocol terms.
- **Minions, Hermit, Titans**: visual/behavioral scale classes for agents or workers manifested inside the Forge. They are presentation and orchestration concepts, not synonyms for LLM providers.

The Sleepy Hollow / Ichabod Crane association is flavor and visual inspiration, not a requirement that constrains the transport API.

## HeadlessAi package boundary

HeadlessAi should own reusable mechanics for communicating with configured AI endpoints over HTTP. A user configures a provider/endpoint profile with the information required to form the request, such as:

- endpoint URL and request method where applicable;
- authentication configuration supplied securely by the host;
- required headers and provider-specific request metadata;
- model identifier and supported request options;
- request-body construction or a provider adapter that knows the endpoint's contract;
- response extraction/normalization, error reporting, cancellation, timeouts, and retry policy;
- capabilities/limitations and a stable identity for the configured agent profile.

Treat GPT, Gemini, Claude, and other endpoints as examples of provider families, not as proof that one generic request body works everywhere. Their request and response contracts differ and evolve. Prefer explicit provider adapters/capability descriptions over a giant bag of universal fields or hard-coded assumptions.

### Security and operational constraints

- Never store API keys in ordinary configuration documents, logs, source control, or Forge Experience artifacts. Resolve secrets through host-provided secret storage or a secure credential callback.
- Do not let model output become executable code or arbitrary tool invocation by default.
- Tool permissions must be explicit, narrow, observable, and revocable.
- Bound response size, retries, concurrency, token/cost budgets, and request duration.
- Preserve cancellation and surface provider failures honestly.
- Redact credentials and sensitive request data from diagnostics.
- Keep network transport, provider-specific wire formats, orchestration policy, and UI presentation as separable responsibilities.

### Optional ecosystem integrations

ProtocolAi and GrammarAi may be integrated, but must not be mandatory dependencies of HeadlessAi.

- **ProtocolAi** could supply optional vocabulary/identity/protocol mapping where useful.
- **GrammarAi** could supply optional grammar- or expression-oriented interpretation and generation.
- The package should still support a straightforward configured HTTP agent with neither package installed.

The package should not pretend all providers share one native schema. It should make differences explicit and provide a stable host-facing seam.

## From one configured horseman to a company of riders

A configured endpoint profile is a reusable definition. The system may create multiple agent instances from it, each with its own identity, conversation/request context, budgets, permissions, and lifecycle. Do not confuse a reusable provider profile with a running agent instance.

Potential roles can include:
- clarification: find ambiguity and ask focused questions;
- planner: propose a sequence of bounded changes;
- critic: challenge assumptions and identify contradictions;
- researcher: gather supporting evidence through explicitly permitted tools;
- implementer: prepare changes for review;
- narrator: explain what is happening in the Forge.

These are examples, not hard-coded mandatory agent classes. The orchestration layer should support user-configured roles and policies. Multiple agents do not automatically make an answer more reliable; disagreement, provenance, and unresolved uncertainty should remain visible.

## The Forge as a living, diegetic environment

The envisioned Forge is not just a form-driven editor with an animated background. Its tools and changes should inhabit a visible environment. A full-sized humanoid agent named **Hermit** can explore tools, test possibilities, and demonstrate what parameters might be changed. Hermit may be playful or theatrical—“clowning around”—while still making purposeful, legible contributions.

The experience should help the user understand:
- what can be changed;
- what a candidate change would look like;
- what it affects and why;
- what remains uncertain;
- what is proposed versus accepted;
- what has actually been applied to the authored model or live runtime.

Hermit's idle experimentation should be sandboxed. It may discover options or prepare previews, but it must not silently commit authored changes or mutate a live runtime.

## Visual scale language

The proposed visual scale classes are relative to Hermit:

| Class | Relative scale | Intended expression |
|---|---:|---|
| Minion | 1/3 Hermit's size | Many small agents handling fine-grained, repetitive, or distributed visual work |
| Hermit | 1× | The full-size humanoid guide/agent; interacts directly with tools and demonstrates choices |
| Titan | 3× Hermit's size | A few large agents representing heavyweight, broad-scope, or high-impact work |

Scale is a visual metaphor, not a literal measure of intelligence, authority, or execution speed. A Titan should not receive more permission just because it is large; a Minion should not be treated as less trustworthy just because it is small.

### Changes should be staged, not teleported

If the user changes the age of a distant mountain range, the Forge should not merely replace one value and instantly swap the landscape. The intended experience is that the system:

1. identifies the changed parameter and its scope;
2. determines which world elements or generated representations depend on it;
3. explains or previews the expected consequence;
4. stages a visual task/event plan;
5. manifests many Minions and a few Titans racing toward the distant range;
6. shows the affected environment changing as work progresses;
7. reports completion, partial completion, or failure, with a way to inspect the actual result.

The count of visible workers can be an artistic representation of work, not a promise that thousands of real independent processes are running. The visualization must remain grounded in real task state: planned, queued, running, completed, failed, or cancelled. Avoid fake progress that claims changes happened when they did not.

This is the proposed **visible consequence pipeline**: intent → impact analysis → preview/approval → staged work → observable change → verification.

## Trust and authorship

AI suggestions should reduce ambiguity without erasing the user's authority.

- Distinguish suggestion, draft edit, accepted authored change, and live runtime mutation.
- Preserve the user's ability to inspect, revise, reject, undo, or replay supported operations.
- Show the target, scope, rationale, and uncertainty for consequential changes.
- Require explicit approval for destructive, expensive, external, or live-runtime actions.
- Keep a provenance trail from user intent through agent suggestions to the final applied operation.
- If impact cannot be determined, show uncertainty rather than inventing a confident animation.
- The animation may dramatize real work, but it must not replace verification.

## Architectural ownership

| Responsibility | Proposed owner |
|---|---|
| HTTP request/response mechanics and provider adapters | HeadlessAi |
| Optional vocabulary/protocol mapping | ProtocolAi |
| Optional grammar/expression capability | GrammarAi |
| Authored Experience, draft edits, validation, provenance, composition intent | The Forge core |
| MicroBundle descriptors and neutral schemas | MicroBundleDomain |
| Artifact storage and immutable retrieval | MicroBundleRepository |
| Runtime composition and assembly | FSM_COS |
| Runtime FSM behavior and lifecycle | FSM_API |
| Agent permissions, tool mediation, execution loop, spatial manifestation | Host/runtime integration, with explicit contracts |
| Hermit/minion/Titan appearance and theatrical staging | Forge presentation/host layer, driven by real state |

Do not make HeadlessAi depend on Forge, FSM_COS, ProtocolAi, or GrammarAi merely to support the baseline HTTP-agent use case. Avoid putting 3D transforms, renderer objects, or humanoid concepts in the headless transport package.

## Suggested implementation order

1. Specify HeadlessAi's neutral contract: configured provider profile vs agent instance; request/response abstraction; secret resolution; cancellation/timeouts; errors; capability declaration; and extension points.
2. Survey actual provider contracts for the first target providers. Keep provider adapters testable with recorded or fixture HTTP responses; do not require live API credentials in CI.
3. Implement one real provider adapter end to end, with redaction, cancellation, deterministic tests, and explicit failure handling.
4. Add a second provider to validate that the abstraction captures genuine differences without becoming a lowest-common-denominator trap.
5. Define optional ProtocolAi/GrammarAi adapters only where they deliver concrete value; keep the base package usable without them.
6. Define agent/task contracts for Forge: identity, role, permissions, budgets, context, task states, proposals, and provenance. Keep these separate from raw HTTP transport.
7. Prototype Hermit in one bounded Forge task: show one real parameter's current value, candidate change, impact explanation, preview, and explicit accept/reject.
8. Add Minion/Titan staging for a task with real observable progress. Start with a small count; scale visual representation only after task-state mapping is proven.
9. Connect to host runtime only after authoring semantics and safety boundaries are testable.

## Decisions still required

- Does HeadlessAi's public API model generic HTTP requests, provider profiles, agent sessions, or a small composition of all three?
- Which provider adapters are in the first supported slice?
- What is the stable identity/version scheme for provider profiles and agent definitions?
- Which component owns conversation history, context-window management, and cost accounting?
- How are tools declared, authorized, and invoked without turning model output into unrestricted execution?
- What persistence format stores provider configuration while keeping secrets external?
- What is the minimum host-independent task/proposal contract for the Forge?
- Which operations are safe to preview automatically, and which require explicit user confirmation?
- How does a visual worker count map to actual work items while avoiding misleading progress?
- What spatial/runtime host is the first target for Hermit, and what real state will drive its animation?

## First recommended vertical slice

Build a **headless, credential-safe provider profile and one HTTP adapter** with fixture-driven tests. It should prove that a configured agent can form a request, send it through an injected HTTP transport seam, parse a response, cancel safely, and return a structured diagnostic without leaking secrets. Do not start with a multi-agent swarm or the animated Hermit.

In parallel, keep the Forge's first visual proof deliberately small: Hermit inspects one real schema parameter, proposes one typed edit, shows the preview/impact, and waits for acceptance. The theatrical scale system follows when changes and their task states are real.

---

<p align="center"><em>The Singularity Workshop — Tools for the curious, the bold, and the systemically inclined.</em><br><strong>Because state shouldn't be a mess.</strong></p>
