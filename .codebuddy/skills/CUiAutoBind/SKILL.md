---
name: cui-autobind
description: This skill should be used when the user needs to work with the CUiAutoBind Unity Package, including creating UI bindings, configuring the AutoBind system, generating code, or setting up the tool in a Unity project. It provides specialized knowledge for working with Unity's UI binding workflow, managing ScriptableObject configurations, and understanding the code generation system.
---

# CUiAutoBind Skill

CUiAutoBind is a Unity UI automatic binding system designed to accelerate UI development by automating the generation of UI component binding code. This skill provides specialized knowledge for working with the CUiAutoBind package.

## Purpose

Enable efficient UI development in Unity by:
- Automatically generating UI binding code
- Providing flexible configuration through ScriptableObject
- Supporting incremental code updates that preserve manual edits
- Offering comprehensive editor tools for UI management

## When to Use This Skill

Use this skill when:
- Setting up CUiAutoBind in a Unity project
- Creating or modifying UI bindings with the AutoBind component
- Configuring generation parameters (namespace, base class, interfaces)
- Configuring suffix naming rules for automatic binding
- Generating or updating binding code
- Working with nested UI structures and hierarchical code generation
- Troubleshooting issues with code generation or binding
- Extending the system with custom components

## Core Components

### AutoBind Component
The AutoBind component marks GameObjects for automatic code generation. Key features:
- Stores binding data linking GameObject components to field names
- Supports all Unity built-in components and user-defined components
- Provides inspector interface for manual binding configuration

### AutoBindData
Binding data structure containing:
- `component`: The Component to bind
- `fieldName`: The generated field name
- `generateField`: Boolean flag to control field generation

### SuffixConfig
Naming convention rule for automatic binding:
- `suffix`: Suffix pattern (e.g., "_btn", "_txt")
- `componentType`: Component type selector with full type path

### BindConfig (ScriptableObject)
Configuration asset managing code generation parameters:
- `namespaceName`: Namespace for generated code (default: "UI")
- `basePath`: Base path for file generation (default: "Scripts/UI/Auto/")
- `baseClass`: Base class for generated classes (default: "MonoBehaviour")
- `interfaces`: Array of interfaces to implement
- `additionalNamespaces`: Additional using statements to include in generated code
- `suffixConfigs`: Array of SuffixConfig for naming convention auto-binding

## Workflow

### Setting Up CUiAutoBind

**Installation Methods:**

1. **Unity Package Installation (Recommended)**:
   - Copy CUiAutoBind folder to project's `Packages` directory
   - Unity will automatically load it
   - Or add to `Packages/manifest.json`:
     ```json
     "com.cframework.cuibind": "file:../Packages/CUiAutoBind"
     ```

2. **Package Manager**:
   - Open Unity Package Manager
   - Click "+" → "Add package from disk"
   - Select CUiAutoBind folder

3. **Traditional Installation** (Not recommended):
   - Copy to `Assets` directory

**Initial Setup:**
- Open editor window via `Tools/CUIBind/打开窗口`
- Create default config via the window
- Config location: `Assets/CUIBind/Resources/AutoBindConfig.asset`

### Creating UI Bindings

**Three Binding Modes:**

**1. Manual Drag Binding**
- Add AutoBind component to GameObject
- Click "添加新绑定" to manually add bindings
- Select components and specify field names
- Suitable for precise control over specific components

**2. Suffix Auto Binding (Recommended)**
- Name GameObjects with suffixes following conventions:
  - `_btn` → Button
  - `_txt` → Text
  - `_img` → Image
  - `_tgl` → Toggle
  - `_slr` → Slider
  - `_inp` → InputField
- Click "按命名约定自动绑定" to auto-bind all children
- Extremely fast for large UI structures
- Auto-converts suffix to camelCase: `Start_btn` → `start`

**3. Mixed Binding**
- Combines manual and suffix auto binding
- First use suffix auto-binding for most components
- Then manually add special components or adjust specific bindings
- Flexible approach for complex UI structures

**Auto-Binding from GameObject:**
- Click "从当前 GameObject 添加组件" to scan all components
- Useful when you want to bind multiple components at once

### Generating Code

**Single GameObject**
- In AutoBind component Inspector, click "生成绑定代码"

**Batch Generation**
- Open the AutoBind window (`Tools/CUIBind/打开窗口`)
- Review all AutoBind components in the scene
- Click individual "生成代码" buttons or "全部生成" for batch processing

