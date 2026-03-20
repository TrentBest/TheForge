namespace TheSingularityWorkshop.Forge.Builders.GuiBuilders
{
    public interface IGuiRouter
    {
        // The Mediator command. Tell the router where to go next!
        void NavigateTo(string routeId);
    }
}