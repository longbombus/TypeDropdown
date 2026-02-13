using UnityEngine;

namespace TypeDropdown.Tests
{
	public class BaseClass
	{
	}

	public class TestDerivedClass : BaseClass
	{
	}

	public class TestOtherClass
	{
		public class TestNestedClass : BaseClass
		{
		}
	}

	[CreateAssetMenu(menuName = "Testing/TypeDropdown")]
	public class TestTypeDropdown : ScriptableObject
	{
		[Header("Error: wrong type")]
		[SerializeField, TypeDropdown] private int typeInt;
		[Header("Error: no constraints")]
		[SerializeField, TypeDropdown] private string typeString;
		[Header("String with constraints")]
		[SerializeField, TypeDropdown(typeof(BaseClass))] private string typeAString;
		[Header("Object reference without constraints")]
		[SerializeReference, TypeDropdown] private BaseClass typeBaseClass;
	}
}