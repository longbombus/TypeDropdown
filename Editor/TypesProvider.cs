using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace TypeDropdown.Editor
{
	public class TypesProvider
	{
		private readonly Dictionary<TypesFilter, List<Type>> filteredTypes = new();

		public bool TryGetType(string typeName, out Type type)
		{
			try
			{
				type = Type.GetType(typeName);
				return type != null;
			}
			catch (ArgumentNullException) { /* ok */ }
			catch (Exception e)
			{
				Debug.LogException(e);
			}

			type = null;
			return false;
		}

		// Src: https://docs.unity3d.com/ScriptReference/SerializedProperty-managedReferenceFieldTypename.html
		// Unity style type name format: "[assembly-name] [namespace.][parent-class-names][classname]" where:
		// - [assembly-name] is the name of the assembly that contains the target type
		// - [namespace.] is an optional (if empty) namespace followed by a '.'
		// - [parent-class-names] is a '/' separated list of optional parent class names (in the case of nested class definitions)
		// - [classname] is the managed reference field class name.
		public bool TryGetTypeUnityStyle(string unityStyleTypeName, out Type type)
		{
			do
			{
				if (string.IsNullOrEmpty(unityStyleTypeName))
					break;

				// Unity format: "[assembly] [namespace.Type/Inner]"
				int spaceIndex = unityStyleTypeName.IndexOf(' ');
				if (spaceIndex < 0)
					break;

				StringBuilder dotNetTypeName = new StringBuilder(unityStyleTypeName.Length + 2);

				var typeNameLength = unityStyleTypeName.Length - spaceIndex - 1;
				dotNetTypeName.Append(unityStyleTypeName, spaceIndex + 1, typeNameLength);
				dotNetTypeName.Replace('/', '+');
				dotNetTypeName.Append(", ");
				dotNetTypeName.Append(unityStyleTypeName, 0, spaceIndex);

				return TryGetType(dotNetTypeName.ToString(), out type);
			} while (false);

			type = null;
			return false;
		}

		public string GetTypeName(Type type)
			=> type.AssemblyQualifiedName;

		public IReadOnlyList<Type> GetTypes(TypesFilter filter)
		{
			if (filteredTypes.TryGetValue(filter, out var filterTypes))
				return filterTypes;

			filterTypes = new List<Type>();
			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				IEnumerable<Type> types;
				try { types = assembly.GetTypes(); }
				catch (ReflectionTypeLoadException e) { types = e.Types.Where(t => t != null); }

				foreach (var type in types)
					if (filter.IsMatch(type))
						filterTypes.Add(type);
			}

			filteredTypes[filter] = filterTypes;
			return filterTypes;
		}
	}
}