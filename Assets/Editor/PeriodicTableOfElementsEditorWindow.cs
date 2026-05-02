using UnityEditor;
using UnityEngine;
using Workshop;

namespace Assets.Editor
{
    class PeriodicTableOfElementsEditorWindow : EditorWindow
    {
        [MenuItem("TheSingularityWorkshop/Builders/Periodic Table of Elements Editor")]
        public static void ShowWindow()
        {
            var wnd = GetWindow<PeriodicTableOfElementsEditorWindow>();
            wnd.titleContent = new GUIContent("PeriodicTableOfElementsEditor");
            wnd.minSize = new Vector2(1200, 800);
        }

        private void OnEnable()
        {
            Refresh();
        }

        private void Refresh()
        {
            rootVisualElement.Clear();

            // Example atom builder (you can wire defaults as you prefer)
            var builder = new PeriodicTableOfElementsBuilder().WithKnownElements();

            var panel = builder.GetGuiBuilder().WithEditorMode(false).Build();
            panel.style.flexGrow = 1;
            rootVisualElement.Add(panel);
        }
    }
}
