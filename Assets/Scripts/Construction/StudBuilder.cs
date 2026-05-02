using TheSingularityWorkshop;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Construction
{
    public class StudBuilder
    {
        //A stud's dimensions are based upon how it would stabilize, i.e. longest dimension along the ground plane
        private float width = 1.5f;   // Inches in spec, but your project uses feet? 
        private float depth = 3.5f;   // Standard 2x4 is 1.5" x 3.5"
        private float length = 8.0f;  // Standard 8ft
        private string species = "Douglas Fir";
        private string grade = "No. 2";
        private List<DrillData> drillHoles = new List<DrillData>(); // CNC ready: distance from bottom
        BuildingMaterial studMaterial;
        public string DisplayName => $"{length}ft {species} {grade} Stud";
        public int Dimensionality => 2;

        public struct DrillData
        {
            public float distanceFromBottom;
            public float holeDiameter;
        }

        public StudBuilder(BuildingMaterial studMaterial, float width, float length, float depth)
        {
            this.width = width;
            this.length = length;
            this.depth = depth;
            this.studMaterial = studMaterial;
        }

        public StudBuilder(BuildingMaterial studMaterial)
        {
            this.studMaterial = studMaterial;
            this.length = 3.5f; //Default length in feet
            this.width = 1.5f;  //Default width in feet
            this.depth = 0.5f;  //Default depth in feet
        }

        public StudBuilder()
        {
            this.length = 3.5f; //Default length in feet
            this.width = 1.5f;  //Default width in feet
            this.depth = 0.5f;  //Default depth in feet
            this.studMaterial = new BuildingMaterial(); //Default material
        }

        public StudBuilder WithDrillHole(float distanceFromBottom, float holeDiameter)
        {
            //Implementation to add a drill hole
            drillHoles.Add(new DrillData() { distanceFromBottom = distanceFromBottom, holeDiameter = holeDiameter });
            return this;
        }

        public StudBuilder WithMaterial(BuildingMaterial studMaterial)
        {
            this.studMaterial = studMaterial;
            return this;
        }

        public StudBuilder WithLength(float length)
        {
            this.length = length;
            return this;
        }

        public StudBuilder WithDepth(float depth)
        {
            this.depth = depth;
            return this;
        }
        public StudBuilder WithWidth(float width)
        {
            this.width = width;
            return this;
        }



        public StudContext BuildStud()
        {
            //Implementation to build and return a StudContext
            return new GameObject($"Stud").AddComponent<StudContext>();
        }
    }
}