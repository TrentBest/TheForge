using System;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders
{
    /// <summary>
    /// Decorate a specific Field or Property to override its UI generation, 
    /// regardless of its underlying Type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true)]
    public class ReflectiveGuiAttribute : Attribute
    {
        public Type ProviderType { get; }

        public ReflectiveGuiAttribute(Type providerType)
        {
            if (!typeof(IGuiProvider).IsAssignableFrom(providerType))
            {
                throw new ArgumentException($"ProviderType must implement IGuiProvider: {providerType.Name}");
            }
            ProviderType = providerType;
        }
    }
}