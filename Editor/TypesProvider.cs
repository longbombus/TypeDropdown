using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace TypeDropdown.Editor
{
	public class TypesProvider
	{
		private readonly Dictionary<TypesFilter, List<Type>> filteredTypes = new();

		public Type GetType(string typeName)
			=> Type.GetType(typeName);

		public string GetTypeName(Type type)
			=> type.AssemblyQualifiedName;

		public IReadOnlyCollection<Type> GetTypes(TypesFilter filter)
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