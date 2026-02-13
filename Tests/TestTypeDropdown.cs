using UnityEngine;

namespace TypeDropdown.Tests
{
	public class BaseClass
	{
	}

	public class TestDerivedClass : BaseClass
	{
	}

	[System.Serializable]
	public class TestSerializableClass : BaseClass
	{
		[SerializeField] private int integer;
	}

	[System.Serializable]
	public class TestSerializableWithReferencesClass : BaseClass
	{
		[SerializeField] private int integer;
		[SerializeReference, TypeDropdown] private BaseClass other;
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