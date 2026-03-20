using System;
using System.Collections.Generic;
using System.Diagnostics;
using TheSingularityWorkshop.FSM_API;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace TheSingularityWorkshop.Forge.Builders.GuiBuilders
{
    /// <summary>
    /// A Decorator/Proxy that wraps FSM delegates to guarantee logging 
    /// without altering the core FSM API or the target methods.
    /// </summary>
    public interface IFsmProxy
    {
        Action<IStateContext> Wrap(string stateName, string methodName, Action<IStateContext> originalMethod);
    }



    /// <summary>
    /// Base class that handles the actual delegate wrapping and execution order.
    /// Derived classes simply override the hooks they care about.
    /// </summary>
    public abstract class AbstractFsmProxy : IFsmProxy
    {
        public Action<IStateContext> Wrap(string stateName, string methodName, Action<IStateContext> originalMethod)
        {
            if (originalMethod == null) return null;

            return (ctx) =>
            {
                try
                {
                    OnBeforeExecute(stateName, methodName, ctx);

                    originalMethod(ctx); // The actual FSM logic

                    OnAfterExecute(stateName, methodName, ctx);
                }
                catch (Exception ex)
                {
                    OnException(stateName, methodName, ctx, ex);
                }
            };
        }

        // --- TEMPLATE HOOKS ---
        protected virtual void OnBeforeExecute(string stateName, string methodName, IStateContext ctx) { }
        protected virtual void OnAfterExecute(string stateName, string methodName, IStateContext ctx) { }
        protected virtual void OnException(string stateName, string methodName, IStateContext ctx, Exception ex) { }
    }

    public class FsmDebugProxy : AbstractFsmProxy
    {
        protected override void OnBeforeExecute(string stateName, string methodName, IStateContext ctx)
        {
            Debug.Log($"<color=orange>[FSM PROXY]</color> --> ATTEMPT: {stateName}.{methodName} | Context: {ctx.Name}");
        }

        protected override void OnException(string stateName, string methodName, IStateContext ctx, Exception ex)
        {
            Debug.LogError($"<color=red>[FSM PROXY]</color> !!! EXCEPTION in {stateName}.{methodName}: {ex.Message}");
        }

        public FsmDebugProxy()
        {

        }
    }

    public class FsmPerformanceProxy : AbstractFsmProxy
    {
        private Stopwatch _stopwatch = new Stopwatch();

        protected override void OnBeforeExecute(string stateName, string methodName, IStateContext ctx)
        {
            _stopwatch.Restart();
        }

        protected override void OnAfterExecute(string stateName, string methodName, IStateContext ctx)
        {
            _stopwatch.Stop();
            if (_stopwatch.ElapsedMilliseconds > 16) // If a single state takes longer than 1 frame (16ms)
            {
                UnityEngine.Debug.LogWarning($"[PERF] {stateName}.{methodName} is causing lag! Took {_stopwatch.ElapsedMilliseconds}ms.");
            }
        }
    }

    public static class FsmDebug
    {
        private static Dictionary<string, Action<IStateContext>> _originalOnEnters = new Dictionary<string, Action<IStateContext>>();
        private static IFsmProxy _currentShoe = new FsmDebugProxy();

        public static void ActivateLogging()
        {
            var processGroups = FSM_API.FSM_API.Internal.GetProcessingGroups();
            foreach (var group in processGroups)
            {
                foreach (var fsmName in FSM_API.FSM_API.Internal.GetFsmDefinitionNamesInGroup(group))
                {
                    var def = FSM_API.FSM_API.Internal.GetFsmDefinition(fsmName, group);

                    foreach (var state in def.GetAllStates())
                    {
                        string memoryKey = $"{group}_{fsmName}_{state.Name}";

                        // 1. Cache the original DNA if we haven't already
                        //Action<IStateContext> originalOnEnter = state.GetOnEnter();
                        //if (!_originalOnEnters.ContainsKey(memoryKey))
                        //{
                        //    _originalOnEnters[memoryKey] = originalOnEnter;
                        //}

                        //// 2. Wrap it in the Proxy Shoe
                        //Action<IStateContext> proxiedOnEnter = _currentShoe.Wrap(state.Name, "OnEnter", originalOnEnter);

                        //// 3. Inject the modified DNA
                        //FSM_API.FSM_API.Interaction.StateModifier.SetOnEnter(def, state.Name, proxiedOnEnter);
                    }
                }
            }
        }

        public static Action<IStateContext> OnEnterShunt { get; set; } = DefaultDebugOnEnterShunt;
        public static Action<IStateContext> OnUpdateShunt { get; set; } = DefaultDebugOnUpdateShunt;
        public static Action<IStateContext> OnExitShunt { get; set; } = DefaultDebugOnExitShunt;

        private static void DefaultDebugOnExitShunt(IStateContext context)
        {
            
        }

        private static void DefaultDebugOnUpdateShunt(IStateContext context)
        {
            
        }

        private static void DefaultDebugOnEnterShunt(IStateContext context)
        {
            
        }

        public static void DeactivateLogging()
        {

        }
    }
}