using System;
using System.Text;
using System.Collections.Generic;
// Assuming your FSM namespace:
// using TheSingularityWorkshop.FSMs; 

namespace Assets.Scripts.raWWar.Gui
{
    public class ShaderForgeContext // : IStateContext (Assuming your interface here)
    {
        public bool IsValid { get; set; } = true;

        // The modular parts of the shader being built
        public string ShaderName { get; set; } = "raWWar/TacticalShader";
        public List<string> Properties { get; set; } = new List<string>();
        public List<string> StructData { get; set; } = new List<string>();
        public List<string> VertexLogic { get; set; } = new List<string>();
        public List<string> FragmentLogic { get; set; } = new List<string>();

        // The final output
        public string CompiledHLSL { get; set; } = "";
    }
}