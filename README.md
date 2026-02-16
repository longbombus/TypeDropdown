# Type Dropdown

The `TypeDropdown` attribute adds a type dropdown to the Unity Inspector. It works for:

- fields with `[SerializeReference]` (select a concrete type for a managed reference);
- string fields with `[SerializeField]` (stores the Assembly Qualified Name in the string).

The package uses UI Toolkit (UIElements) for the selector, so you can use `CustomPropertyDrawer` with modern `CreatePropertyGUI` to draw subfields.

## Usage in code

### 1) `[SerializeReference]` field

Just mark a field with `[SerializeReference, TypeDropdown]` and it will show a dropdown of all non-abstract types that can be assigned to the field.

```csharp
using UnityEngine;
using TypeDropdown;

public abstract class WeaponBase { }
public class Sword : WeaponBase { }
public class Bow : WeaponBase { }

public class Damager : MonoBehaviour
{
    [SerializeReference, TypeDropdown]
    private WeaponBase weapon;
}
```

### 2) `[SerializeField] string` field

For string fields you **must** provide at least one constraint (base type or name pattern).

```csharp
using UnityEngine;
using TypeDropdown;

public class Example : MonoBehaviour
{
    [SerializeField, TypeDropdown(typeof(WeaponBase))]
    private string weaponTypeName;

    private void Start()
    {
        if (string.IsNullOrEmpty(weaponTypeName))
            return;
        
        var weaponType = Type.GetType(weaponTypeName);
        if (weaponType == null)
            return;

        var weapon = (WeaponBase)Activator.CreateInstance(weaponType);
        // ...
    }
}
```

### Type filtering by Regex

All types that match the provided regular expression will be shown in the dropdown.

```csharp
[SerializeField, TypeDropdown(".*Controller$")]
private string controllerType;
```

### Type filtering by base Type

All types that inherit from one of the provided base types will be shown in the dropdown.

```csharp
[SerializeField, TypeDropdown(typeof(FireWeapon), typeof(IceWeapon))]
private IWeapon effectWeapon;
```

## Installation

### Via Package Manager (Git URL)

1. Open **Window → Package Manager**.
2. Click **+** → **Add package from git URL...**.
3. Paste the repository URL and confirm.

### Via `manifest.json`

Add to `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.longbombus.typedropdown": "https://github.com/longbombus/TypeDropdown.git"
  }
}
```
