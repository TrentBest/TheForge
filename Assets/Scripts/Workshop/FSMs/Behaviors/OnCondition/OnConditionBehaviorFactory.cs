using System;
using System.Collections.Generic;
using System.Text;
using TheSingularityWorkshop.FSM_API;
using UnityEngine;

namespace TheSingularityWorkshop.FsmApi.Behaviors.OnCondition
{
    public static class OnConditionBehaviorFactory
    {
        /// <summary>
        /// Returns the Atomic Digital Logic library. 
        /// These evaluate raw bitwise or boolean states from the context.
        /// </summary>
        public static Dictionary<string, Func<IStateContext, bool>> GetDigitalLogic()
        {
            return new Dictionary<string, Func<IStateContext, bool>>
            {
                // -- Basic Logic Gates --
                { "LOGIC_NOT", ctx => !ctx.ReadSignal("A") },
                { "LOGIC_AND", ctx => ctx.ReadSignal("A") && ctx.ReadSignal("B") },
                { "LOGIC_OR",  ctx => ctx.ReadSignal("A") || ctx.ReadSignal("B") },
                { "LOGIC_XOR", ctx => ctx.ReadSignal("A") ^  ctx.ReadSignal("B") },
                { "LOGIC_NAND", ctx => !(ctx.ReadSignal("A") && ctx.ReadSignal("B")) },

                // -- Bitwise Comparison (Magnitude) --
                { "GREATER_THAN", ctx => ctx.ReadValue("A") > ctx.ReadValue("B") },
                { "LESS_THAN",    ctx => ctx.ReadValue("A") < ctx.ReadValue("B") },
                { "EQUALS",       ctx => Mathf.Approximately(ctx.ReadValue("A"), ctx.ReadValue("B")) },

                // -- Temporal Logic (The "Clock") --
                { "PULSE_HIGH", ctx => ctx.ReadValue("Clock") > 0.5f },
                { "ON_RISING_EDGE", ctx => ctx.ReadSignal("A") && !ctx.ReadSignal("Prev_A") },

                // -- State Memory --
                { "BIT_IS_SET", ctx => ((int)ctx.ReadValue("Register") & (1 << (int)ctx.ReadValue("BitIndex"))) != 0 }
            };
        }

        // Extension methods to interface with your Context's memory system
        private static bool ReadSignal(this IStateContext ctx, string channel)
            => ctx.ReadValue(channel) > 0.5f;

        private static float ReadValue(this IStateContext ctx, string channel)
        {
            // This links directly to your FSM_Memory or DataWarehouse system
            // For now, returning 0 as a placeholder
            return 0f;
        }
    }
}
