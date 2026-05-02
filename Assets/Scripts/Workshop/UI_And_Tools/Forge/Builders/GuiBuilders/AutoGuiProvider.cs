using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders
{
    /// <summary>
    /// A drop-in IGuiProvider that automatically resolves the correct UI for the object passed into it.
    /// </summary>
    public class AutoGuiProvider : IGuiProvider
    {
        private IGuiProvider _resolvedProvider;

        public string Title => _resolvedProvider?.Title ?? "AUTO ROUTER";

        public AutoGuiProvider(object targetObject, string optionalTitleOverride = null)
        {
            // The magic happens here: We ask the factory to find or build the right provider
            _resolvedProvider = TypeGuiProviderFactory.GetProviderFor(targetObject, optionalTitleOverride);
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            // LINT FIX: Graceful fallback if the factory fails to resolve a provider for the object.
            if (_resolvedProvider == null)
            {
                Debug.LogWarning($"[AutoGuiProvider] Failed to resolve IGuiProvider for target object.");

                return new ForgeContainerBuilder("AutoRouter_ResolutionFailure")
                    .WithPadding(15)
                    .WithBackgroundColor(new Color(0.1f, 0.0f, 0.0f))
                    .WithBorderColor(Color.red)
                    .WithBorderWidth(1)
                    .AddChild(new ForgeLabelBuilder("RESOLUTION FAILURE")
                        .WithBold()
                        .WithColor(Color.red)
                        .WithMarginBottom(5))
                    .AddChild(new ForgeLabelBuilder("The TypeGuiProviderFactory could not resolve a valid provider for the injected data object.")
                        .WithWordWrap()
                        .WithFontSize(10)
                        .WithColor(Color.gray))
                    .CreateGui(ctx);
            }

            return _resolvedProvider.CreateGui(ctx);
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));

        public void FromUIDocument(string doc) => _resolvedProvider?.FromUIDocument(doc);

#if UNITY_EDITOR
        public void ToUIDocument(string id) => _resolvedProvider?.ToUIDocument(id);
#else
        public void ToUIDocument(string id) { }
#endif
    }
}