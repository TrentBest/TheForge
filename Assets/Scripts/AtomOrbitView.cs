
// Assets/Scripts/Gui/AtomOrbitView.cs
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts
{
    /// <summary>
    /// 2D atomic model viewer using UI Toolkit VisualElements (rings & dots).
    /// - Nucleus rendered as clustered protons/neutrons (small colored dots).
    /// - Orbits rendered as rings with electron dots evenly spaced and optionally animated.
    /// </summary>
    public class AtomOrbitView : VisualElement
    {
        // --- Appearance ---
        public Color ProtonColor { get; set; } = new Color(0.9f, 0.35f, 0.35f, 1f);
        public Color NeutronColor { get; set; } = new Color(0.65f, 0.65f, 0.65f, 1f);

        public Color RingColor { get; set; } = new Color(0.75f, 0.75f, 0.75f, 1f);
        public Color ElectronColor { get; set; } = new Color(0.2f, 0.7f, 1.0f, 1f);

        /// <summary>Nucleus target radius (the cluster will auto-scale up if needed).</summary>
        public float NucleusRadius { get; set; } = 20f;
        public float NucleusDotRadius { get; set; } = 4f;
        public float NucleusRingStep { get; set; } = 1.9f; // multiplier on dot diameter for ring spacing

        public float RingThickness { get; set; } = 2f;
        public float RingSpacing { get; set; } = 30f;
        public float ElectronRadius { get; set; } = 6f;

        // --- Animation ---
        public bool Animate { get; set; } = true;
        public float AngularSpeed { get; set; } = 0.6f; // radians/sec base speed

        // --- Data ---
        private readonly List<int> shells = new();
        private int protonCount;
        private int neutronCount;

        // --- Visual caches ---
        private readonly List<VisualElement> ringEls = new();
        private readonly List<VisualElement> electronEls = new();
        private readonly List<VisualElement> nucleusDots = new();

        private IVisualElementScheduledItem tickItem;
        private float t; // time accumulator

        public AtomOrbitView()
        {
            style.position = Position.Relative;
            style.height = 300;
            style.width = 300;
            style.backgroundColor = new Color(0, 0, 0, 0); // transparent

            // Run animation tick if enabled
            tickItem = this.schedule.Execute(Tick).Every(16); // ~60 FPS
        }

        /// <summary>Set proton/neutron counts and rebuild the nucleus cluster.</summary>
        public void SetNucleus(int protons, int neutrons)
        {
            protonCount = Mathf.Max(0, protons);
            neutronCount = Mathf.Max(0, neutrons);
            RebuildNucleus();
        }

        /// <summary>Set shells (list of electron counts per shell) and rebuild rings/electrons.</summary>
        public void SetShells(List<int> shellCounts)
        {
            shells.Clear();
            if (shellCounts != null) shells.AddRange(shellCounts);
            RebuildShells();
        }

        /// <summary>Set the overall view size.</summary>
        public void SetViewSize(float width, float height)
        {
            style.width = width;
            style.height = height;
            // Rebuild both nucleus and shells because center/space may change
            RebuildNucleus();
            RebuildShells();
        }

        // ---------------- Rebuild nucleus ----------------
        private void RebuildNucleus()
        {
            // Remove prior dots
            foreach (var el in nucleusDots) Remove(el);
            nucleusDots.Clear();

            var (center, resolvedW, resolvedH) = GetCenter();
            if (float.IsNaN(resolvedW) || float.IsNaN(resolvedH)) return;

            int total = protonCount + neutronCount;
            if (total <= 0) return;

            // Decide effective radius: grow if we need more room for many nucleons
            float baseR = NucleusRadius;
            float dotD = NucleusDotRadius * 2f;
            // Estimate how many rings we need. This is a heuristic, not exact packing.
            int estimatedRings = Mathf.CeilToInt(Mathf.Max(1, total / 6f));
            float neededR = Mathf.Max(baseR, dotD + (estimatedRings - 1) * (NucleusRingStep * NucleusDotRadius));
            float effectiveR = neededR;

            // Generate positions in concentric rings (simple close-ish packing)
            var positions = GenerateConcentricPositions(total, center, effectiveR - NucleusDotRadius, NucleusDotRadius);

            // Create dots: first protons, then neutrons
            for (int i = 0; i < positions.Count; i++)
            {
                bool isProton = i < protonCount;
                var color = isProton ? ProtonColor : NeutronColor;
                var dot = MakeCircle(NucleusDotRadius, color);
                PositionCircle(dot, positions[i], NucleusDotRadius);
                Add(dot);
                nucleusDots.Add(dot);
            }

            MarkDirtyRepaint();
        }

        // ---------------- Rebuild shells/electrons ----------------
        private void RebuildShells()
        {
            // Clear rings/electrons
            foreach (var el in ringEls) Remove(el);
            foreach (var el in electronEls) Remove(el);
            ringEls.Clear();
            electronEls.Clear();

            var (center, resolvedW, resolvedH) = GetCenter();
            if (float.IsNaN(resolvedW) || float.IsNaN(resolvedH)) return;

            // Rings & electrons
            for (int s = 0; s < shells.Count; s++)
            {
                float r = Mathf.Max(NucleusRadius, EstimateNucleusRadiusForCounts()) + RingSpacing * (s + 1);

                // Ring
                var ring = MakeRing(r, RingThickness, RingColor);
                PositionRing(ring, center, r);
                Add(ring);
                ringEls.Add(ring);

                // Electrons
                int eCount = shells[s];
                for (int i = 0; i < eCount; i++)
                {
                    var eDot = MakeCircle(ElectronRadius, ElectronColor);
                    PositionElectron(eDot, center, r, i, eCount, 0f); // initial angle offset
                    Add(eDot);
                    electronEls.Add(eDot);
                }
            }

            MarkDirtyRepaint();
        }

        private float EstimateNucleusRadiusForCounts()
        {
            // A lightweight estimate to keep shells away from the nucleus cluster.
            int total = protonCount + neutronCount;
            if (total <= 1) return NucleusRadius;
            float dotD = NucleusDotRadius * 2f;
            int rings = Mathf.CeilToInt(Mathf.Max(1, total / 6f));
            return Mathf.Max(NucleusRadius, dotD + (rings - 1) * (NucleusRingStep * NucleusDotRadius));
        }

        private void Tick()
        {
            if (!Animate || shells.Count == 0) return;

            t += 0.016f; // ~16ms

            var (center, resolvedW, resolvedH) = GetCenter();
            if (float.IsNaN(resolvedW) || float.IsNaN(resolvedH)) return;

            int electronIndex = 0;
            for (int s = 0; s < shells.Count; s++)
            {
                float baseR = Mathf.Max(NucleusRadius, EstimateNucleusRadiusForCounts());
                float r = baseR + RingSpacing * (s + 1);
                int eCount = shells[s];
                float angleOffset = AngularSpeed * t / (1 + 0.2f * s);

                for (int i = 0; i < eCount; i++)
                {
                    var eDot = electronEls[electronIndex++];
                    PositionElectron(eDot, center, r, i, eCount, angleOffset);
                }
            }
        }

        // ------- helpers -------
        private (Vector2 center, float w, float h) GetCenter()
        {
            float w = resolvedStyle.width;
            float h = resolvedStyle.height;
            if (float.IsNaN(w) || float.IsNaN(h))
            {
                w = style.width.value.value;
                h = style.height.value.value;
            }
            return (new Vector2(w * 0.5f, h * 0.5f), w, h);
        }

        private VisualElement MakeCircle(float radius, Color color)
        {
            var el = new VisualElement
            {
                style =
                {
                    position = Position.Absolute,
                    width = radius * 2f,
                    height = radius * 2f,
                    backgroundColor = color,
                    borderTopLeftRadius = radius,
                    borderTopRightRadius = radius,
                    borderBottomLeftRadius = radius,
                    borderBottomRightRadius = radius
                }
            };
            return el;
        }

        private VisualElement MakeRing(float radius, float thickness, Color color)
        {
            var ring = new VisualElement
            {
                style =
                {
                    position = Position.Absolute,
                    width = radius * 2f,
                    height = radius * 2f,
                    backgroundColor = new Color(0,0,0,0),
                    borderTopLeftRadius = radius,
                    borderTopRightRadius = radius,
                    borderBottomLeftRadius = radius,
                    borderBottomRightRadius = radius,
                    borderTopWidth = thickness,
                    borderRightWidth = thickness,
                    borderBottomWidth = thickness,
                    borderLeftWidth = thickness,
                    borderTopColor = color,
                    borderRightColor = color,
                    borderBottomColor = color,
                    borderLeftColor = color
                }
            };
            return ring;
        }

        private void PositionCircle(VisualElement circle, Vector2 center, float radius)
        {
            circle.style.left = center.x - radius;
            circle.style.top = center.y - radius;
        }

        private void PositionRing(VisualElement ring, Vector2 center, float radius)
        {
            ring.style.left = center.x - radius;
            ring.style.top = center.y - radius;
        }

        private void PositionElectron(VisualElement electron, Vector2 center, float ringRadius, int i, int count, float angleOffset)
        {
            if (count <= 0) return;
            float angle = angleOffset + (Mathf.PI * 2f) * (i / (float)count);
            float ex = center.x + ringRadius * Mathf.Cos(angle);
            float ey = center.y + ringRadius * Mathf.Sin(angle);
            float r = ElectronRadius;

            electron.style.left = ex - r;
            electron.style.top = ey - r;
        }

        /// <summary>
        /// Generate positions in concentric rings for 'count' dots inside maxRadius.
        /// This is a simple, visually pleasing layout (not strict close packing).
        /// </summary>
        private List<Vector2> GenerateConcentricPositions(int count, Vector2 center, float maxRadius, float dotRadius)
        {
            var pts = new List<Vector2>(count);
            if (count <= 0) return pts;

            float ringStep = NucleusRingStep * dotRadius;  // e.g., ~1.9 * r
            int placed = 0;
            int ringIdx = 0;

            while (placed < count)
            {
                float r = ringIdx == 0 ? 0f : ringIdx * ringStep;
                if (r > maxRadius) r = maxRadius;

                int onRing = ringIdx == 0 ? 1 : Mathf.Max(6, Mathf.FloorToInt(2f * Mathf.PI * r / (dotRadius * 2f)));
                for (int j = 0; j < onRing && placed < count; j++)
                {
                    float angle = (Mathf.PI * 2f) * (j / (float)onRing);
                    float x = center.x + r * Mathf.Cos(angle);
                    float y = center.y + r * Mathf.Sin(angle);
                    pts.Add(new Vector2(x, y));
                    placed++;
                }
                ringIdx++;
                if (r >= maxRadius && placed < count)
                {
                    // If we run out of room, keep placing at max radius.
                    // (Visual compromise for very large counts.)
                }
            }

            return pts;
        }
    }
}
