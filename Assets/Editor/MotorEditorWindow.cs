#if UNITY_EDITOR
using Assets.Scripts.Builders;
using UnityEditor;
using UnityEngine;

namespace TheSingularityWorkshop.Editors
{
    public class MotorEditorWindow : EditorWindow
    {
        [MenuItem("TheSingularityWorkshop/Drones/Motor Editor Window")]
        public static void ShowWindow()
        {
            var wnd = GetWindow<MotorEditorWindow>();
            wnd.titleContent = new GUIContent("Motor Editor");
            wnd.minSize = new Vector2(300, 200);
        }

        private void OnEnable()
        {
            //rootVisualElement.Clear();

            //var builder = new MotorBuilder("Prototype_Thruster", 250f);

            //// FIX: Removed 'new GuiContext()'
            //var panel = builder.GetGuiBuilder().Build();
            //rootVisualElement.Add(panel);
        }
    }
}
#endif