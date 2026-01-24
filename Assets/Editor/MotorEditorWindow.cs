#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Assets.Scripts.Builders;

namespace Assets.Scripts.Editors
{
    public class MotorEditorWindow : EditorWindow
    {
        [MenuItem("Drones/Motor Editor Window")]
        public static void ShowWindow()
        {
            var wnd = GetWindow<MotorEditorWindow>();
            wnd.titleContent = new GUIContent("Motor Editor");
            wnd.minSize = new Vector2(300, 200);
        }

        private void OnEnable()
        {
            rootVisualElement.Clear();

            var builder = new MotorBuilder("Prototype_Thruster", 250f);

            // FIX: Removed 'new GuiContext()'
            var panel = builder.GetGuiBuilder().Build();
            rootVisualElement.Add(panel);
        }
    }
}
#endif