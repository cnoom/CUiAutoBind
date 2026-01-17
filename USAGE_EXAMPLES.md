# 命名约定自动绑定使用示例

## 示例1: 基础使用

### 场景结构
```
MainMenu (AutoBind组件)
├── Start_btn
├── Title_txt
├── Bg_img
└── Exit_btn
```

### 步骤

1. **在根对象上添加 AutoBind 组件**
   - 选择 `MainMenu` 对象
   - 点击 `Add Component`
   - 搜索并添加 `AutoBind`

2. **点击"按命名约定自动绑定"按钮**
   - 在 AutoBind Inspector 中找到"命名约定自动绑定"部分
   - 点击"按命名约定自动绑定"按钮
   - 系统会自动扫描并绑定所有子对象

3. **查看绑定结果**
   ```
   ✓ 新增绑定: 4
   ○ 已存在（跳过）: 0
   ```

4. **点击"生成绑定代码"**
   - 生成的字段：
     ```csharp
     private Button start;
     private Text title;
     private Image bg;
     private Button exit;
     ```

---

## 示例2: 嵌套UI结构

### 场景结构
```
MainMenu (AutoBind组件)
├── Start_btn
├── Title_txt
└── SettingsPanel (AutoBind组件)  ← 有自己的AutoBind
    ├── Close_btn
    ├── Volume_slr
    └── Music_txt
```

### 步骤

1. **在根对象 MainMenu 上添加 AutoBind 组件**
   - 点击"按命名约定自动绑定"
   - ✅ 绑定: `Start_btn`, `Title_txt`
   - ⏭️  跳过: `SettingsPanel`（因为它有自己的AutoBind组件）

2. **在子对象 SettingsPanel 上添加 AutoBind 组件**
   - 点击"按命名约定自动绑定"
   - ✅ 绑定: `Close_btn`, `Volume_slr`, `Music_txt`

3. **生成的代码**
   ```csharp
   // MainMenu.cs
   private Button start;
   private Text title;

   // SettingsPanel.cs
   private Button close;
   private Slider volume;
   private Text music;
   ```

**关键点**：SettingsPanel 不会被 MainMenu 绑定，因为它有自己的 AutoBind 组件。

---

## 示例3: 使用排除前缀

### 场景结构
```
MainMenu (AutoBind组件)
├── Start_btn
├── Title_txt
├── _Background (Image)  ← 不想绑定
└── TMP_Title (Text)     ← 不想绑定
```

### 步骤

1. **配置排除前缀**
   - 在 AutoBind 组件中设置 `Excluded Prefixes` 为: `_Background, TMP_`

2. **点击"按命名约定自动绑定"**
   - ✅ 绑定: `Start_btn`, `Title_txt`
   - ⏭️  跳过: `_Background`, `TMP_Title`（匹配排除前缀）

3. **生成的代码**
   ```csharp
   private Button start;
   private Text title;
   // _Background 和 TMP_Title 不会被绑定
   ```

---

## 示例4: 自定义命名规则

### 配置自定义规则

打开 `Tools/CUIBind/打开窗口`，在配置中添加：

```csharp
// 添加自定义后缀规则（注意使用下划线前缀）
{
    "suffix": "_slider",
    "componentType": "Slider",
    "namespaceName": "UnityEngine.UI"
}
{
    "suffix": "_input",
    "componentType": "InputField",
    "namespaceName": "UnityEngine.UI"
}
```

### 场景结构
```
SettingsPanel (AutoBind组件)
├── Volume_slider
├── Music_slider
└── PlayerName_input
```

### 生成的代码
```csharp
private Slider volume;
private Slider music;
private InputField playerName;
```

---

## 示例5: 复杂嵌套UI

### 场景结构
```
MainMenu (AutoBind组件)
├── Start_btn
├── Settings_btn
├── Title_txt
├── Bg_img
└── SettingsPanel (AutoBind组件)
    ├── Close_btn
    ├── Volume_slr
    ├── Music_slr
    ├── Volume_txt
    ├── Music_txt
    └── AudioPanel (AutoBind组件)
        ├── Mute_btn
        └── Status_txt
```

