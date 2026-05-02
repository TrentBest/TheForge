// File: Assets/Scripts/Workshop/Forge/Builders/GuiBuilders/Math/MathEquationDef.cs
using System;
using System.Collections.Generic;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.Math
{
    public enum MathOperation
    {
        Input_Variable,
        Constant,
        Add,
        Subtract,
        Multiply,
        Divide,
        Power,
        Dot_Product,
        Sigmoid,
        ReLU,
        Tanh
    }

    [Serializable]
    public class MathNode
    {
        public string Id = Guid.NewGuid().ToString().Substring(0, 6);
        public string NodeName = "New Op";
        public MathOperation Operation = MathOperation.Constant;

        public float ConstantValue = 0f;
        public string InputA_RefId = ""; // Points to the ID of the node supplying the A value
        public string InputB_RefId = ""; // Points to the ID of the node supplying the B value
    }

    [Serializable]
    public class MathEquationDef
    {
        public string Id = Guid.NewGuid().ToString();
        public string EquationName = "New Neural Strategy";
        public string Description = "e.g., Feedforward hidden layer activation";

        public List<MathNode> Nodes = new List<MathNode>();
        public string FinalOutputNodeId = ""; // The node that represents the final answer
    }
}