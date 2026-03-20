using System;
using UnityEngine;
using UnityEngine.UIElements;
using TheSingularityWorkshop.FSM_API;

namespace TheSingularityWorkshop.Forge.Architectures
{
    public interface IComputeArchitecture
    {
        string Id { get; }
        string DisplayName { get; }
        string Description { get; }
        Color ThemeColor { get; }

        // The specific Forge GUI needed to configure this compute model
        VisualElement BuildEditorUI();

        // FUTURE: The method that actually translates the FSM and Data Context
        // void CompileArchitecture(IStateContext context, FSM_Definition fsmDef);
    }
}