using System.Collections.Generic;

namespace Workshop.Systems.MicroPackages.Packages.DesignPatterns.Factory
{
    /// <summary>
    /// Holds the working memory for the Factory Architect GUI.
    /// </summary>
    public class FactoryArchitectContext
    {
        public string ActiveFactoryName { get; set; } = "New Entity Factory";
        public List<BlueprintDef> DraftBlueprints { get; set; } = new List<BlueprintDef>();

        public class BlueprintDef
        {
            public string BlueprintId { get; set; } = "Entity_01";
            public string PrefabPath { get; set; } = "Prefabs/Default";
            public string InitialFsmState { get; set; } = "State_Idle";
        }
    }
}