**Batch Auto-Binding**
- In the main window, click "批量按命名约定自动绑定"
- Automatically binds all UI objects in the scene using suffix rules
- Fastest way to set up multiple UI panels

### Customizing Generated Code

**Dual-File Generation (Default & Recommended)**

CUiAutoBind generates two files using partial class:

**1. Auto-Generated File (GameObjectName.Auto.cs)**
- Automatically generated - DO NOT EDIT
- Contains serialized field declarations
- Fully overwritten on regeneration

Example:
```csharp
// Auto-generated by CUIBind
// DO NOT EDIT MANUALLY

using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public partial class MainMenuUI : MonoBehaviour
    {
        [SerializeField]
        private Button startButton;

        [SerializeField]
        private Text titleText;
    }
}
```

**2. Manual File (GameObjectName.cs)**
- Created on first generation, can be freely edited
- Contains all custom logic and methods
- Never overwritten on regeneration

Example:
```csharp
// Manual file - free to edit
// Auto-file regeneration won't affect this

using UnityEngine;

namespace UI
{
    public partial class MainMenuUI : MonoBehaviour
    {
        private void Start()
        {
            startButton.onClick.AddListener(OnStartClick);
        }

        private void OnStartClick()
        {
            Debug.Log("Start button clicked!");
        }
    }
}
```

**Editor-Time Auto-Assignment:**
- Fields use `[SerializeField] private` modifier
- Components automatically assigned in editor after code generation
- No runtime GetComponent() calls needed
- Improves performance at runtime
- Sub-object references also auto-assigned

### Incremental Updates

**Dual-File Mode (Default):**
When UI structure changes:
1. Modify AutoBind component bindings (add/remove/edit)
2. Click "生成绑定代码" to regenerate
3. System completely overwrites `GameObjectName.Auto.cs`
4. `GameObjectName.cs` remains completely untouched
5. Editor automatically re-assigns new bindings to fields

**Advantages of Dual-File Mode:**
- ✅ Zero risk of losing manual code
- ✅ Clear separation of concerns
- ✅ No region markers needed
- ✅ Faster regeneration

## Configuration Examples

### Basic Configuration
```csharp
Namespace: UI
Base Path: Scripts/UI/Auto/
Base Class: MonoBehaviour
Interfaces: (empty)
```

Generated class:
```csharp
namespace UI {
    public partial class MainMenuUI : MonoBehaviour {
        // Auto-generated fields...
    }
}
```

### With Base Class and Interface
```csharp
Namespace: GameUI
Base Path: Scripts/Game/UI/
Base Class: BaseUIController
Interfaces: ["IUIPanel", "IUpdateable"]
```

Generated class:
```csharp
namespace GameUI {
    public partial class MainMenuUI : BaseUIController, IUIPanel, IUpdateable {
        // Auto-generated fields...
    }
}
```

### Suffix Configuration for Auto-Binding

Suffix configs define naming rules for automatic binding:

```csharp
Suffix Configs:
[
    {
        "suffix": "_btn",
        "componentType": "Button",
        "namespaceName": "UnityEngine.UI"
    },
    {
        "suffix": "_txt",
        "componentType": "Text",
        "namespaceName": "UnityEngine.UI"
    },
    {
        "suffix": "_custom",
        "componentType": "MyCustomComponent",
        "namespaceName": "MyNamespace"
    }
]
```

**Usage:**
- Name GameObject: `Start_btn`
- Auto-bind generates: `private Button start;`
- Supports both Unity built-in and custom components

**Default Suffixes:**
- `_btn` → Button
- `_txt` → Text
- `_img` → Image
- `_tgl` → Toggle
- `_slr` → Slider
- `_inp` → InputField
- `_scr` → ScrollRect
- `_grid` → GridLayoutGroup

## Configuration Parameters

### BindConfig Properties

| Parameter | Description | Default |
|-----------|-------------|---------|
| `namespaceName` | Namespace for generated classes | `"UI"` |
| `basePath` | Base path for file generation (relative to Assets/) | `"Scripts/UI/Auto/"` |
| `baseClass` | Base class for generated UI classes | `"MonoBehaviour"` |
| `interfaces` | Array of interfaces to implement | `[]` (empty) |
| `additionalNamespaces` | Additional namespaces to include in generated code | `[]` (empty) |
| `suffixConfigs` | Array of suffix naming rules for auto-binding | 8 default rules |

### AutoBind Component Options

Each AutoBind component can be configured with:

