using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using Assets.Scripts.raWWar;

namespace Assets.Scripts.raWWar.Editors
{
    public class raWWar_SoldierForgeConfiguration
    {
        // This isn't an IGuiProvider itself; it's a factory that wires together generic Forge components
        //public IGuiProvider GetSoldierEditor(SoldierDataModel currentSoldier)
        //{
        //    // 1. Setup the generic CRUD builder
        //    var crudBuilder = new CRUD_Builder<SoldierDataModel>(
        //        onSave: (data) => SaveToDatabase(data),
        //        onDelete: (data) => DeleteFromDatabase(data)
        //    );

        //    // 2. Define the left pane using your GraphicalUserInterfaceBuilder and ControlFactories
        //    crudBuilder.SetEditorPane(ctx => {
        //        return new GraphicalUserInterfaceBuilder("SoldierStats")
        //            .AddStringData("Designation", currentSoldier.Name, v => currentSoldier.Name = v)
        //            .AddSliderData("Base Health", 10, 500, currentSoldier.Health, v => currentSoldier.Health = v)
        //            // ...
        //            .Build();
        //    });

        //    // 3. Define the right pane by injecting the generic InstancedMeshPreviewBuilder
        //    crudBuilder.SetPreviewPane(ctx => {
        //        ComputeBuffer formationBuffer = GenerateFormationBuffer(currentSoldier);

        //        return new InstancedMeshPreviewBuilder(
        //            mesh: currentSoldier.BaseMesh,
        //            material: currentSoldier.FactionMaterial,
        //            count: 8000,
        //            dataBuffer: formationBuffer,
        //            bufferNameInShader: "_SoldierBuffer"
        //        ).CreateGui(ctx);
        //    });

        //    return crudBuilder;
        //}
    }
}