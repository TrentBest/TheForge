#if UNITY_EDITOR
using Assets.Editor.Singularity;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TheSingularityWorkshop.Builders.GuiBuilders
{
    public class Workshop_Gui_FsmStateMethodsCrudTabBuilder : IGuiProvider
    {
        public string TabName => "Method Registry";
        public string TabIcon => "ƒ";

        public string Title => TabName;

        public VisualElement CreateGui(GuiContext ctx)
        {
            FSM_DefinitionsLibrary.LoadRegistry();

            var root = new GraphicalUserInterfaceBuilder("MethodsRoot")
                .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Stretch)
                .WithPadding(5);

            // Columns use flexGrow 1 and flexBasis 0 via BuildMethodColumn internal layout
            root.AddChild(BuildMethodColumn("On Enter", new Color(0.2f, 0.4f, 0.2f),
                FSM_DefinitionsLibrary.GetEnterMethods, FSM_DefinitionsLibrary.AddEnterMethod, FSM_DefinitionsLibrary.RemoveEnterMethod, ctx));

            root.AddChild(BuildMethodColumn("On Update", new Color(0.2f, 0.2f, 0.5f),
                FSM_DefinitionsLibrary.GetUpdateMethods, FSM_DefinitionsLibrary.AddUpdateMethod, FSM_DefinitionsLibrary.RemoveUpdateMethod, ctx));

            root.AddChild(BuildMethodColumn("On Exit", new Color(0.5f, 0.2f, 0.2f),
                FSM_DefinitionsLibrary.GetExitMethods, FSM_DefinitionsLibrary.AddExitMethod, FSM_DefinitionsLibrary.RemoveExitMethod, ctx));

            return root.Build();
        }

        public void FromUIDocument(string assetPath)
        {
            throw new NotImplementedException();
        }

        public Action<VisualElement> GetGuiBuilder()
        {
            throw new NotImplementedException();
        }

        public void ToUIDocument(string assetPath)
        {
            throw new NotImplementedException();
        }

        private IGuiProvider BuildMethodColumn(string title, Color headerColor, System.Func<List<string>> getList,
            System.Action<string> onAdd, System.Action<string> onRemove, GuiContext ctx)
        {
            var colBuilder = new GraphicalUserInterfaceBuilder(title.Replace(" ", "") + "Col")
                .WithBackgroundColor(new Color(0.18f, 0.18f, 0.18f))
                .WithBorderRadius(5)
                .WithBorderWidth(1)
                .WithBorderColor(new Color(0.1f, 0.1f, 0.1f));
               

            // Force equal column width
            colBuilder.OnBuild(el => { el.style.flexGrow = 1; el.style.flexBasis = 0; });

            // 1. Header (Pinned)
            colBuilder.AddChild(new Label(title)
            {
                style = {
                    fontSize = 14, unityFontStyleAndWeight = FontStyle.Bold,
                    backgroundColor = headerColor, color = Color.white,
                    paddingLeft = 5, borderTopLeftRadius = 5, borderTopRightRadius = 5
                }
            });

            // 2. Add New Input Area (Pinned)
            colBuilder.AddChild(c => {
                var row = new VisualElement { style = { flexDirection = FlexDirection.Row, paddingBottom = 5, borderBottomWidth = 1, borderBottomColor = Color.gray } };
                var input = new TextField { value = "Behavior Name", style = { flexGrow = 1, marginRight = 5 } };

                // Select all text on focus for easy replacement
                input.RegisterCallback<FocusInEvent>(evt => input.SelectAll());

                var addBtn = new Button(() => {
                    if (!string.IsNullOrWhiteSpace(input.value) && input.value != "Behavior Name")
                    {
                        onAdd(input.value);
                        ctx.OnBuilt?.Invoke(CreateGui(ctx));
                    }
                })
                { text = "+" };

                row.Add(input);
                row.Add(addBtn);
                return row;
            });

            // 3. Scrollable List
            colBuilder.AddChild(c => {
                var scroll = new ScrollView(ScrollViewMode.Vertical) { style = { flexGrow = 1 } };
                var items = getList();

                foreach (var item in items)
                {
                    var row = new VisualElement
                    {
                        style = {
            flexDirection = FlexDirection.Row,
            backgroundColor = new Color(0, 0, 0, 0.2f),
            marginBottom = 2, paddingLeft = 4, minHeight = 25
        }
                    };

                    // 1. The Method Name Label
                    var nameLabel = new Label(item)
                    {
                        style = { flexGrow = 0, width = 120, unityTextAlign = TextAnchor.MiddleLeft, unityFontStyleAndWeight = FontStyle.Bold }
                    };
                    row.Add(nameLabel);

                    // 2. The Path/Status Label (The new part!)
                    string currentPath = FSM_DefinitionsLibrary.GetDllPath(item);
                    bool isMapped = !string.IsNullOrEmpty(currentPath);

                    var pathLabel = new Label(isMapped ? System.IO.Path.GetFileName(currentPath) : "Unmapped")
                    {
                        style = {
            flexGrow = 1,
            fontSize = 10,
            opacity = 0.6f,
            unityTextAlign = TextAnchor.MiddleLeft,
            color = isMapped ? Color.green : Color.gray
        }
                    };
                    row.Add(pathLabel);

                    // 3. DLL Bind Button
                    Button bindBtn = null;
                    bindBtn = new Button(() => {
                        string path = EditorUtility.OpenFilePanel("Select Behavior DLL", "", "dll");
                        if (!string.IsNullOrEmpty(path))
                        {
                            FSM_DefinitionsLibrary.SetDllPath(item, path);
                            bindBtn.style.backgroundColor = Color.green;
                            pathLabel.text = System.IO.Path.GetFileName(path);
                            pathLabel.style.color = Color.green;
                        }
                    })
                    {
                        text = "🔗",
                        style = {
            backgroundColor = isMapped ? Color.green : Color.red,
            marginRight = 2, width = 25
        }
                    };
                    row.Add(bindBtn);

                    // 4. Delete Button
                    row.Add(new Button(() => {
                        onRemove(item);
                        ctx.OnBuilt?.Invoke(CreateGui(ctx));
                    })
                    { text = "x", style = { color = Color.red, backgroundColor = Color.clear } });

                    scroll.Add(row);
                }
                return scroll;
            });

            return colBuilder;
        }
    }
}
#endif