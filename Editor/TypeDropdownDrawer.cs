using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
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
				var filterTypes = typeDropdownAttribute.BaseTypes ?? Enumerable.Empty<Type>();

				if (behaviour == Behaviour.Reference && TypesProvider.TryGetTypeUnityStyle(property.managedReferenceFieldTypename, out var baseType))
					filterTypes = filterTypes.Append(baseType);

				typesFilter = new TypesFilter(filterTypes, typeDropdownAttribute.NamePattern);
			}
			catch (Exception e)
			{
				return new HelpBox(
					$"{property.displayName}: {e.Message}",
					HelpBoxMessageType.Error
				);
			}

			var types = TypesProvider.GetTypes(typesFilter);
			var typeLabels = new List<string>(types.Count + 1);
			var typeNames = new List<string>(types.Count + 1);

			typeLabels.Add("null");
			typeNames.Add(string.Empty);

			foreach (var t in types)
			{
				typeLabels.Add(GetTypeLabel(t));
				typeNames.Add(TypesProvider.GetTypeName(t));
			}

			var currentValue = behaviour switch
			{
				Behaviour.Reference => property.managedReferenceValue?.GetType().AssemblyQualifiedName,
				Behaviour.String => property.stringValue,
				_ => throw new ArgumentOutOfRangeException(nameof(behaviour), behaviour, null)
			};

			int currentIndex = typeNames.IndexOf(currentValue);
			if (currentIndex < 0)
				currentIndex = 0;

			var root = new VisualElement();
			var dropdown = new PopupField<string>(property.displayName, typeLabels, currentIndex);
			dropdown.RegisterValueChangedCallback(evt =>
			{
				int idx = typeLabels.IndexOf(evt.newValue);
				if (idx < 0)
					idx = 0;
				string selectedTypeName = typeNames[idx];
				switch (behaviour)
				{
					case Behaviour.Reference:
						string oldValueJson = null;
						if (property.managedReferenceValue != null)
						{
							if (TypesProvider.GetTypeName(property.managedReferenceValue.GetType()) == selectedTypeName)
								return;

							oldValueJson = JsonUtility.ToJson(property.managedReferenceValue, false);
						}

						if (TypesProvider.TryGetType(selectedTypeName, out var selectedType))
						{
							var selectedTypeInstance = Activator.CreateInstance(selectedType);
							if (oldValueJson != null)
								JsonUtility.FromJsonOverwrite(oldValueJson, selectedTypeInstance);

							property.managedReferenceValue = selectedTypeInstance;
						}
						else
						{
							property.managedReferenceValue = null;
						}

						break;

					case Behaviour.String:
						property.stringValue = selectedTypeName;
						break;
				}
				property.serializedObject.ApplyModifiedProperties();
			});

			root.Add(dropdown);
			return root;
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