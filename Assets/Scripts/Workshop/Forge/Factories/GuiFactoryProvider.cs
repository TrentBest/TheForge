using TheSingularityWorkshop.Forge.Builders.GuiBuilders;

namespace TheSingularityWorkshop.Builders.GuiBuilders
{
    public static class GuiFactoryProvider
    {
        private static IControlFactory _overrideFactory;

        // The Factory of Factories: Determines the correct atomic factory at runtime
        public static IControlFactory GetFactory()
        {
            if (_overrideFactory != null)
                return _overrideFactory;

#if UNITY_EDITOR
            return new EditorControlFactory();
#else
            return new RuntimeControlFactory();
#endif
        }

        // Useful for unit testing or forcing a specific UI paradigm
        public static void SetOverrideFactory(IControlFactory factory)
        {
            _overrideFactory = factory;
        }
    }
}