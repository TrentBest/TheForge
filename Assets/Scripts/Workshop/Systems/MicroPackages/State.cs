namespace Workshop.Systems.MicroPackages
{
    public struct State
    {
        public string Name { get; }
        public string Description { get; }
        public IStateOnEnterProvider OnEnter { get; }
        public IStateOnUpdateProvider OnOnUpdate { get; }
        public IStateOnExitProvider OnOnExit { get; }
    }
}
