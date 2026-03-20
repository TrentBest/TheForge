using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using TheSingularityWorkshop.Builders.GuiBuilders;

namespace TheSingularityWorkshop.Forge.Builders.GuiBuilders
{
    /// <summary>
    /// A builder that uses reflection to automatically generate a CRUD-style 
    /// interface for modifying the properties and fields of a target object.
    /// </summary>
    public class ReflectiveGuiBuilder<T> : IGuiProvider where T : class
    {
        public string Title { get; set; }
        private readonly T _target;
        private readonly EditorControlFactory _factory;
        private GuiContext _lastCtx;

        public ReflectiveGuiBuilder(T target, string title = null)
        {
            _target = target;
            _factory = new EditorControlFactory();
            Title = title ?? $"Edit {typeof(T).Name}";
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            _lastCtx = ctx;

            var rootBuilder = new GraphicalUserInterfaceBuilder($"{typeof(T).Name}_ReflectiveEditor")
                .WithPadding(10)
                .WithAutoGrow()
                .AddChild(new Label(Title)
                {
                    style = {
                        fontSize = 20,
                        unityFontStyleAndWeight = FontStyle.Bold,
                        marginBottom = 10,
                        color = Color.white
                    }
                })
                .AddSeparator(Color.gray, 1);

            // Reflect over Properties
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var prop in properties)
            {
                if (!prop.CanWrite || !prop.CanRead) continue;
                var control = CreateControlForMember(prop.PropertyType, prop.Name,
                    () => prop.GetValue(_target),
                    val => prop.SetValue(_target, val));

                if (control != null) rootBuilder.AddChild(control);
            }

            // Reflect over Fields
            var fields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (var field in fields)
            {
                var control = CreateControlForMember(field.FieldType, field.Name,
                    () => field.GetValue(_target),
                    val => field.SetValue(_target, val));

                if (control != null) rootBuilder.AddChild(control);
            }

            return rootBuilder.Build();
        }

        private VisualElement CreateControlForMember(Type type, string name, Func<object> getter, Action<object> setter)
        {
            // Parametric mapping using the EditorControlFactory
            if (type == typeof(string))
            {
                return _factory.CreateTextField(name, (string)getter(), val => setter(val));
            }
            if (type == typeof(bool))
            {
                return _factory.CreateToggle(name, (bool)getter(), val => setter(val));
            }
            if (type == typeof(float))
            {
                // Defaulting to a text field for float if no range attribute is found, 
                // or you could expand this to use Sliders if metadata is provided.
                return _factory.CreateTextField(name, getter().ToString(), val => {
                    if (float.TryParse(val, out float result)) setter(result);
                });
            }
            if (type == typeof(int))
            {
                return _factory.CreateTextField(name, getter().ToString(), val => {
                    if (int.TryParse(val, out int result)) setter(result);
                });
            }
            if (type == typeof(Color))
            {
                return _factory.CreateColorField(name, (Color)getter(), val => setter(val));
            }
            if (type.IsEnum)
            {
                // Generic invocation for Enum fields
                var method = typeof(EditorControlFactory).GetMethod(nameof(EditorControlFactory.CreateEnumField));
                var generic = method.MakeGenericMethod(type);
                return (VisualElement)generic.Invoke(_factory, new object[] { name, getter(), setter });
            }
            if (typeof(UnityEngine.Object).IsAssignableFrom(type))
            {
                return _factory.CreateObjectField(name, type, (UnityEngine.Object)getter(), val => setter(val));
            }

            return new Label($"{name} (Unsupported Type: {type.Name})") { style = { color = Color.red } };
        }

        // IGuiProvider Implementation
        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));

#if UNITY_EDITOR
        public void ToUIDocument(string assetPath) =>
            GraphicalUserInterfaceBuilder.ConvertToUIDocument(CreateGui(new GuiContext()), assetPath);
#endif

        public void FromUIDocument(string assetPath) =>
            _lastCtx?.OnBuilt?.Invoke(GraphicalUserInterfaceBuilder.ConvertFromUIDocument(assetPath));
    }
}