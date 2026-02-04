using System;
using UnityEngine;

#nullable enable

namespace TypeDropdown
{
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public class TypeDropdownAttribute : PropertyAttribute
	{
		public readonly string? NamePattern;
		public readonly Type[]? BaseTypes;

		public TypeDropdownAttribute()
			: this((string?)null, null)
		{
		}

		public TypeDropdownAttribute(string namePattern)
			: this(namePattern, null)
		{
		}

		public TypeDropdownAttribute(params Type[] baseTypes)
			: this(null, baseTypes)
		{
		}

		public TypeDropdownAttribute(string? namePattern, params Type[]? baseTypes)
		{
			NamePattern = namePattern;
			BaseTypes = baseTypes;
		}
	}
}