using TheSingularityWorkshop.Builders.GuiBuilders;
using TheSingularityWorkshop;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using UnityEditor;



/// <summary>
/// This is old, an early version of what would become the FSM Editor inside the Singularity Workshop Hub.
/// </summary>
public class FsmEditorWindow : EditorWindow
{
   // [MenuItem("Tools/TheSingularityWorkshop/FSM Editor")]
    public static void ShowWindow() => GetWindow<FsmEditorWindow>("FSM Editor");

    private void CreateGUI()
    {
        var editor = new Workshop_Gui_FsmBuilderGui();
        var context = new GuiContext { EditMode = true };
        rootVisualElement.Add(editor.CreateGui(context));
    }
}