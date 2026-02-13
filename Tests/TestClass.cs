using UnityEngine;

namespace TypeDropdown.Tests
{
    public class TestBaseClass
    {
    }

    public class TestClass : TestBaseClass
    {

    }

    public class TestDerivedClass : TestBaseClass
    {
    }

    public abstract class TestAbstractClass : TestBaseClass
    {
    }

    [System.Serializable]
    public class TestSerializableClass : TestBaseClass
    {
        [SerializeField] private int integer;
    }

    [System.Serializable]
    public class TestSerializableWithReferencesClass : TestBaseClass
    {
        [SerializeField] private int integer;
        [SerializeReference, TypeDropdown] private TestBaseClass other;
    }

    public class TestOtherClass
    {
        public class TestNestedClass : TestBaseClass
        {
        }
    }
}