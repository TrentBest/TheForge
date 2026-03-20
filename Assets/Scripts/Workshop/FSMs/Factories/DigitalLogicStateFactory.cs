using TheSingularityWorkshop.FsmApi.Contexts; // For IDigitalStateContext
using TheSingularityWorkshop.FsmApi.Interfaces;
using TheSingularityWorkshop.MicroPackages;
using System;
using System.Collections.Generic;
using TheSingularityWorkshop.FSM_API;

namespace TheSingularityWorkshop.FsmApi.Factories
{
    public static class DigitalLogicStateFactory
    {
        /// <summary>
        /// Returns a collection of StateMethods representing sequential digital logic behaviors.
        /// Each method expects an IStateContext that can be cast to IDigitalStateContext.
        /// </summary>
        public static IEnumerable<StateMethod> GetDigitalStates()
        {
            return new List<StateMethod>
            {
                // -- Basic Memory & Signal Control --
                new StateMethod("SET_HIGH_A", "Sets Channel A to 1.0", ctx => (ctx as IDigitalStateContext)?.WriteSignal("A", true)),
                new StateMethod("SET_LOW_A", "Sets Channel A to 0.0", ctx => (ctx as IDigitalStateContext)?.WriteSignal("A", false)),
                
                // -- Data Buffering (Latch Logic) --
                new StateMethod("LATCH_A_TO_REG", "Copies value from Channel A into the Register", ctx => {
                    var dCtx = ctx as IDigitalStateContext;
                    if (dCtx != null) dCtx.WriteFloatValue("Register", dCtx.ReadFloatValue("A"));
                }),

                // -- Sequential Arithmetic (Accumulators) --
                new StateMethod("INCREMENT_REG", "Adds 1.0 to the current Register value", ctx => {
                    var dCtx = ctx as IDigitalStateContext;
                    if (dCtx != null) dCtx.WriteFloatValue("Register", dCtx.ReadFloatValue("Register") + 1.0f);
                }),
                new StateMethod("DECREMENT_REG", "Subtracts 1.0 from the current Register value", ctx => {
                    var dCtx = ctx as IDigitalStateContext;
                    if (dCtx != null) dCtx.WriteFloatValue("Register", dCtx.ReadFloatValue("Register") - 1.0f);
                }),

                // -- Temporal Logic (Signal Generators) --
                new StateMethod("TOGGLE_CLOCK", "Inverts the current state of the Clock signal", ctx => {
                    var dCtx = ctx as IDigitalStateContext;
                    if (dCtx != null) dCtx.WriteSignal("Clock", !dCtx.ReadSignal("Clock"));
                }),

                // -- Bit Manipulation --
                new StateMethod("SET_BIT_0", "Sets the first bit of the Register (Digital Universe bitwise logic)", ctx => {
                    var dCtx = ctx as IDigitalStateContext;
                    if (dCtx != null) {
                        int val = (int)dCtx.ReadFloatValue("Register");
                        dCtx.WriteFloatValue("Register", val | 1);
                    }
                })
            };
        }


    }
}