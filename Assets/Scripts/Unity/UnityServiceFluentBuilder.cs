#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Assets.Scripts.Unity
{
    /// <summary>
    /// Base fluent builder for Unity Services that recursively provides its own GUI.
    /// </summary>
    public abstract class UnityServiceFluentBuilder<TBuilder, TConfig>
        where TBuilder : UnityServiceFluentBuilder<TBuilder, TConfig>
        where TConfig : ScriptableObject
    {
        protected TConfig _config;

        public UnityServiceFluentBuilder(TConfig config)
        {
            _config = config;
        }

        /// <summary>
        /// Returns the GuiBuilder for this specific service configuration.
        /// </summary>
        public abstract GraphicalUserInterfaceBuilder GetGuiBuilder();

        /// <summary>
        /// Terminal build method to return the configured data asset.
        /// </summary>
        public TConfig Build() => _config;

        protected TBuilder Self => (TBuilder)this;
    }
}
#endif