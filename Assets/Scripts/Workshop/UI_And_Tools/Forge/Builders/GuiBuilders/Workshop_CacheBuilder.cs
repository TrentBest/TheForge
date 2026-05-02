using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders
{
    /// <summary>
    /// A centralized graphical builder for handling item caching and persistence.
    /// Allows other builders to plug in their data types without rewriting storage logic.
    /// </summary>
    public class Workshop_CacheBuilder<T> : GraphicalUserInterfaceBuilder where T : class, new()
    {
        private readonly string _cacheKey;
        private readonly Action<T> _onItemSelected;
        private readonly Func<List<T>> _onFetchAll;
        private readonly Action<T> _onSaveItem;
		private readonly Action _onClearTemporaryCache;

		private ListView _itemListView;
        private List<T> _cachedItems = new List<T>();

        public Workshop_CacheBuilder(
            string title,
            string cacheKey,
            Action<T> onItemSelected,
            Func<List<T>> onFetchAll,
            Action<T> onSaveItem,
            Action onClearTemporaryCache)
        {
            Title = $"{title} Cache Manager";
            _cacheKey = cacheKey;
            _onItemSelected = onItemSelected;
            _onFetchAll = onFetchAll;
            _onSaveItem = onSaveItem;
            _onClearTemporaryCache = onClearTemporaryCache;
        }

        public new VisualElement CreateGui(GuiContext ctx)
        {
            var root = new VisualElement();
            root.style.flexDirection = FlexDirection.Column;
            root.style.flexGrow = 1;

            // 1. Toolbar for Cache Actions
            var toolbar = new VisualElement();
            toolbar.style.flexDirection = FlexDirection.Row;
            toolbar.style.paddingBottom = 5;

            var refreshBtn = new Button(RefreshCache) { text = "↻ Refresh" };
            var clearBtn = new Button(ClearCache) { text = "🗑 Clear Temporary" };

            toolbar.Add(refreshBtn);
            toolbar.Add(clearBtn);
            root.Add(toolbar);

            // 2. The List View
            _itemListView = new ListView(_cachedItems, 30, MakeItem, BindItem);
            _itemListView.selectionType = SelectionType.Single;
            _itemListView.selectionChanged += objects => {
                if (objects.FirstOrDefault() is T item) _onItemSelected?.Invoke(item);
            };
            _itemListView.style.flexGrow = 1;

            root.Add(_itemListView);

            RefreshCache();
            return root;
        }

        private VisualElement MakeItem() => new Label();

        private void BindItem(VisualElement ve, int index)
        {
            if (ve is Label label && index < _cachedItems.Count)
            {
                // Reflection fallback if T doesn't have a 'Name' property
                var nameProp = _cachedItems[index].GetType().GetProperty("Name")?.GetValue(_cachedItems[index]);
                label.text = nameProp?.ToString() ?? $"Item {index}";
            }
        }

        public void RefreshCache()
        {
            // First check the DataWarehouse temporary storage
            // This leverages your existing DW extensions
            _cachedItems = _onFetchAll?.Invoke() ?? new List<T>();
            _itemListView?.Rebuild();

            Debug.Log($"[Workshop Cache] Refreshed {_cacheKey}. Found {_cachedItems.Count} items.");
        }

        public void ClearCache()
        {
			// Uses the DataWarehouse logic we recently fixed
			_onClearTemporaryCache?.Invoke(); // Execute the passed-in logic
			RefreshCache();
		}

        public void SaveToCache(T item)
        {
            _onSaveItem?.Invoke(item);
            RefreshCache();
        }
    }
}