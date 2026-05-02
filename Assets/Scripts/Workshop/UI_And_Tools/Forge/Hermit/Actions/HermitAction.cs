using System;
using System.Collections.Generic;
using System.Text;
using Workshop.UI_And_Tools.Forge.Hermit.Directives;

namespace Assets.Scripts.Workshop.UI_And_Tools.Forge.Hermit.Actions
{
    [Serializable]
    public class HermitAction
    {
        public string Type;     // Matches IHermitDirective.ActionType
        public string Target;   // The Subject of the action (e.g., "Empire_Playable_Alpha")
        public string Data;     // JSON string or parameters for the action
        public float Priority;  // 0-1 range for action scheduling

        public static void Execute(Directive_SpawnMini miniDirective)
        {
            
        }
    }
}
