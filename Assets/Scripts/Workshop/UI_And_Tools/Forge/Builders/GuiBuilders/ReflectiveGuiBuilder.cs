using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders
{
    public class ReflectiveGuiBuilder<T> : IGuiProvider where T : class
    {
        public string Title { get; set; }

        private T _target;
        private string _filter;
        private readonly EditorControlFactory _factory;
        private GuiContext _lastCtx;

        // --- Recursion Safety Fuse ---
        private int _recursionDepth = 1; // Default to 1 layer deep

        // --- NEW: Scoped Puzzle Pieces ---
        // Maps a Data Type (e.g., typeof(float)) to a Provider Type (e.g., typeof(FloatInspectorProvider))
        private Dictionary<Type, Type> _scopedProviders = new Dictionary<Type, Type>();

        private string LogPrefix => $"[ReflectBuilder<{typeof(T).Name}>]";

        public ReflectiveGuiBuilder(T target = null, string title = null)
        {
            _target = target;
            _factory = new EditorControlFactory();
            Title = title;
            Debug.Log($"{LogPrefix} Constructor invoked. Target is {(_target == null ? "NULL" : "Present")}.");
        }

        public ReflectiveGuiBuilder<T> WithTarget(T target) { _target = target; return this; }
        public ReflectiveGuiBuilder<T> WithTitle(string title) { Title = title; return this; }
        public ReflectiveGuiBuilder<T> WithFilter(string filter) { _filter = filter; return this; }

        public ReflectiveGuiBuilder<T> WithRecursion(int depth)
        {
            _recursionDepth = depth;
            return this;
        }

        // --- NEW: Fluent Injection for Scoped Providers ---
        public ReflectiveGuiBuilder<T> WithProviders(Dictionary<Type, Type> providers)
        {
            if (providers != null)
            {
                _scopedProviders = providers;
            }
            return this;
        }

        public VisualElement Build() => CreateGui(new GuiContext());

        public VisualElement CreateGui(GuiContext ctx)
        {
            Debug.Log($"{LogPrefix} 🟢 CreateGui started. Current Depth Allowance: {_recursionDepth}");
            _lastCtx = ctx;
            string layoutName = Title ?? $"{typeof(T).Name}_ReflectiveEditor";

            var rootBuilder = new ForgeContainerBuilder()
                .WithDirection(FlexDirection.Column)
                .WithPadding(10)
                .WithFlexGrow(1);

            if (!string.IsNullOrEmpty(Title))
            {
                rootBuilder.AddChild(new ForgeLabelBuilder(Title)
                    .WithFontSize(18)
                    .WithBold()
                    .WithMarginBottom(10)
                    .WithColor(Color.white))
                .AddSeparator(Color.gray, 1);
            }

            if (_target == null)
            {
                Debug.LogWarning($"{LogPrefix} Target is null. Aborting reflection and returning placeholder.");
                rootBuilder.AddChild(new ForgeLabelBuilder($"[Awaiting {typeof(T).Name} Injection]")
                    .WithColor(Color.yellow)
                    .WithFontStyle(FontStyle.Italic));
                return rootBuilder.CreateGui(ctx);
            }

            var members = typeof(T).GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(m => (m is PropertyInfo || m is FieldInfo) && !m.Name.Contains("<")).ToList();

            Debug.Log($"{LogPrefix} Found {members.Count} valid members to inspect.");

            foreach (var member in members)
            {
                if (!string.IsNullOrEmpty(_filter) && !member.Name.Contains(_filter, StringComparison.OrdinalIgnoreCase))
                    continue;

                VisualElement control = null;
                Type memberType = null;
                Func<object> getter = null;
                Action<object> setter = null;
                bool isPrivate = false;

                try
                {
                    // ==========================================
                    // 1. STRICT BOUNDARY: Evaluate Access Levels
                    // ==========================================
                    if (member is PropertyInfo prop)
                    {
                        memberType = prop.PropertyType;
                        getter = () => prop.GetValue(_target);

                        // It is private/reporting if it can't write, or if the setter is non-public
                        isPrivate = !prop.CanWrite || prop.GetSetMethod() == null;
                        if (!isPrivate) setter = val => prop.SetValue(_target, val);
                    }
                    else if (member is FieldInfo field)
                    {
                        memberType = field.FieldType;
                        getter = () => field.GetValue(_target);

                        // It is private/reporting if the field is private or readonly
                        isPrivate = field.IsPrivate || field.IsInitOnly;
                        if (!isPrivate) setter = val => field.SetValue(_target, val);
                    }

                    if (memberType == null) continue; // Skip unsupported members (e.g., methods/events)

                    Debug.Log($"{LogPrefix} 🔍 Inspecting '{member.Name}' (Type: {memberType.Name} | Private: {isPrivate})");

                    // ==========================================
                    // 2. CHECK FOR FIELD ATTRIBUTE (Most Specific)
                    // ==========================================
                    var customDrawerAttr = Attribute.GetCustomAttribute(member, typeof(ReflectiveGuiAttribute)) as ReflectiveGuiAttribute;

                    if (customDrawerAttr != null)
                    {
                        Debug.Log($"{LogPrefix} 🎯 Custom Drawer Attribute detected. Routing to {customDrawerAttr.ProviderType.Name}.");
                        var customProvider = (IGuiProvider)Activator.CreateInstance(customDrawerAttr.ProviderType, new object[] { getter() });

                        control = new ForgeFoldoutBuilder($"{member.Name.ToUpper()} (INJECTED UI)", false)
                            .WithMarginBottom(5)
                            .AddChild(customProvider.CreateGui(_lastCtx))
                            .Build();
                    }
                    // ==========================================
                    // 3. CHECK SCOPED PROVIDERS (Dictionary)
                    // ==========================================
                    else if (_scopedProviders.TryGetValue(memberType, out Type customProviderType))
                    {
                        Debug.Log($"{LogPrefix} 🧩 Scoped Provider mapping found. Routing '{member.Name}' to {customProviderType.Name}.");

                        // Note: Instantiating with Name, Getter, and Setter (which is null if private)
                        var customProvider = (IGuiProvider)Activator.CreateInstance(
                            customProviderType,
                            new object[] { member.Name, getter, setter }
                        );
                        control = customProvider.CreateGui(_lastCtx);
                    }
                    // ==========================================
                    // 4. FALLBACK TO STANDARD REFLECTION
                    // ==========================================
                    else
                    {
                        control = CreateControlForMember(memberType, member.Name, getter, setter, isPrivate);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"{LogPrefix} ❌ FATAL CRASH while processing member '{member.Name}': {ex.Message}\nInner: {ex.InnerException?.Message}\n{ex.StackTrace}");
                }

                if (control != null)
                {
                    rootBuilder.OnBuild(ve => ve.Add(control));
                    Debug.Log($"{LogPrefix} Successfully generated UI control for {member.Name}.");
                }
            }

            Debug.Log($"{LogPrefix} 🏁 CreateGui completed.");
            return rootBuilder.CreateGui(ctx);
        }

        private VisualElement CreateControlForMember(Type type, string name, Func<object> getter, Action<object> setter, bool isPrivate)
        {
            Debug.Log($"{LogPrefix} Routing UI creation for '{name}' (Type: {type.Name})...");

            // ==========================================
            // 1. SAFETY NET: Try to safely retrieve the value
            // ==========================================
            object value = null;
            try
            {
                value = getter();
            }
            catch (Exception ex)
            {
                var realError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                Debug.LogError($"{LogPrefix} Getter failed for '{name}': {realError}");
                return new ForgeLabelBuilder($"{name}: [Getter Failed: {realError}]").WithColor(Color.red).Build();
            }

            // ==========================================
            // 2. REPORTING MODE FOR PRIVATE DATA
            // ==========================================
            if (isPrivate)
            {
                var row = new VisualElement { style = { flexDirection = FlexDirection.Row, justifyContent = Justify.SpaceBetween, marginBottom = 2 } };
                row.Add(new Label(name + ":") { style = { color = Color.gray } });

                var valLabel = new Label();
                valLabel.style.color = Color.cyan;

                // Native Live Polling for Read-Only Data!
                row.schedule.Execute(() => {
                    try { valLabel.text = getter()?.ToString() ?? "null"; }
                    catch { valLabel.text = "Error"; }
                }).Every(100);

                row.Add(valLabel);
                return row;
            }

            // ==========================================
            // 3. STACKING THE DECK: Check the Global Factory Registry First
            // ==========================================
            if (TypeGuiProviderFactory.TryGetCustomProvider(type, value, out IGuiProvider customProvider))
            {
                Debug.Log($"{LogPrefix} Found Custom Provider in Global Factory for '{name}'.");
                return new ForgeFoldoutBuilder($"{name.ToUpper()} (CUSTOM UI)", false)
                    .WithMarginTop(5)
                    .WithMarginBottom(5)
                    .AddChild(customProvider.CreateGui(_lastCtx))
                    .Build();
            }

            // 4. PRIMITIVES
            if (type == typeof(string)) return _factory.CreateTextField(name, (string)value, val => setter(val));
            if (type == typeof(bool)) return _factory.CreateToggle(name, value != null && (bool)value, val => setter(val));
            if (type == typeof(float) || type == typeof(int))
            {
                return _factory.CreateTextField(name, value?.ToString(), val => {
                    if (type == typeof(float) && float.TryParse(val, out float f)) setter(f);
                    if (type == typeof(int) && int.TryParse(val, out int i)) setter(i);
                });
            }
            if (type == typeof(Color)) return _factory.CreateColorField(name, value != null ? (Color)value : Color.white, val => setter(val));

            if (type.IsEnum)
            {
                Debug.Log($"{LogPrefix} Routing '{name}' as Enum.");
                return CreateEnumControl(type, name, () => value, setter);
            }

            // Catch dictionaries and multidimensional arrays safely
            if (type.IsArray && type.GetArrayRank() > 1)
                return new ForgeLabelBuilder($"{name}: [{type.GetArrayRank()}D Map Array]").WithColor(Color.cyan).WithMarginTop(5).Build();
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                return new ForgeLabelBuilder($"{name}: [Dictionary]").WithColor(Color.cyan).WithMarginTop(5).Build();

            // 5. COLLECTIONS
            if (typeof(IEnumerable).IsAssignableFrom(type) && type != typeof(string))
            {
                if (_recursionDepth <= 0)
                {
                    Debug.LogWarning($"{LogPrefix} Collection '{name}' reached recursion limit.");
                    return new ForgeLabelBuilder($"{name}: [ Collection ] (Max Depth Reached)").WithColor(Color.yellow).Build();
                }

                Debug.Log($"{LogPrefix} Routing '{name}' as Collection.");
                return CreateCollectionControl(type, name, () => value);
            }

            // 6. COMPLEX CLASSES (Nested Reflection)
            if (type.IsClass && !type.IsAbstract)
            {
                Debug.Log($"{LogPrefix} Routing '{name}' as nested Complex Class. Depth: {_recursionDepth}");
                if (value == null) return new ForgeLabelBuilder($"{name}: [Null Reference]").WithColor(Color.gray).Build();

                if (_recursionDepth <= 0)
                {
                    Debug.LogWarning($"{LogPrefix} Complex Class '{name}' reached recursion limit.");
                    return new ForgeLabelBuilder($"{name}: [ {type.Name} ] (Max Depth Reached)").WithColor(Color.yellow).Build();
                }

                try
                {
                    var subBuilderType = typeof(ReflectiveGuiBuilder<>).MakeGenericType(type);
                    var subBuilder = Activator.CreateInstance(subBuilderType, new object[] { value, null });

                    // PASS THE BATON
                    var withRecursionMethod = subBuilderType.GetMethod("WithRecursion");
                    if (withRecursionMethod != null)
                    {
                        withRecursionMethod.Invoke(subBuilder, new object[] { _recursionDepth - 1 });
                    }

                    // Pass along the scoped providers so nested objects respect the dictionary!
                    var withProvidersMethod = subBuilderType.GetMethod("WithProviders");
                    if (withProvidersMethod != null)
                    {
                        withProvidersMethod.Invoke(subBuilder, new object[] { _scopedProviders });
                    }

                    var createGuiMethod = subBuilderType.GetMethod("CreateGui");

                    return new ForgeFoldoutBuilder(name.ToUpper(), false)
                        .WithMarginTop(5)
                        .WithMarginBottom(5)
                        .AddChild((VisualElement)createGuiMethod.Invoke(subBuilder, new object[] { _lastCtx }))
                        .Build();
                }
                catch (Exception ex)
                {
                    var realError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                    Debug.LogError($"{LogPrefix} ❌ Failed to build nested class '{name}': {realError}");
                    return new ForgeLabelBuilder($"{name}: [Nested Build Failed]").WithColor(Color.red).Build();
                }
            }

            Debug.LogWarning($"{LogPrefix} Unsupported type '{type.Name}' for member '{name}'.");
            return new ForgeLabelBuilder($"{name} (Unsupported: {type.Name})").WithColor(Color.red).Build();
        }

        private VisualElement CreateEnumControl(Type type, string name, Func<object> getter, Action<object> setter)
        {
            try
            {
                var method = typeof(EditorControlFactory).GetMethod(nameof(EditorControlFactory.CreateEnumField));
                if (method != null)
                    return (VisualElement)method.MakeGenericMethod(type).Invoke(_factory, new object[] { name, getter(), setter });
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"{LogPrefix} Factory Enum generation failed. Using Native Fallback. Error: {ex.Message}");
            }

            var enumVal = (Enum)getter();
            var field = new EnumField(name, enumVal);
            field.RegisterValueChangedCallback(e => setter(e.newValue));
            return field;
        }

        private VisualElement CreateCollectionControl(Type type, string name, Func<object> getter)
        {
            var collection = (IEnumerable)getter();
            if (collection == null)
            {
                Debug.Log($"{LogPrefix} Collection '{name}' is null. Returning placeholder.");
                return new ForgeLabelBuilder($"{name}: [Null Collection]").WithColor(Color.gray).Build();
            }

            Type elementType = type.IsArray ? type.GetElementType() : type.GetGenericArguments().FirstOrDefault();

            if (elementType == null)
            {
                Debug.LogWarning($"{LogPrefix} Could not determine element type for collection '{name}'.");
                return new ForgeLabelBuilder($"{name}: [Unknown Element Type]").Build();
            }
            if (!elementType.IsClass)
            {
                Debug.LogWarning($"{LogPrefix} Element type '{elementType.Name}' in collection '{name}' is not a class. Aborting CRUD builder.");
                return new ForgeLabelBuilder($"{name}: [Unsupported Data Collection - Not a Class]").Build();
            }

            Debug.Log($"{LogPrefix} Building CRUD interface for collection '{name}' with elements of type '{elementType.Name}'.");

            try
            {
                var crudType = typeof(CRUD_Builder<>).MakeGenericType(elementType);

                var getProviderMethod = GetType().GetMethod(nameof(GetTypedProvider), BindingFlags.NonPublic | BindingFlags.Instance).MakeGenericMethod(elementType);
                var getNameMethod = GetType().GetMethod(nameof(GetTypedNameFunc), BindingFlags.NonPublic | BindingFlags.Instance).MakeGenericMethod(elementType);
                var getRendererMethod = GetType().GetMethod(nameof(GetTypedRendererFunc), BindingFlags.NonPublic | BindingFlags.Instance).MakeGenericMethod(elementType);
                var getActionMethod = GetType().GetMethod(nameof(GetTypedAction), BindingFlags.NonPublic | BindingFlags.Instance).MakeGenericMethod(elementType);

                var crudInstance = Activator.CreateInstance(crudType, new object[]
                {
                    name,
                    getProviderMethod.Invoke(this, new object[] { collection }),
                    getNameMethod.Invoke(this, null),
                    getRendererMethod.Invoke(this, null),
                    getActionMethod.Invoke(this, null),
                    getActionMethod.Invoke(this, null),
                    null, null
                });

                var crudElement = (VisualElement)crudType.GetMethod("CreateGui").Invoke(crudInstance, new object[] { _lastCtx });

                return new ForgeFoldoutBuilder($"{name.ToUpper()} (COLLECTION)", false)
                    .AddChild(crudElement)
                    .Build();
            }
            catch (Exception ex)
            {
                var realError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                Debug.LogError($"{LogPrefix} ❌ Failed to create CRUD Collection for '{name}': {realError}\n{ex.StackTrace}");
                return new ForgeLabelBuilder($"{name}: [CRUD Build Failed]").WithColor(Color.red).Build();
            }
        }

        private Func<IEnumerable<TItem>> GetTypedProvider<TItem>(IEnumerable collection) => () => collection?.Cast<TItem>() ?? Enumerable.Empty<TItem>();
        private Func<TItem, string> GetTypedNameFunc<TItem>() => item => item?.ToString() ?? "Element";
        private Func<TItem, VisualElement> GetTypedRendererFunc<TItem>() => item => {
            try
            {
                var itemReflectorType = typeof(ReflectiveGuiBuilder<>).MakeGenericType(typeof(TItem));
                var itemReflector = Activator.CreateInstance(itemReflectorType, new object[] { item, null });

                var withRecursionMethod = itemReflectorType.GetMethod("WithRecursion");
                if (withRecursionMethod != null)
                {
                    withRecursionMethod.Invoke(itemReflector, new object[] { _recursionDepth - 1 });
                }

                // Make sure CRUD items also respect scoped providers!
                var withProvidersMethod = itemReflectorType.GetMethod("WithProviders");
                if (withProvidersMethod != null)
                {
                    withProvidersMethod.Invoke(itemReflector, new object[] { _scopedProviders });
                }

                return (VisualElement)itemReflectorType.GetMethod("CreateGui").Invoke(itemReflector, new object[] { _lastCtx });
            }
            catch (Exception ex)
            {
                Debug.LogError($"{LogPrefix} ❌ Failed to render CRUD item: {ex.InnerException?.Message ?? ex.Message}");
                return new Label("Item Render Failed");
            }
        };
        private Action<TItem> GetTypedAction<TItem>() => item => { };

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) => _lastCtx?.OnBuilt?.Invoke(GraphicalUserInterfaceBuilder.ConvertFromUIDocument(assetPath));
    }
}