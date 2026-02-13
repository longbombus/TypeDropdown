using System;
using UnityEngine;

#nullable enable

namespace TypeDropdown
{
	/// <summary>
	/// Attribute to show a dropdown of types in the inspector.
	/// Can be applied to <c>[SerializeReference]</c> object or <c>[SerializeField]</c> string.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public class TypeDropdownAttribute : PropertyAttribute
	{
		public readonly string? NamePattern;
		public readonly Type[]? BaseTypes;

		/// <inheritdoc cref="TypeDropdownAttribute(string, Type[])"/>
		public TypeDropdownAttribute()
			: this((string?)null, null)
		{
		}

		/// <inheritdoc cref="TypeDropdownAttribute(string, Type[])"/>
		public TypeDropdownAttribute(string namePattern)
			: this(namePattern, null)
		{
		}

		/// <inheritdoc cref="TypeDropdownAttribute(string, Type[])"/>
		public TypeDropdownAttribute(params Type[] baseTypes)
			: this(null, baseTypes)
		{
		}

		/// <param name="namePattern"> Regular expression pattern to filter type names </param>
		/// <param name="baseTypes"> Base classes or interfaces to filter types. </param>
		public TypeDropdownAttribute(string? namePattern, params Type[]? baseTypes)
		{
			NamePattern = namePattern;
			BaseTypes = baseTypes;
		}
	}
}