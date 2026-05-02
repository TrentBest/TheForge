using System;
using System.Collections.Generic;
using UnityEngine;

namespace Workshop.GURPS
{
    /// <summary>
    /// The Geopolitical Blueprint for a societal entity.
    /// Reforged to include Reputation, Jurisdiction, and Member Trait Injections.
    /// </summary>
    [Serializable]
    public class GURPSFaction
    {
        // --- IDENTITY ---
        public string Name { get; set; } = "Unknown Syndicate";
        public string Description { get; set; } = "A shadowy collective operating in the fringe.";

        // --- GURPS SYSTEMIC VECTORS ---
        public int TechLevel { get; set; } = 8;        // TL 0 to TL 12+
        public int ControlRating { get; set; } = 3;   // CR 0 (Anarchy) to CR 6 (Totalitarian)
        public WealthTier BaseWealth { get; set; } = WealthTier.Average;

        // --- SOCIAL REACH & RECOGNITION ---
        public int RecognitionScore { get; set; } = 10; // 3d6 vs Recognition to see if an NPC knows them
        public Reach Jurisdiction { get; set; } = Reach.Planetary;

        // --- FACTION TRAIT INJECTIONS (The "Foot Clan" Logic) ---
        /// <summary>
        /// Global modifiers applied to all members of this faction.
        /// Example: { "IQ", -1 }, { "ST", +1 }, { "Brawling", +2 }
        /// </summary>
        public List<FactionModifier> MemberModifiers = new List<FactionModifier>();

        // --- DERIVED METRICS ---
        /// <summary>
        /// Calculates the "Friction Index" for operations within this faction's space.
        /// $$Friction = \frac{ControlRating \times 2}{RecognitionScore + 1}$$
        /// </summary>
        public float GetFrictionIndex() => (ControlRating * 2.0f) / (RecognitionScore + 1.0f);
    }

    [Serializable]
    public struct FactionModifier
    {
        public string AttributeOrSkill;
        public int Modifier;
    }

    public enum WealthTier { DeadBroke, Poor, Struggling, Average, Comfortable, Wealthy, FilthyRich }
    public enum Reach { Local, Planetary, Systemic, Sector, Galactic }
}