| Option | Description |
|--------|-------------|
| `bindings` | List of AutoBindData entries (component → field name mappings) |
| `customClassName` | Override auto-generated class name (empty = use GameObject name) |

### AutoBindData Properties

Each binding entry contains:
- `component`: The Component to bind (or AutoBind component for nested UI)
- `fieldName`: The generated field name
- `generateField`: Whether to generate this field (can be toggled to exclude)

## Code Generation Details

### File Structure (Dual-File Mode)

Generated files are placed at: `{basePath}/{GameObjectName}/`

Example: For GameObject "MainMenu" with default config:
- Auto file: `Scripts/UI/Auto/MainMenu/MainMenu.Auto.cs`
- Manual file: `Scripts/UI/Auto/MainMenu/MainMenu.cs`

**Nested UI Structure:**
```
MainMenu (AutoBind)
├── Start_btn
└── SettingsPanel (AutoBind)
    ├── Close_btn
    └── Volume_slr
```

Generated files:
```
Scripts/UI/Auto/MainMenu/
├── MainMenu.Auto.cs           (Root object)
├── MainMenu.cs
└── SettingsPanel/
    ├── SettingsPanel.Auto.cs  (Child object)
    └── SettingsPanel.cs
```

### Field Naming

**Auto-Generated from GameObject:**
- GameObject names convert to camelCase
- Suffix removed during conversion
- Examples:
  - `Start_btn` → `start`
  - `PlayerName_txt` → `playerName`
  - `Background_img` → `background`

**Component Type Conversion:**
- Not directly used in field names (uses GameObject name)
- Used for namespace import generation
- Full type name stored in binding data for editor

### Component Type Support
All Component-derived types are supported:
- Unity UI: Button, Text, Image, Toggle, Slider, ScrollRect, etc.
- TextMeshPro: TMP_Text, TMP_Dropdown, TMP_InputField, etc.
- User-defined: Any custom Component class

## Troubleshooting

### Generated Code Not Found
- Ensure the generated class name matches GameObject name
- Check that files exist at: `{basePath}/{GameObjectName}/`
- Verify namespace settings match usage location

### Manual Code Disappears
- **Dual-File Mode**: Should not happen - manual file is never overwritten
- Verify both files exist: `GameObjectName.Auto.cs` and `GameObjectName.cs`

### Compilation Errors
- Check for duplicate field names in bindings
- Ensure component types are valid and accessible
- Verify namespace and using statements are correct
- Check that partial class declarations match

### Component Not Detected in Auto-Binding
- Ensure GameObject names match suffix patterns (e.g., `Button_btn`)
- Verify the component actually exists on the GameObject
- Check suffix config for correct component types and namespaces

### Fields Not Assigned in Editor
- Wait for script compilation after generation
- Click "绑定组件" button to manually trigger assignment
- Check for compilation errors preventing auto-assignment

### Nested UI Issues
- Ensure child panels have their own AutoBind component
- Verify child AutoBind is explicitly bound in parent's bindings list
- Check that subdirectories were created correctly

## Best Practices

1. **Use Suffix Auto-Binding**: Fastest way to set up new UI panels
2. **Consistent Naming**: Follow suffix conventions across your project
3. **Dual-File Mode**: Leverage partial classes to separate generated and manual code
4. **Descriptive GameObject Names**: Use clear names for auto-generated fields
5. **Nested Structure**: Add AutoBind to child panels to avoid duplicate bindings
6. **Editor-Time Assignment**: Rely on auto-assignment for better runtime performance
7. **Regular Regeneration**: Regenerate after UI structure changes
8. **Version Control**: Include both Auto and Manual files in version control
9. **Custom Suffix Rules**: Add project-specific components to suffix configs

## Reference Files

For detailed implementation information, consult:
- `Runtime/AutoBind.cs` - Core component implementation
- `Runtime/BindConfig.cs` - Configuration class details
- `Editor/CodeGenerator.cs` - Code generation logic
- `Editor/AutoBindWindow.cs` - Editor window implementation
- `README.md` - Complete user documentation

## Notes

- The system uses `ExecuteAlways` on AutoBind for editor-time operation
- Generated fields use `[SerializeField] private` - visible in Inspector, not accessible to other scripts
- Dual-file mode completely separates generated and manual code
- Editor-time auto-assignment improves runtime performance (no GetComponent calls)
- Config files are ScriptableObject assets for easy version control
- Supports recursive code generation for nested UI structures
- Auto-binding skips child objects with their own AutoBind components to avoid duplicates
