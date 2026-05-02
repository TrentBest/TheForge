using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.UI_And_Tools.Forge.Core;

namespace Workshop.UI_And_Tools.Forge.Hermit.UI
{
    public static class HermitForgeExtensions
    {
        /// <summary>
        /// Injects the Hermit AI Presence into the GUI being built.
        /// This adds a floating chat portal to the root of the interface.
        /// </summary>
        public static GraphicalUserInterfaceBuilder WithHermit(this GraphicalUserInterfaceBuilder builder, HermitSettings settings)
        {
            builder.OnBuild(root =>
            {
                // Correctly calls the new HermitSettings-based constructor
                var portal = new ForgeHermitChatPortalBuilder(settings).Build(new GuiContext());
                root.Add(portal);
            });

            return builder;
        }
    }
}
