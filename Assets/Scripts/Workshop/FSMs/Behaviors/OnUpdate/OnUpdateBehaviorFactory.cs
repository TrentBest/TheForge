using TheSingularityWorkshop.FsmApi.Contexts;
using TheSingularityWorkshop.FsmApi.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using TheSingularityWorkshop.FSM_API;

namespace TheSingularityWorkshop.FsmApi.Behaviors.OnUpdate
{
   public static class OnUpdateBehaviorFactory
    {
        public static IDigitalStateContext ConvertToDigital(IStateContext context)
        {
            return context as IDigitalStateContext;
        }
        public static Dictionary<string, Action<IStateContext>> GetDigitalStates()
        {
            return new Dictionary<string, Action<IStateContext>>
            {
                // -- Memory Operations --
                { "SET_HIGH_CHANNEL_A", ctx => ConvertToDigital(ctx).WriteFloatValue("A", 1.0f) },
                { "SET_LOW_CHANNEL_A",  ctx => ConvertToDigital(ctx).WriteFloatValue("A", 0.0f) },
                
                // -- Counter (Sequential Logic) --
                { "INCREMENT_REGISTER", ctx => {
                    float val = ConvertToDigital(ctx).ReadFloatValue("Register");
                    ConvertToDigital(ctx).WriteFloatValue("Register", val + 1.0f);
                }},

                // -- Signal Processing --
                { "LATCH_VALUE", ctx => {
                    // Holds the value from A into Register on enter or update
                    float input = ConvertToDigital(ctx).ReadFloatValue("A");
                    ConvertToDigital(ctx).WriteFloatValue("Register", input);
                }},

                // -- Temporal States --
                { "TOGGLE_CLOCK", ctx => {
                    float clock = ConvertToDigital(ctx).ReadFloatValue("Clock");
                    ConvertToDigital(ctx).WriteFloatValue("Clock", clock > 0.5f ? 0.0f : 1.0f);
                }}
            };
        }

    }
}
