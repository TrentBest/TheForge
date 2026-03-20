using System;
using TheSingularityWorkshop.Builders.GuiBuilders;
using UnityEngine.UIElements;
// Assuming your FSM API namespace here:
// using TheSingularityWorkshop.FSMs; 

namespace TheSingularityWorkshop.Forge.Builders.GuiBuilders
{
    public class FsmInteractiveButtonBuilder
    {
        private readonly IControlFactory _factory;
        // IFiniteStateMachine _fsm; (Your FSM implementation)

        public FsmInteractiveButtonBuilder()
        {
            _factory = GuiFactoryProvider.GetFactory();

            // Setup internal FSM for this UI component
            // _fsm = new StateMachine();
            // _fsm.AddState("Idle", onEnter: SetIdleVisuals);
            // _fsm.AddState("Hover", onEnter: SetHoverVisuals);
            // _fsm.AddState("Pressed", onEnter: SetPressedVisuals);
            // _fsm.AddState("Disabled", onEnter: SetDisabledVisuals);
        }

        public VisualElement Build()
        {
            // Use the factory to create the base element
            var container = _factory.CreateLabel("Stateful Button");

            // Bind UI Toolkit events to FSM transitions
            container.RegisterCallback<MouseEnterEvent>(e => /* _fsm.Fire("PointerEntered") */ { });
            container.RegisterCallback<MouseLeaveEvent>(e => /* _fsm.Fire("PointerExited") */ { });
            container.RegisterCallback<MouseDownEvent>(e => /* _fsm.Fire("PointerDown") */ { });
            container.RegisterCallback<MouseUpEvent>(e => /* _fsm.Fire("PointerUp") */ { });

            return container;
        }
    }
}