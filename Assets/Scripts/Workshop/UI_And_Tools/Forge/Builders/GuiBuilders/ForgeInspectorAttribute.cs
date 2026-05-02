using System;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders
{
    /// <summary>
    /// Decorate any IGuiProvider with this attribute to automatically register it 
    /// as the global UI handler for a specific Data Type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class ForgeInspectorAttribute : Attribute
    {
        public Type TargetType { get; }

        public ForgeInspectorAttribute(Type targetType)
        {
            TargetType = targetType;
        }
    }
}