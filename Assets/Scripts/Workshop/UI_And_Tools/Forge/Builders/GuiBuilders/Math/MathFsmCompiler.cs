using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TheSingularityWorkshop.FSM_API;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.Math
{
    /// <summary>
    /// Transforms a visual MathEquationDef (DAG) into an executable FSM_API Definition.
    /// </summary>
    public static class MathFsmCompiler
    {
        public static void CompileToFsm(MathEquationDef equation, string targetProcessGroup = "Neural_Math")
        {
            string fsmName = $"MATH_{equation.EquationName.Replace(" ", "_")}";

            // 1. Order of Operations (Topological Sort)
            var executionOrder = GetExecutionOrder(equation);
            if (executionOrder == null)
            {
                Debug.LogError($"[Math Compiler] Circular dependency detected in {equation.EquationName}! Cannot compile.");
                return;
            }

            // 2. Begin FSM Definition
            var fsmBuilder = FSM_API.Create.CreateFiniteStateMachine(fsmName, -1, targetProcessGroup);

            // 3. Generate States and Linear Transitions
            for (int i = 0; i < executionOrder.Count; i++)
            {
                var node = executionOrder[i];
                string stateName = $"Op_{i}_{node.Operation}_{node.Id}";

                // TODO: Here you would map specific MathOperations to your actual FSM Behavior classes.
                // e.g., mapping MathOperation.Multiply to a new MultiplyBehavior class that reads A and B from the context.
                fsmBuilder.State(stateName, null, null, null);

                // Chain to the next mathematical operation
                if (i < executionOrder.Count - 1)
                {
                    string nextStateName = $"Op_{i + 1}_{executionOrder[i + 1].Operation}_{executionOrder[i + 1].Id}";
                    // Immediate transition (frame-by-frame execution)
                    fsmBuilder.Transition(stateName, nextStateName, ctx => true);
                }
            }

            // 4. Cap off the FSM
            if (executionOrder.Count > 0)
            {
                var firstNode = executionOrder[0];
                fsmBuilder.WithInitialState($"Op_0_{firstNode.Operation}_{firstNode.Id}");

                var lastNode = executionOrder.Last();
                fsmBuilder.State("Output_Result", null, null, null);
                fsmBuilder.Transition($"Op_{executionOrder.Count - 1}_{lastNode.Operation}_{lastNode.Id}", "Output_Result", ctx => true);
            }

            fsmBuilder.BuildDefinition();
            Debug.Log($"[Math Compiler] Successfully compiled {fsmName} into FSM_API with {executionOrder.Count} sequential states.");
        }

        /// <summary>
        /// Flattens the Math Node Tree into a linear sequence where dependencies are always calculated first.
        /// </summary>
        private static List<MathNode> GetExecutionOrder(MathEquationDef eq)
        {
            var sorted = new List<MathNode>();
            var visited = new HashSet<string>();
            var inProgress = new HashSet<string>();

            // Helper for Depth-First Search
            bool Visit(MathNode node)
            {
                if (node == null) return true;
                if (visited.Contains(node.Id)) return true;
                if (inProgress.Contains(node.Id)) return false; // Circular dependency!

                inProgress.Add(node.Id);

                // Visit Dependencies (Inputs) First
                if (!string.IsNullOrEmpty(node.InputA_RefId))
                {
                    var inputA = eq.Nodes.FirstOrDefault(n => n.Id == node.InputA_RefId);
                    if (!Visit(inputA)) return false;
                }

                if (!string.IsNullOrEmpty(node.InputB_RefId))
                {
                    var inputB = eq.Nodes.FirstOrDefault(n => n.Id == node.InputB_RefId);
                    if (!Visit(inputB)) return false;
                }

                inProgress.Remove(node.Id);
                visited.Add(node.Id);
                sorted.Add(node); // Add self only after inputs are resolved
                return true;
            }

            // Ensure we traverse from the final output backwards
            var outputNode = eq.Nodes.FirstOrDefault(n => n.Id == eq.FinalOutputNodeId);
            if (outputNode != null)
            {
                if (!Visit(outputNode)) return null;
            }
            else
            {
                // Fallback: Just try to visit everything if no output is set
                foreach (var node in eq.Nodes)
                {
                    if (!Visit(node)) return null;
                }
            }

            return sorted;
        }
    }
}