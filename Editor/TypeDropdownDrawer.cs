using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;

namespace TypeDropdown.Editor
{
	[CustomPropertyDrawer(typeof(TypeDropdownAttribute))]
	public class TypeDropdownDrawer : PropertyDrawer
	{
		private static readonly TypesProvider TypesProvider = new();

		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			Behaviour behaviour;

			switch (property.propertyType)
			{
				case SerializedPropertyType.String:
					behaviour = Behaviour.String;
					break;

				case SerializedPropertyType.ManagedReference:
					behaviour = Behaviour.Reference;
					break;

				default:
					return new HelpBox(
						$"{FormatHighlighted(property.displayName)} must be {FormatHighlighted("[SerializeReference] object")} or {FormatHighlighted("[SerializeField] string")}",
						HelpBoxMessageType.Error
					);
			}

			var typeDropdownAttribute = (TypeDropdownAttribute)attribute;
			TypesFilter typesFilter;
			try
			{
				var filterTypes = Enumerable.Empty<Type>();

				if (behaviour == Behaviour.Reference)
					filterTypes = filterTypes.Append(TypesProvider.GetType(property.managedReferenceFullTypename));

				if (typeDropdownAttribute.BaseTypes != null)
					filterTypes = filterTypes.Concat(typeDropdownAttribute.BaseTypes);

				typesFilter = new TypesFilter(filterTypes, typeDropdownAttribute.NamePattern);
			}
			catch (Exception e)
			{
				return new HelpBox(
					$"{property.displayName}: {e.Message}",
					HelpBoxMessageType.Error
				);
			}

			var types = TypesProvider.GetTypes(typesFilter).ToList();
			var typeLabels = new List<string>(types.Count + 1);
			var typeNames = new List<string>(types.Count + 1);

			typeLabels.Add("null");
			typeNames.Add(string.Empty);

			foreach (var t in types)
			{
				typeLabels.Add(GetTypeLabel(t));
				typeNames.Add(t.FullName);
			}

			var currentValue = property.stringValue ?? string.Empty;
			int currentIndex = FindBestIndex(typeNames, types, currentValue);

			var root = new VisualElement();
			var dropdown = new PopupField<string>(ObjectNames.NicifyVariableName(property.name), typeLabels, currentIndex);
			dropdown.RegisterValueChangedCallback(evt =>
			{
				int idx = typeLabels.IndexOf(evt.newValue);
				if (idx < 0)
					idx = 0;
				string selected = typeNames[idx];
				property.stringValue = selected;
				property.serializedObject.ApplyModifiedProperties();
			});

			root.Add(dropdown);
			return root;
		}

		private static int FindBestIndex(List<string> valueChoices, List<Type> types, string currentValue)
		{
			if (string.IsNullOrEmpty(currentValue))
				return 0;

			int idx = valueChoices.IndexOf(currentValue);
			if (idx >= 0) return idx;

			for (int i = 1; i < valueChoices.Count; i++)
			{
				var t = types[i - 1];
				if (string.Equals(t.Name, currentValue, StringComparison.Ordinal))
					return i;
			}
			return 0;
		}

		private static string GetTypeLabel(Type t)
			=> string.IsNullOrEmpty(t.Namespace) ? t.Name : $"{t.Namespace}/{t.Name}";

		private static string FormatHighlighted(string code)
			=> $"<b><nobr><noparse>{code}</noparse></nobr></b>";

		private enum Behaviour : byte
		{
			Reference,
			String,
		}
	}
}