### 绑定流程

**第1步: 在 MainMenu 上自动绑定**
```
✓ 新增绑定: 4
  - Start_btn → start
  - Settings_btn → settings
  - Title_txt → title
  - Bg_img → bg
⏭️  跳过: SettingsPanel (有自己的AutoBind)
```

**第2步: 在 SettingsPanel 上自动绑定**
```
✓ 新增绑定: 5
  - Close_btn → close
  - Volume_slr → volume
  - Music_slr → music
  - Volume_txt → volumeText
  - Music_txt → musicText
⏭️  跳过: AudioPanel (有自己的AutoBind)
```

**第3步: 在 AudioPanel 上自动绑定**
```
✓ 新增绑定: 2
  - Mute_btn → mute
  - Status_txt → status
```

### 生成的代码
```csharp
// MainMenu.cs
private Button start;
private Button settings;
private Text title;
private Image bg;

// SettingsPanel.cs
private Button close;
private Slider volume;
private Slider music;
private Text volumeText;
private Text musicText;

// AudioPanel.cs
private Button mute;
private Text status;
```

---

## 示例6: 批量操作

### 场景中多个UI Panel
```
MainMenu (AutoBind组件)
SettingsPanel (AutoBind组件)
HelpPanel (AutoBind组件)
InventoryPanel (AutoBind组件)
```

### 批量绑定

1. **打开主窗口**
   - 菜单: `Tools/CUIBind/打开窗口`

2. **点击"批量按命名约定自动绑定"**
   - 系统会遍历场景中所有 AutoBind 组件
   - 对每个组件执行自动绑定

3. **查看结果**
   ```
   批量自动绑定完成！

   总计:
     ✓ 新增绑定: 24
     ○ 已存在（跳过）: 0
     ✗ 未找到组件: 2
     处理对象数: 4
   ```

4. **点击"全部生成"按钮**
   - 批量生成所有绑定代码

---

## 命名转换规则

### 规则说明

系统自动将对象名称转换为驼峰命名字段名：

1. **移除后缀**
   - `Start_btn` → `Start`
   - `PlayerName_txt` → `PlayerName`

2. **首字母小写**
   - `Start` → `start`
   - `PlayerName` → `playerName`

### 转换示例

| 对象名称 | 后缀 | 字段名 |
|---------|------|--------|
| `Start_btn` | _btn | `start` |
| `Settings_btn` | _btn | `settings` |
| `Title_txt` | _txt | `title` |
| `PlayerName_txt` | _txt | `playerName` |
| `Bg_img` | _img | `bg` |
| `Icon_img` | _img | `icon` |
| `Volume_slr` | _slr | `volume` |
| `Mute_tgl` | _tgl | `mute` |
| `Name_inp` | _inp | `name` |

---

## 最佳实践

### 1. 命名规范

**推荐的命名方式（后缀模式）：**
```
✅ Start_btn          (按钮)
✅ Title_txt          (文本)
✅ Icon_img           (图片)
✅ Volume_slr         (滑块)
✅ Enable_tgl         (开关)
✅ Search_inp         (输入框)
✅ Content_scr        (滚动区域)
✅ Items_grid         (网格布局)
```

**不推荐的命名方式：**
```
❌ Button_Start       (类型前缀，不推荐)
❌ Text_Title         (类型前缀，不推荐)
❌ startButton        (驼峰命名，无法自动识别)
```

### 2. 排除规则

使用排除前缀来跳过不需要绑定的对象：
```
排除前缀: _, TMP, Temp, Background

示例：
_Background        ← 被排除
TMP_Title          ← 被排除
Temp_Object        ← 被排除
Background_Image    ← 被排除
```

### 3. 嵌套结构

