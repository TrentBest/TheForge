#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.Builders.GuiBuilders
{
    public class StateMethodsCrudTabBuilder : IHubTabBuilder
    {
        public string TabName => "Method Registry";
        public string TabIcon => "ƒ"; // Mathematical function symbol

        public VisualElement CreateGui(GuiContext ctx)
        {
            // Ensure data is loaded
            FSM_DefinitionsLibrary.LoadRegistry();

            var root = new GraphicalUserInterfaceBuilder("MethodsRoot")
                .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Stretch)
                .WithAutoGrow()
                .WithPadding(5);

            // Column 1: OnEnter
            root.AddChild(BuildMethodColumn(
                "On Enter (Initialization)",
                new Color(0.2f, 0.4f, 0.2f), // Greenish header
                FSM_DefinitionsLibrary.GetEnterMethods,
                FSM_DefinitionsLibrary.AddEnterMethod,
                FSM_DefinitionsLibrary.RemoveEnterMethod,
                ctx
            ));

            // Column 2: OnUpdate
            root.AddChild(BuildMethodColumn(
                "On Update (Execution)",
                new Color(0.2f, 0.2f, 0.5f), // Blueish header
                FSM_DefinitionsLibrary.GetUpdateMethods,
                FSM_DefinitionsLibrary.AddUpdateMethod,
                FSM_DefinitionsLibrary.RemoveUpdateMethod,
                ctx
            ));

            // Column 3: OnExit
            root.AddChild(BuildMethodColumn(
                "On Exit (Cleanup)",
                new Color(0.5f, 0.2f, 0.2f), // Reddish header
                FSM_DefinitionsLibrary.GetExitMethods,
                FSM_DefinitionsLibrary.AddExitMethod,
                FSM_DefinitionsLibrary.RemoveExitMethod,
                ctx
            ));

            return root.Build();
        }

        // Generic Column Builder to avoid code duplication
        private IGuiProvider BuildMethodColumn(
            string title,
            Color headerColor,
            System.Func<List<string>> getList,
            System.Action<string> onAdd,
            System.Action<string> onRemove,
            GuiContext ctx)
        {
            return new GraphicalUserInterfaceBuilder(title.Replace(" ", "") + "Col")
                
                
                .WithBackgroundColor(new Color(0.18f, 0.18f, 0.18f))
                .WithBorderRadius(5)
                .WithBorderWidth(1)
                .WithBorderColor(new Color(0.1f, 0.1f, 0.1f))

                // Header
                .AddChild(new Label(title)
                {
                    style = {
                        fontSize = 14,
                        unityFontStyleAndWeight = FontStyle.Bold,
                        
                        backgroundColor = headerColor,
                        color = Color.white,
                        borderTopLeftRadius = 5,
                        borderTopRightRadius = 5
                    }
                })

                // Add New Input Field
                .AddChild(c =>
                {
                    var row = new VisualElement { style = { flexDirection = FlexDirection.Row,  borderBottomWidth = 1, borderBottomColor = Color.gray } };
                    var input = new TextField { style = { flexGrow = 1, marginRight = 5 } };
                    var addBtn = new Button(() =>
                    {
                        if (!string.IsNullOrWhiteSpace(input.value))
                        {
                            onAdd(input.value);
                            input.value = "";
                            ctx.OnBuilt?.Invoke(CreateGui(ctx)); // Trigger Refresh
                        }
                    })
                    { text = "+" };

                    row.Add(input);
                    row.Add(addBtn);
                    return row;
                })

                // Scrolling List
                .AddChild(c =>
                {
                    var listContainer = new ScrollView(ScrollViewMode.Vertical) { style = { flexGrow = 1, } };
                    var items = getList();

                    if (items.Count == 0)
                    {
                        listContainer.Add(new Label("No methods defined.") { style = { opacity = 0.5f, unityFontStyleAndWeight = FontStyle.Italic } });
                    }
                    else
                    {
                        foreach (var item in items)
                        {
                            var row = new VisualElement
                            {
                                style = {
                                    flexDirection = FlexDirection.Row,
                                    justifyContent = Justify.SpaceBetween,
                                    backgroundColor = new Color(0,0,0,0.2f),
                                    marginBottom = 2,
                                    
                                    alignItems = Align.Center
                                }
                            };

                            row.Add(new Label(item));

                            var delBtn = new Button(() =>
                            {
                                onRemove(item);
                                ctx.OnBuilt?.Invoke(CreateGui(ctx));
                            })
                            { text = "x", style = { color = new Color(1f, 0.4f, 0.4f), backgroundColor = Color.clear,  } };

                            row.Add(delBtn);
                            listContainer.Add(row);
                        }
                    }
                    return listContainer;
                });
        }
    }
}
#endif