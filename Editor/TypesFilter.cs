using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

#nullable enable

namespace TypeDropdown.Editor
{
	public readonly struct TypesFilter : IEquatable<TypesFilter>
	{
		public readonly Regex? NameRegex;
		public readonly Type[] BaseTypes;

		public TypesFilter(IEnumerable<Type> baseTypes, string? namePattern)
		{
			BaseTypes = baseTypes.OrderBy(t => t.Name).ToArray();
			NameRegex = namePattern != null ? new Regex(namePattern, RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace) : null;

			if (BaseTypes.Length == 0 && namePattern == null)
				throw new ArgumentException("At least one base type or a name pattern must be provided");
		}

		public bool Equals(TypesFilter other)
			=> NameRegex == other.NameRegex && BaseTypes.SequenceEqual(other.BaseTypes);

		public override bool Equals(object? obj)
			=> obj is TypesFilter other && Equals(other);

		public override int GetHashCode()
			=> BaseTypes.Aggregate(HashCode.Combine(NameRegex), HashCode.Combine);

		public bool IsMatch(Type type)
			=> (BaseTypes == null || BaseTypes.Any(t => t.IsAssignableFrom(type)))
			&& (NameRegex == null || NameRegex.IsMatch(type.Name));
	}
}