#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using TheSingularityWorkshop.Builders;

namespace TheSingularityWorkshop.Editors
{
    public class DroneEditorWindow : EditorWindow
    {
        [MenuItem("TheSingularityWorkshop/Drones/Drone Editor Window")]
        public static void ShowWindow()
        {
            var wnd = GetWindow<DroneEditorWindow>();
            wnd.titleContent = new GUIContent("Drone Editor");
            wnd.minSize = new Vector2(550, 650);
        }

        private void OnEnable()
        {
            rootVisualElement.Clear();

            // Instantiate Builder
            var builder = new DroneBuilder()
                .WithName("Scout_MK1")
                .WithBehavior("StandardDrone");

            // FIX: Removed 'new GuiContext()' - Build() takes 0 arguments
            var panel = builder.GetGuiBuilder().Build();
            rootVisualElement.Add(panel);
        }
    }
}
#endif