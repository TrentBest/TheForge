
#if UNITY_EDITOR
using Assets.Scripts.Workshop.Core.Physics;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TheSingularityWorkshop
{
    public class AtomBuilderWindow : EditorWindow
    {
        [MenuItem("TheSingularityWorkshop/NonSupported/Tools/Builders/Atom Builder")]
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

