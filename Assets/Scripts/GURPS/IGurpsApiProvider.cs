using System;
using Workshop.Core.Memory;
using Workshop.GURPS;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

public interface IGurpsApiProvider : IGuiProvider
{
    string ModuleName { get; }
    Guid Id { get; set; }

    
    void Bind(DataWarehouse warehouse);

    void Initialize();
    void SaveToCache();
    void LoadFromCache();
}