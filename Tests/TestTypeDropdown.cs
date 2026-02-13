using UnityEngine;

namespace TypeDropdown.Tests
{
	[CreateAssetMenu(menuName = "Testing/TypeDropdown")]
	public class TestTypeDropdown : ScriptableObject
	{
		[Header("Error: wrong type")]
		[SerializeField, TypeDropdown] private int typeInt;
		[Header("Error: no constraints")]
		[SerializeField, TypeDropdown] private string typeString;
		[Header("String with constraints")]
		[SerializeField, TypeDropdown(typeof(TestBaseClass))] private string typeAString;
		[Header("Object reference without constraints")]
		[SerializeReference, TypeDropdown] private TestBaseClass typeBaseClass;
	}
}