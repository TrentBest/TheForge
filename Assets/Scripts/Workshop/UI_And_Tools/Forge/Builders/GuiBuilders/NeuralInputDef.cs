namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders
{
    public class NeuralInputDef
    {
        public string InputName { get; set; }
        public float CurrentValue { get; set; }
        public float MinValue { get; set; } = -1f;
        public float MaxValue { get; set; } = 1f;
    }
}