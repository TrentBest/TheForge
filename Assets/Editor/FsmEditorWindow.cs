// Simple wrapper to test the GUI
using UnityEditor;

public class FsmEditorWindow : EditorWindow
{
    [MenuItem("Tools/TheSingularityWorkshop/FSM Editor")]
    public static void ShowWindow() => GetWindow<FsmEditorWindow>("FSM Editor");

    private void CreateGUI()
    {
        var editor = new Assets.Scripts.FsmBuilderGui();
        var context = new Assets.Scripts.GuiContext { EditMode = true };
        rootVisualElement.Add(editor.CreateGui(context));
    }
}