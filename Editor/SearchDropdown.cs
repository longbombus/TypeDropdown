using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace TypeDropdown.Editor
{
	public class SearchDropdown<TItem> : VisualElement
		where TItem : class
	{
		private readonly PrefixTree<ItemInfo> items = new();

		private VisualElement selectedValueFrame;
		private Clickable selectedValueClickable;
		private Label selectedValueLabel;
		private VisualElement dropdownFrame;
		private ToolbarSearchField dropdownSearchField;
		private TreeView dropdownTreeView;

		private event Action<TItem> ItemSelected;

		public SearchDropdown(IEnumerable<TItem> itemValues, Func<TItem, string> pathSelector, Func<TItem, string> descriptionSelector)
		{
			foreach (var itemValue in itemValues)
				items.Add(pathSelector(itemValue), new ItemInfo(itemValue, descriptionSelector(itemValue)));

			AddToClassList(BasePopupField<Type, string>.ussClassName);

			selectedValueFrame = new VisualElement
			{
				focusable = true,
				style =
				{
					flexDirection = FlexDirection.Row,
				}
			};
			selectedValueFrame.AddToClassList(BasePopupField<Type, string>.inputUssClassName);
			Add(selectedValueFrame);

			selectedValueClickable = new Clickable(ShowDropdown);
			selectedValueFrame.AddManipulator(selectedValueClickable);

			selectedValueLabel = new Label("Value label")
			{
				style =
				{
					flexGrow = 1,
				}
			};
			selectedValueFrame.Add(selectedValueLabel);

			var selectedValueArrow = new VisualElement();
			selectedValueArrow.AddToClassList(BasePopupField<Type, string>.arrowUssClassName);
			selectedValueFrame.Add(selectedValueArrow);
		}

		private void HideDropdown()
		{
			if (dropdownFrame == null)
				return;

			dropdownFrame.RemoveFromHierarchy();
			dropdownFrame = null;
		}

		private void ShowDropdown()
		{
			HideDropdown();

			dropdownFrame = new VisualElement
			{
				style =
				{
					position = Position.Absolute,
					left = 0,
					right = 0,
					top = Length.Percent(100),
				}
			};
			Add(dropdownFrame);

			dropdownSearchField = new ToolbarSearchField();
			dropdownSearchField.RegisterCallback<FocusOutEvent>(UpdateDropdownFocus);
			dropdownFrame.Add(dropdownSearchField);

			dropdownTreeView = new TreeView
			{
				selectionType = SelectionType.Single,
				style =
				{
					flexGrow = 1,
				}
			};
			dropdownTreeView.makeItem = () => new Label("item");
			dropdownTreeView.bindItem = (element, index) =>
			{
				var itemInfo = dropdownTreeView.GetItemDataForIndex<ItemInfo>(index);
				((Label)element).text = itemInfo?.Description ?? "no info";
			};
			dropdownTreeView.SetRootItems(Array.Empty<TreeViewItemData<ItemInfo>>());
			dropdownTreeView.RegisterCallback<FocusOutEvent>(UpdateDropdownFocus);
			dropdownFrame.Add(dropdownTreeView);

			if (items.Root.HasChildren)
			{
				int id = 0;
				var stack = new Stack<(PrefixTree<ItemInfo>.Node Node, int ParentId)>();
				foreach (var itemNode in items.Root.Children)
					stack.Push((itemNode, -1));

				while (stack.TryPop(out var item))
				{
					dropdownTreeView.AddItem(new TreeViewItemData<ItemInfo>(id, item.Node.Value), item.ParentId, rebuildTree: false);

					if (item.Node.HasChildren)
						foreach (var childNode in item.Node.Children)
							stack.Push((childNode, id));

					++id;
				}

				dropdownTreeView.Rebuild();
			}

			dropdownSearchField.Focus();
		}

		private void UpdateDropdownFocus(FocusOutEvent evt)
		{
			for (var p = evt.relatedTarget as VisualElement; p != null; p = p.parent)
				if (p == dropdownFrame)
				{
					dropdownSearchField.Focus();
					return;
				}

			HideDropdown();
		}

		private class ItemInfo
		{
			public readonly TItem Value;
			public readonly string Description;

			public ItemInfo(TItem value, string description)
			{
				Value = value;
				Description = description;
			}
		}
	}
}