对于复杂的UI结构，建议分层管理：
```
RootUI (AutoBind)
├── HeaderPanel (AutoBind)   ← 独立管理
│   ├── Back_btn
│   └── Title_txt
├── ContentPanel (AutoBind) ← 独立管理
│   ├── Volume_slr
│   └── Enable_tgl
└── FooterPanel (AutoBind)  ← 独立管理
    └── Confirm_btn
```

**优势：**
- 每个Panel独立管理，职责清晰
- 避免字段过多，便于维护
- 支持Panel复用

### 4. 自定义组件

为自定义组件添加命名规则：
```csharp
// AutoBindConfig
{
    "suffix": "_progress",
    "componentType": "ProgressBar",
    "namespaceName": "Game.UI"
}

// 场景中
LoadingPanel (AutoBind)
└── Loading_progress  (ProgressBar组件)

// 生成的字段
private ProgressBar loading;
```

---

## 常见问题

### Q1: 点击"按命名约定自动绑定"没有反应？

**A:** 请检查：
1. 配置文件中是否已添加命名规则（注意使用下划线前缀，如 `_btn`）
2. 子对象名称是否匹配后缀规则（使用后缀模式，如 `Start_btn`）
3. 子对象上是否有对应的组件

### Q2: 为什么有些子对象没有被绑定？

**A:** 可能的原因：
1. 子对象名称不匹配任何后缀规则（检查是否使用下划线前缀，如 `_btn`）
2. 子对象有匹配排除前缀
3. 子对象有自己的AutoBind组件
4. 子对象上没有对应的组件

### Q3: 嵌套时父对象会重复绑定子对象的组件吗？

**A:** 不会！系统会自动跳过有自己AutoBind组件的子对象，避免重复绑定。

### Q4: 如何添加自定义组件的命名规则？

**A:**
1. 打开 `Tools/CUIBind/打开窗口`
2. 在配置的 `Suffix Configs` 数组中添加新规则
3. 填写：Suffix（后缀，如 `_btn`）、Component Type（组件类型）、Namespace（命名空间）
4. 保存配置

### Q5: 批量绑定时如何查看每个对象的绑定结果？

**A:** 批量操作会显示汇总结果。如需查看详细信息，可以在每个AutoBind组件上单独点击"按命名约定自动绑定"。

### Q6: 生成的字段名不符合预期？

**A:** 检查命名转换规则：
- 系统会自动移除后缀并首字母小写
- 例如：`StartGame_btn` → `startGame`
- 可以手动在AutoBind组件中调整字段名

---

## 高级技巧

### 1. 结合使用多种绑定方式

```csharp
// 关键组件手动绑定（精确控制）
[SerializeField] private Button criticalButton;

// 常规组件按命名约定自动绑定
// Start_btn, Title_txt, Bg_img 等自动识别
```

### 2. 使用排除前缀管理装饰性对象

```
排除前缀: _, Decor, Bg

场景：
MainMenu (AutoBind)
├── Start_btn           ← 绑定
├── Title_txt           ← 绑定
├── _DecorativeIcon     ← 排除（装饰性）
└── DecorativeBg        ← 排除（装饰性）
```

### 3. 自定义后缀规则提高效率

```csharp
// 为项目特有的组件配置后缀（使用下划线前缀）
{
    "suffix": "_stat",
    "componentType": "StatBar",
    "namespaceName": "Game.UI"
}
{
    "suffix": "_card",
    "componentType": "CardView",
    "namespaceName": "Game.UI"
}
```

### 4. 批量工作流

```
1. 批量按命名约定自动绑定（所有UI Panel）
2. 检查绑定结果，调整遗漏的组件
3. 批量生成所有代码
4. 批量重新绑定（确保所有引用正确）
```

---

## 总结

命名约定自动绑定功能通过简单的命名规范，实现了高效的UI组件绑定。合理使用命名规则、排除前缀和嵌套结构，可以大幅提升UI开发效率。

**核心优势：**
- ⚡ 快速：一键绑定所有子对象
- 🎯 智能：自动跳过已绑定的组件
- 🔧 灵活：支持自定义规则和排除前缀
- 🛡️ 安全：避免重复绑定和引用冲突
