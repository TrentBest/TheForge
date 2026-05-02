// File: Assets/Scripts/Workshop/Forge/Builders/GuiBuilders/Math/CalculusEngine.cs
using System;
using System.Collections.Generic;
using System.Linq;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.Math
{
    public static class CalculusEngine
    {
        /// <summary>
        /// Symbolically differentiates a MathEquationDef with respect to a specific input variable node.
        /// </summary>
        public static MathEquationDef Derive(MathEquationDef originalEq, string variableNodeId)
        {
            var derivedEq = new MathEquationDef
            {
                EquationName = $"d/d({variableNodeId}) [{originalEq.EquationName}]",
                Description = "Auto-generated symbolic derivative."
            };

            // Recursive derivation starting from the final output
            string derivedOutputId = DeriveNode(originalEq.FinalOutputNodeId, originalEq.Nodes, derivedEq.Nodes, variableNodeId);
            derivedEq.FinalOutputNodeId = derivedOutputId;

            return derivedEq;
        }

        private static string DeriveNode(string nodeId, List<MathNode> origNodes, List<MathNode> derivedNodes, string varId)
        {
            var node = origNodes.FirstOrDefault(n => n.Id == nodeId);
            if (node == null) return GenerateConstant(0, derivedNodes);

            switch (node.Operation)
            {
                case MathOperation.Constant:
                    // d/dx [c] = 0
                    return GenerateConstant(0, derivedNodes);

                case MathOperation.Input_Variable:
                    // d/dx [x] = 1, d/dx [y] = 0
                    return GenerateConstant(node.Id == varId ? 1f : 0f, derivedNodes);

                case MathOperation.Add:
                    // d/dx [u + v] = u' + v'
                    return GenerateOperation(MathOperation.Add,
                        DeriveNode(node.InputA_RefId, origNodes, derivedNodes, varId),
                        DeriveNode(node.InputB_RefId, origNodes, derivedNodes, varId),
                        derivedNodes, "Sum Rule");

                case MathOperation.Multiply:
                    // Product Rule: d/dx [u * v] = (u' * v) + (u * v')
                    // We must clone the original u and v into the derived graph first
                    string u = CloneGraph(node.InputA_RefId, origNodes, derivedNodes);
                    string v = CloneGraph(node.InputB_RefId, origNodes, derivedNodes);
                    string uPrime = DeriveNode(node.InputA_RefId, origNodes, derivedNodes, varId);
                    string vPrime = DeriveNode(node.InputB_RefId, origNodes, derivedNodes, varId);

                    string leftTerm = GenerateOperation(MathOperation.Multiply, uPrime, v, derivedNodes, "Product (u'*v)");
                    string rightTerm = GenerateOperation(MathOperation.Multiply, u, vPrime, derivedNodes, "Product (u*v')");

                    return GenerateOperation(MathOperation.Add, leftTerm, rightTerm, derivedNodes, "Product Rule Sum");

                // Note: Implement Subtract, Divide (Quotient Rule), and Power (Chain Rule) similarly!
                default:
                    return GenerateConstant(0, derivedNodes);
            }
        }

        // --- Helpers to build the new Node Graph ---

        private static string GenerateConstant(float value, List<MathNode> targetList)
        {
            var cNode = new MathNode { Operation = MathOperation.Constant, ConstantValue = value, NodeName = $"Const({value})" };
            targetList.Add(cNode);
            return cNode.Id;
        }

        private static string GenerateOperation(MathOperation op, string inputA, string inputB, List<MathNode> targetList, string label)
        {
            var opNode = new MathNode { Operation = op, InputA_RefId = inputA, InputB_RefId = inputB, NodeName = label };
            targetList.Add(opNode);
            return opNode.Id;
        }

        private static string CloneGraph(string origNodeId, List<MathNode> origNodes, List<MathNode> targetList)
        {
            var orig = origNodes.FirstOrDefault(n => n.Id == origNodeId);
            if (orig == null) return null;

            var clone = new MathNode { Operation = orig.Operation, ConstantValue = orig.ConstantValue, NodeName = $"Copy_{orig.NodeName}" };
            if (!string.IsNullOrEmpty(orig.InputA_RefId)) clone.InputA_RefId = CloneGraph(orig.InputA_RefId, origNodes, targetList);
            if (!string.IsNullOrEmpty(orig.InputB_RefId)) clone.InputB_RefId = CloneGraph(orig.InputB_RefId, origNodes, targetList);

            targetList.Add(clone);
            return clone.Id;
        }
    }
}