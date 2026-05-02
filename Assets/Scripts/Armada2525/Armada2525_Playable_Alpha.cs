using Assets.Scripts.MastersOfOrionII;
using System;
using TheSingularityWorkshop.FSM_API;
using Workshop.Core.Math; // Injected for Deterministic Rng
using Workshop.Systems.MicroPackages;

namespace Workshop.Armada2525
{
    public class Armada2525_Playable_Alpha : IStateOnEnterProvider
    {
        public Action<IStateContext> Provided => OnEnter;
        public int Id => 252501;
        public ProviderType ProviderType => ProviderType.StateOnEnter;

        public async void OnEnter(IStateContext context)
        {
            // Pattern Match Cast for safety
            if (!(context is ArmadaGalaxyContext armadaCtx)) return;

            // Initialize the Universe struct
            armadaCtx.GalaxyConfig = new Galaxy();

            // 1. DETERMINISTIC SOVEREIGNTY
            // We use the Universal Constants seed so a player can type "42" and always get the exact same galaxy map.
            // If none exists, we hash the context name to guarantee reproducibility.
            uint gameSeed = armadaCtx.UniversalSeed > 0 ? armadaCtx.UniversalSeed : (uint)armadaCtx.Name.GetHashCode();

            await ArmadaClassicMapGenerator.GenerateClassicGalaxyAsync(armadaCtx, gameSeed);

            // 2. BOOT THE MACRO SIMULATION FSM
            // This simulation will tick on "Armada_Sim_Tick", which the ElasticScheduler handles in the background
            var simHandle = FSM_API.Create.CreateInstance(
                "ArmadaGalaxySim_Def",
                armadaCtx,
                "Armada_Sim_Tick"
            );

            simHandle.TransitionTo("SimulatingTurn");

            // 3. UI DECOUPLING
            // ANTI-PATTERN PURGED: We no longer create a "Showcase_UI_Def" FSM here.
            // We simply flag the pure data context as ready. The UI Router will read this.
            armadaCtx.IsGalaxyGenerated = true;
        }
    }
}