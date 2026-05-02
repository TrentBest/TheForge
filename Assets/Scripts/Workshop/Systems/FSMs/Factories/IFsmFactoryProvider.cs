namespace Workshop.Systems.FSMs.Factories
{
    /// <summary>
    /// Contract for micro-packages to provide their own specialized FSM and State instantiation logic.
    /// </summary>
    public interface IFsmFactoryProvider
    {
        string FactoryDomain { get; }
      
    }
}