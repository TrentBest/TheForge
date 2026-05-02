using Assets.Scripts.Construction;
using Assets.Scripts.GURPS;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Workshop.GURPS;

namespace Workshop.GURPS.Construction
{
    /// <summary>
    /// The Authoring Engine for Architecture. 
    /// Converts geometric definitions (walls, floors) into GURPS-statted Building Blueprints.
    /// </summary>
    public class BuildingBuilder
    {
        private string buildingName = "Unknown Building";
        private List<string> levels = new List<string>();
        private List<WallBuilder> walls = new List<WallBuilder>();
        private List<FloorBuilder> floors = new List<FloorBuilder>();
        private List<RoofBuilder> roofs = new List<RoofBuilder>();

        private BuildingType buildingType = BuildingType.Residential;
        private GURPSTechDefinition techLevel;

        // --- GURPS SYSTEMIC DATA ---
        public int TotalDR { get; private set; }
        public long TotalHP { get; private set; }
        public float EstimatedCost { get; private set; }

        public BuildingBuilder(string name)
        {
            this.buildingName = name;
        }

        public BuildingBuilder WithTechLevel(int tl)
        {
            // Pulls the official tech definition from the Global Engine
            this.techLevel = DigitalGenericUniversalRolePlayingSystem.TechLevels.GetByTL(tl);
            return this;
        }

        public BuildingBuilder WithLevels(List<string> levelNames)
        {
            this.levels = levelNames ?? new List<string> { "Ground Floor" };
            return this;
        }

        public BuildingBuilder WithWalls(List<WallBuilder> walls)
        {
            this.walls = walls;
            CalculateStructuralStats();
            return this;
        }

        /// <summary>
        /// Translates the construction components into GURPS 3e Structural Stats.
        /// </summary>
        private void CalculateStructuralStats()
        {
            // Logic: DR is based on the average material thickness of walls * Tech Level multiplier
            // HP is based on the total mass/volume of the structure
            int baseDR = walls.Sum(w => w.CalculateBaseDR());
            int tlMod = techLevel?.TL ?? 3;

            this.TotalDR = baseDR + (tlMod * 2);
            this.TotalHP = (walls.Count + floors.Count) * 100;
            this.EstimatedCost = (TotalHP * 50) * (tlMod * 0.5f);
        }

        /// <summary>
        /// Registers this finished building into the ArchitectureAPI for use in the Campaign.
        /// </summary>
        public void FinalizeAndRegister()
        {
            var template = new GURPSBuildingTemplate
            {
                Name = this.buildingName,
                TechLevel = this.techLevel?.TL ?? 3,
                DR = this.TotalDR,
                HP = (int)this.TotalHP,
                Purpose = this.buildingType.ToString()
            };

            DigitalGenericUniversalRolePlayingSystem.Architecture.Register(template);
            Debug.Log($"[DURPS] Registered {buildingName} (DR:{TotalDR} HP:{TotalHP}) to the Architecture DB.");
        }
    }


}