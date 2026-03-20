using TheSingularityWorkshop.MicroPackages;

namespace TheSingularityWorkshop.Armada2525.GURPS
{
    public interface IGurpsApiProvider : IProvider
    {
        string ModuleName { get; }

        /// <summary>Called when the engine boots. Should trigger LoadFromCache.</summary>
        void Initialize();

        /// <summary>Forces the module to serialize its current state to disk.</summary>
        void SaveToCache();

        /// <summary>Reads state from disk.</summary>
        void LoadFromCache();
    }
}