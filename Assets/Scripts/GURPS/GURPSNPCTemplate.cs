using System;
using UnityEngine;

namespace Workshop.GURPS
{
    [Serializable]
    public class GURPSNPCTemplate
    {
        public string Name = "Generic Citizen";
        public string Description = "A standard background NPC.";

        [Header("Attributes")]
        public int ST = 10;
        public int DX = 10;
        public int IQ = 10;
        public int HT = 10;

        public int BasePoints = 25;
        public string PrimaryOccupation = "Laborer";

        public GURPSNPCTemplate() { }

        public GURPSNPCTemplate(string name, int points)
        {
            Name = name;
            BasePoints = points;
        }
    }
}