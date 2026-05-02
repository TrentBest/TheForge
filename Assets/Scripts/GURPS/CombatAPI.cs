using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.Core.Memory;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.GURPS
{
    /// <summary>
    /// The Sovereign Combat Engine for DURPS.
    /// Resolves Success Rolls, Active Defenses, and Damage Mitigation.
    /// </summary>
    public class CombatAPI : IGurpsApiProvider
    {
        public string ModuleName => "Tactical Combat Engine";
        public string Title => "DURPS :: COMBAT RESOLVER";
        public Guid Id { get; set; } = Guid.NewGuid();

        private readonly DataWarehouse _warehouse;

        public CombatAPI() { _warehouse = new DataWarehouse(); }
        public CombatAPI(DataWarehouse warehouse) => _warehouse = warehouse;

        public void Initialize() { }

        // --- CORE SUCCESS LOGIC ---

        /// <summary>
        /// Performs a standard GURPS 3d6 Success Roll.
        /// Returns (Success/Failure, Margin).
        /// </summary>
        public (bool isSuccess, int margin) CheckSuccess(int effectiveSkill, int roll = -1)
        {
            if (roll == -1) roll = UnityEngine.Random.Range(1, 7) + UnityEngine.Random.Range(1, 7) + UnityEngine.Random.Range(1, 7);

            // Critical Success/Failure Logic
            if (roll == 3 || roll == 4) return (true, effectiveSkill - roll); // Critical
            if (roll == 18) return (false, effectiveSkill - roll);           // Critical Failure

            bool success = roll <= effectiveSkill;
            return (success, effectiveSkill - roll);
        }

        /// <summary>
        /// Calculates Active Defense (Dodge).
        /// Standard GURPS: (DX + HT) / 4 + 3.
        /// </summary>
        public int CalculateBaseDodge(int dx, int ht) => Mathf.FloorToInt((dx + ht) / 4f) + 3;

        // --- INTEGRATION: GPU BRIDGE ---

        public void DispatchMassCombat(ComputeBuffer actorBuffer, int count)
        {
            // This would trigger the GURPS_MassCombat.compute kernel
            Debug.Log($"[DURPS] Dispatching tactical resolution for {count:N0} entities...");
        }

        // --- IGuiProvider Stubs ---
        public Action<VisualElement> GetGuiBuilder() => root => { };
        public VisualElement CreateGui(GuiContext ctx) => new UnityEngine.UIElements.VisualElement();
        public void Bind(DataWarehouse warehouse) { }
        public void SaveToCache() { }
        public void LoadFromCache() { }
        public void ToUIDocument(string path) { }
        public void FromUIDocument(string path) { }
    }
}