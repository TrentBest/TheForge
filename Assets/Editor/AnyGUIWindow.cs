using Assets.Scripts;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine.UIElements;

namespace Assets.Editor
{
    class AnyGUIWindow : EditorWindow
    {
        private IGuiProvider guiProvider;

        public AnyGUIWindow(IGuiProvider guiProvider)
        {
            this.guiProvider = guiProvider;
        }

        [MenuItem("Window/OneGUI/New Editor Window")]
        public static void ShowWindow()
        {
            GetWindow<AnyGUIWindow>();
        }
    }
}
