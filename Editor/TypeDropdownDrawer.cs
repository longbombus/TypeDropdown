using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace TypeDropdown.Editor
{
	[CustomPropertyDrawer(typeof(TypeDropdownAttribute))]
	public class TypeDropdownDrawer : PropertyDrawer
	{
		private static readonly TypesCache TypesCache = new();

		private Button pickButton;

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

				if (behaviour == Behaviour.Reference && TypeUtility.TryGetTypeUnityStyle(property.managedReferenceFieldTypename, out var baseType))
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

			var types = TypesCache.GetTypes(typesFilter);
			var typeLabels = new List<string>(types.Count + 1);
			var typeNames = new List<string>(types.Count + 1);

			typeLabels.Add("null");
			typeNames.Add(string.Empty);

			foreach (var t in types)
			{
				typeLabels.Add(GetTypeLabel(t));
				typeNames.Add(TypeUtility.GetTypeName(t));
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
			PopupField<string> dropdown;

			VisualElement dropdownRow;

			switch (behaviour)
			{
				case Behaviour.Reference:
					var propertyField = new PropertyField(property);
					root.Add(propertyField);

					dropdownRow = new VisualElement
					{
						style =
						{
							position = Position.Absolute,
							left = Length.Percent(38.2f),
							right = 0,
							top = 0,
						},
					};
					root.Add(dropdownRow);

					dropdown = new PopupField<string>(null, typeLabels, currentIndex)
					{
						style =
						{
							flexGrow = 1
						}
					};
					dropdownRow.Add(dropdown);
					break;

				case Behaviour.String:
					dropdownRow = root;
					dropdown = new PopupField<string>(property.displayName, typeLabels, currentIndex)
					{
						style =
						{
							flexGrow = 1
						}
					};
					dropdownRow.Add(dropdown);
					break;

				default:
					throw new ArgumentOutOfRangeException(nameof(behaviour), behaviour, null);
			}

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
							if (TypeUtility.GetTypeName(property.managedReferenceValue.GetType()) == selectedTypeName)
								return;

							oldValueJson = JsonUtility.ToJson(property.managedReferenceValue, false);
						}

						if (TypeUtility.TryGetType(selectedTypeName, out var selectedType))
						{
							var selectedTypeInstance = Activator.CreateInstance(selectedType);
							if (oldValueJson != null)
								JsonUtility.FromJsonOverwrite(oldValueJson, selectedTypeInstance);

							property.managedReferenceValue = selectedTypeInstance;
							pickButton.SetEnabled(true);
						}
						else
						{
							property.managedReferenceValue = null;
							pickButton.SetEnabled(false);
						}

						break;

					case Behaviour.String:
						property.stringValue = selectedTypeName;
						pickButton.SetEnabled(!string.IsNullOrWhiteSpace(selectedTypeName));
						break;
				}
				property.serializedObject.ApplyModifiedProperties();
			});

			dropdownRow.style.flexDirection = FlexDirection.Row;
			pickButton = new Button(() =>
			{
				Type type;
				switch (behaviour)
				{
					case Behaviour.Reference: TypeUtility.TryGetTypeUnityStyle(property.managedReferenceFullTypename, out type); break;
					case Behaviour.String: TypeUtility.TryGetType(property.stringValue, out type); break;
					default: type = null; break;
				}

				if (type == null)
					return;

				if (!TypeUtility.TryPickTypeScript(type))
					Debug.LogError($"Couldn't find script according type {type}");
			});
			pickButton.AddToClassList("unity-object-field__selector");
			pickButton.SetEnabled(GetType(behaviour, property) != null);
			dropdownRow.Add(pickButton);

			return root;
		}

		private static Type GetType(Behaviour behaviour, SerializedProperty property)
			=> behaviour switch
			{
				Behaviour.Reference => property.managedReferenceValue?.GetType(),
				Behaviour.String => TypeUtility.TryGetType(property.stringValue, out var type) ? type : null,
				_ => null
			};

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