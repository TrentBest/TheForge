using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Assets.Editor.raWWar
{
    public class raWWarEditorWindow : EditorWindow
    {
        private VisualElement _tabContentContainer;
        private readonly List<IEditorModule> _modules = new List<IEditorModule>();
        private string _activeModuleName;

        [MenuItem("TheSingularityWorkshop/NonSupported/Command Center")]
        public static void ShowWindow()
        {
            var wnd = GetWindow<raWWarEditorWindow>();
            wnd.titleContent = new GUIContent("raWWar Command");
            wnd.minSize = new Vector2(1100, 700);
        }

        private void OnEnable()
        {
            // Register Modules here. In the future, this could be automated 
            // via reflection or a central registry.
            _modules.Add(new StatusModule());
            _modules.Add(new SoldierModule());
            // _modules.Add(new VehicleModule()); // Future addition
        }

        public void CreateGUI()
        {
            var root = new GraphicalUserInterfaceBuilder("MainShell")
                .WithEditorMode(true)
                .WithAutoGrow()
                .WithPadding(0)
                .WithBackgroundColor(new Color(0.12f, 0.12f, 0.12f));

            // Top Tab Bar
            var tabBar = new GraphicalUserInterfaceBuilder("TabBar")
                .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Center)
                .WithBackgroundColor(new Color(0.08f, 0.08f, 0.08f))
                .WithPadding(5);

            foreach (var module in _modules)
            {
                tabBar.AddChild(ctx => {
                    var btn = new Button(() => SwitchTab(module.ModuleName)) { text = module.ModuleName.ToUpper() };
                    btn.style.width = 120;
                    btn.style.height = 30;
                    return btn;
                });
            }

            root.AddChild(tabBar);

            // Content Area
            _tabContentContainer = new VisualElement { name = "TabContent" };
            _tabContentContainer.style.flexGrow = 1;
            root.AddChild(_tabContentContainer);

            rootVisualElement.Add(root.Build());

            // Default tab
            if (_modules.Count > 0) SwitchTab(_modules[0].ModuleName);
        }

        private void SwitchTab(string moduleName)
        {
            if (_activeModuleName == moduleName) return;
            _activeModuleName = moduleName;
            _tabContentContainer.Clear();

            var module = _modules.Find(m => m.ModuleName == moduleName);
            if (module != null)
            {
                _tabContentContainer.Add(module.CreateGui());
            }
        }
    }

    public interface IEditorModule
    {
        string ModuleName { get; }
        VisualElement CreateGui();
    }
}