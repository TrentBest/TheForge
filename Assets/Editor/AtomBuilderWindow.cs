
#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts
{
    public class AtomBuilderWindow : EditorWindow
    {
        [MenuItem("Tools/Builders/Atom Builder")]
        public static void ShowWindow()
        {
            var wnd = GetWindow<AtomBuilderWindow>();
            wnd.titleContent = new GUIContent("Atom Builder");
            wnd.minSize = new Vector2(520, 600);
        }

        private void OnEnable()
        {
            rootVisualElement.Clear();

            // Example atom builder (you can wire defaults as you prefer)
            var builder = new AtomBuilder("Hydrogen", 1, "H")
                .WithAtomicWeight(1.008f)
                .WithNeutrons(0)
                .WithProtons(1)
                .WithElectrons(1)
                .WithElectronConfiguration(new List<string> { "1s1" });

            var panel = builder.GetGuiBuilder().Build();
            rootVisualElement.Add(panel);
        }
    }
}
#endif

