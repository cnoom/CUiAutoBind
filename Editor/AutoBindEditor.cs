using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace CUiAutoBind
{
    /// <summary>
    ///     AutoBind 组件的自定义编辑器
    /// </summary>
    [CustomEditor(typeof(UiAutoBind))]
    public class AutoBindEditor : Editor
    {
        private SerializedProperty bindingsProperty;
        private SerializedProperty bindModeProperty;

        /// <summary>
        ///     缓存的配置，用于 AutoAssignComponents
        /// </summary>
        private UiBindConfig config;
        private SerializedProperty customClassNameProperty;
        private SerializedProperty showBindingListProperty;

        private void OnEnable()
        {
            bindingsProperty = serializedObject.FindProperty("bindings");
            bindModeProperty = serializedObject.FindProperty("bindMode");
            customClassNameProperty = serializedObject.FindProperty("customClassName");
            showBindingListProperty = serializedObject.FindProperty("showBindingList");

            // 预加载配置
            config = ConfigManager.LoadConfig();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            UiAutoBind uiAutoBind = (UiAutoBind)target;

            // 显示绑定模式选择
            EditorGUILayout.LabelField("绑定模式", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(bindModeProperty, new GUIContent("绑定方式", "选择绑定模式"));
            EditorGUILayout.HelpBox(uiAutoBind.GetBindModeDescription(), MessageType.Info);

            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(customClassNameProperty, new GUIContent("自定义类名", "留空则使用 GameObject 名称"));

            EditorGUILayout.Space();

            // 绑定列表标题（使用 Foldout）
            showBindingListProperty.boolValue = EditorGUILayout.Foldout(showBindingListProperty.boolValue, "UI 绑定列表 (" + bindingsProperty.arraySize + ")", true, EditorStyles.boldLabel);

            // 显示绑定列表
            if(showBindingListProperty.boolValue)
            {
                EditorGUI.indentLevel++;
                for(var i = 0; i < bindingsProperty.arraySize; i++)
                {
                    SerializedProperty bindingProperty = bindingsProperty.GetArrayElementAtIndex(i);

                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                    // 使用 AutoBindDataDrawer 绘制绑定项
                    EditorGUILayout.PropertyField(bindingProperty, true);

                    // 移除按钮
                    if(GUILayout.Button("移除绑定", GUILayout.Height(25)))
                    {
                        Undo.RecordObject(uiAutoBind, "Remove Binding");
                        uiAutoBind.RemoveBinding(i < uiAutoBind.bindings.Count ? uiAutoBind.bindings[i] : null);
                        EditorUtility.SetDirty(uiAutoBind);
                        serializedObject.Update();
                        i--;
                        EditorGUI.indentLevel++;
                        EditorGUILayout.EndVertical();
                        EditorGUI.indentLevel--;
                        continue;
                    }

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space();
                }
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();

            // 根据绑定模式显示不同的操作按钮
            switch(uiAutoBind.bindMode)
            {
                case BindMode.Manual:
                    DrawManualBindingButtons(uiAutoBind);
                    break;
                case BindMode.AutoSuffix:
                    DrawAutoSuffixBindingButtons(uiAutoBind);
                    break;
                case BindMode.Hybrid:
                    DrawHybridBindingButtons(uiAutoBind);
                    break;
            }

            EditorGUILayout.Space();

            // 生成代码按钮
            if(GUILayout.Button("生成绑定代码", GUILayout.Height(35)))
            {
                GenerateCode(uiAutoBind);
            }

            EditorGUILayout.Space();

            // 额外功能按钮区域
            EditorGUILayout.BeginHorizontal();

            if(GUILayout.Button("绑定组件", GUILayout.Height(25)))
            {
                RebindComponents();
            }

            if(GUILayout.Button("验证绑定", GUILayout.Height(25)))
            {
                ValidateBinding();
            }

            EditorGUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        ///     从当前 GameObject 自动添加组件
        /// </summary>
        private void AutoAddComponents(UiAutoBind uiAutoBind)
        {
            Component[] components = uiAutoBind.GetComponents<Component>();

            foreach (Component component in components)
            {
                // 跳过 AutoBind 自身和 Transform
                if(component is UiAutoBind || component is Transform)
                    continue;

                // 检查是否已经存在
                bool exists = uiAutoBind.bindings.Exists(b => b.component == component);
                if(!exists)
                {
                    // 自动生成字段名：驼峰命名
                    string fieldName = component.GetType().Name;
                    fieldName = char.ToLower(fieldName[0]) + fieldName.Substring(1);

                    uiAutoBind.AddBinding(component, fieldName);
                }
            }
        }

        /// <summary>
        ///     按命名约定自动绑定
        /// </summary>
        private void AutoBindByNamingConvention(UiAutoBind uiAutoBind)
        {
            // 加载配置
            UiBindConfig config = ConfigManager.LoadConfig();
            if(config == null || config.suffixConfigs == null || config.suffixConfigs.Length == 0)
            {
                EditorUtility.DisplayDialog("错误", "请先在配置文件中添加命名规则", "确定");
                return;
            }

            // 统计信息
            var addedCount = 0;
            var skippedCount = 0;
            var notFoundCount = 0;

            // 递归遍历所有子对象
            AutoBindByNamingConventionRecursive(uiAutoBind.transform, uiAutoBind, config, ref addedCount, ref skippedCount, ref notFoundCount);

            // 显示结果
            StringBuilder message = new StringBuilder();
            message.AppendLine("自动绑定完成！");
            message.AppendLine($"✓ 新增绑定: {addedCount}");
            message.AppendLine($"○ 已存在（跳过）: {skippedCount}");
            if(notFoundCount > 0)
            {
                message.AppendLine($"✗ 未找到组件: {notFoundCount}");
            }

            EditorUtility.DisplayDialog("完成", message.ToString(), "确定");
        }

        /// <summary>
        ///     递归按命名约定自动绑定
        /// </summary>
        private void AutoBindByNamingConventionRecursive(Transform current, UiAutoBind parentUiAutoBind, UiBindConfig config, ref int addedCount, ref int skippedCount, ref int notFoundCount)
        {
            AutoBindUtility.AutoBindByNamingConventionRecursive(current, parentUiAutoBind, config, ref addedCount, ref skippedCount, ref notFoundCount);
        }

        /// <summary>
        ///     生成代码
        /// </summary>
        private void GenerateCode(UiAutoBind uiAutoBind)
        {
            // 加载配置
            config = ConfigManager.LoadOrCreateConfig();
            if(config == null)
            {
                EditorUtility.DisplayDialog("错误", "无法加载或创建配置文件", "确定");
                return;
            }

            // 创建代码生成器
            CodeGenerator generator = new CodeGenerator(config);

            // 获取有效的绑定
            List<AutoBindData> validBindings = uiAutoBind.GetValidBindings();
            if(validBindings.Count == 0)
            {
                EditorUtility.DisplayDialog("提示", "没有有效的绑定数据", "确定");
                return;
            }

            // 生成代码
            try
            {
                generator.GenerateCode(uiAutoBind.gameObject, validBindings);

                // 刷新资源以触发编译
                AssetDatabase.Refresh();

                // 编译完成后自动绑定（避免编译未完成导致绑定失败）
                AutoBindBindingScheduler.Enqueue(uiAutoBind);

                StringBuilder message = new StringBuilder();

                message.AppendLine("✓ 代码生成成功！");
                message.AppendLine();
                message.AppendLine($"自动文件: {config.GetAutoGeneratedFilePath(uiAutoBind.gameObject.name)}");
                message.AppendLine($"手动文件: {config.GetManualFilePath(uiAutoBind.gameObject.name)}");
                message.AppendLine();
                message.AppendLine("编译完成后将自动添加脚本并绑定组件");
                message.AppendLine("如未绑定，可点击『绑定组件』重试");

                EditorUtility.DisplayDialog("完成", message.ToString(), "确定");
            }
            catch (Exception e)
            {
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("错误", "代码生成失败: " + e.Message, "确定");
                Debug.LogError(e);
            }
        }

        /// <summary>
        ///     重新绑定组件（不重新生成代码）
        /// </summary>
        private void RebindComponents()
        {
            UiAutoBind uiAutoBind = (UiAutoBind)target;

            // 加载配置
            config = ConfigManager.LoadOrCreateConfig();
            if(config == null)
            {
                EditorUtility.DisplayDialog("错误", "无法加载配置文件", "确定");
                return;
            }

            // 使用 CodeBinder 重新绑定
            CodeBinder codeBinder = new CodeBinder(config);
            bool success = codeBinder.BindComponents(uiAutoBind);

            if(success)
            {
                EditorUtility.DisplayDialog("成功", "组件重新绑定成功！", "确定");
            }
            else
            {
                EditorUtility.DisplayDialog("完成", "组件重新绑定完成，但可能存在问题，请检查控制台日志。", "确定");
            }
        }

        /// <summary>
        ///     验证绑定状态
        /// </summary>
        private void ValidateBinding()
        {
            UiAutoBind uiAutoBind = (UiAutoBind)target;

            // 加载配置
            config = ConfigManager.LoadOrCreateConfig();
            if(config == null)
            {
                EditorUtility.DisplayDialog("错误", "无法加载配置文件", "确定");
                return;
            }

            // 使用 CodeBinder 验证
            CodeBinder codeBinder = new CodeBinder(config);
            BindingValidation validation = codeBinder.ValidateBinding(uiAutoBind);
            string report = codeBinder.GenerateReport(validation);

            Debug.Log(report);
            EditorUtility.DisplayDialog("绑定验证", report, "确定");
        }

        /// <summary>
        ///     绘制手动拖拽绑定模式的按钮
        /// </summary>
        private void DrawManualBindingButtons(UiAutoBind uiAutoBind)
        {
            EditorGUILayout.LabelField("手动拖拽绑定", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("完全手动添加绑定，适合精确控制的场景。", MessageType.None);

            // 添加新绑定按钮
            if(GUILayout.Button("添加新绑定", GUILayout.Height(30)))
            {
                Undo.RecordObject(uiAutoBind, "Add New Binding");
                uiAutoBind.AddBinding(null, "NewBinding");
                EditorUtility.SetDirty(uiAutoBind);
                serializedObject.Update();
            }

            // 从 GameObject 自动添加组件按钮
            if(GUILayout.Button("从当前 GameObject 添加组件", GUILayout.Height(30)))
            {
                Undo.RecordObject(uiAutoBind, "Auto Add Components");
                AutoAddComponents(uiAutoBind);
                EditorUtility.SetDirty(uiAutoBind);
                serializedObject.Update();
            }
        }

        /// <summary>
        ///     绘制后缀自动绑定模式的按钮
        /// </summary>
        private void DrawAutoSuffixBindingButtons(UiAutoBind uiAutoBind)
        {
            EditorGUILayout.LabelField("后缀自动绑定", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("根据命名后缀自动扫描并绑定，适合大量组件的场景。", MessageType.None);

            // 按命名约定自动绑定按钮
            if(GUILayout.Button("按命名约定自动绑定", GUILayout.Height(30)))
            {
                Undo.RecordObject(uiAutoBind, "Auto Bind By Naming Convention");
                AutoBindByNamingConvention(uiAutoBind);
                EditorUtility.SetDirty(uiAutoBind);
                serializedObject.Update();
            }

            EditorGUILayout.HelpBox("提示：命名约定会递归扫描所有子对象，根据后缀规则自动绑定。可在配置文件中自定义后缀规则。", MessageType.Info);
        }

        /// <summary>
        ///     绘制混合绑定模式的按钮
        /// </summary>
        private void DrawHybridBindingButtons(UiAutoBind uiAutoBind)
        {
            EditorGUILayout.LabelField("混合绑定", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("同时支持手动和后缀自动绑定，适合复杂场景。", MessageType.None);

            // 添加新绑定按钮
            if(GUILayout.Button("添加新绑定", GUILayout.Height(30)))
            {
                Undo.RecordObject(uiAutoBind, "Add New Binding");
                uiAutoBind.AddBinding(null, "NewBinding");
                EditorUtility.SetDirty(uiAutoBind);
                serializedObject.Update();
            }

            // 从 GameObject 自动添加组件按钮
            if(GUILayout.Button("从当前 GameObject 添加组件", GUILayout.Height(30)))
            {
                Undo.RecordObject(uiAutoBind, "Auto Add Components");
                AutoAddComponents(uiAutoBind);
                EditorUtility.SetDirty(uiAutoBind);
                serializedObject.Update();
            }

            // 按命名约定自动绑定按钮
            if(GUILayout.Button("按命名约定自动绑定", GUILayout.Height(30)))
            {
                Undo.RecordObject(uiAutoBind, "Auto Bind By Naming Convention");
                AutoBindByNamingConvention(uiAutoBind);
                EditorUtility.SetDirty(uiAutoBind);
                serializedObject.Update();
            }

            EditorGUILayout.HelpBox("提示：可以同时使用手动添加和后缀自动绑定。后缀自动绑定会自动跳过已绑定的组件。", MessageType.Info);
        }
    }

    [InitializeOnLoad]
    internal static class AutoBindBindingScheduler
    {
        private const string PendingKey = "CUiAutoBind.PendingAutoBindIds";

        static AutoBindBindingScheduler()
        {
            AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
            EditorApplication.delayCall += ProcessPending;
        }

        [InitializeOnLoadMethod]
        private static void ProcessPendingOnLoad()
        {
            EditorApplication.delayCall += ProcessPending;
        }


        public static void Enqueue(UiAutoBind uiAutoBind)
        {
            if(uiAutoBind == null)
                return;

            var id = GlobalObjectId.GetGlobalObjectIdSlow(uiAutoBind).ToString();
            if(string.IsNullOrEmpty(id))
                return;

            List<string> ids = GetIds();
            if(!ids.Contains(id))
            {
                ids.Add(id);
                SaveIds(ids);
            }

            EditorApplication.delayCall += () => BindWhenReady(uiAutoBind, 0);
        }

        public static void EnqueueAll(UiAutoBind[] autoBinds)
        {
            if(autoBinds == null || autoBinds.Length == 0)
                return;

            List<string> ids = GetIds();
            var changed = false;

            foreach (UiAutoBind autoBind in autoBinds)
            {
                if(autoBind == null)
                    continue;

                var id = GlobalObjectId.GetGlobalObjectIdSlow(autoBind).ToString();
                if(string.IsNullOrEmpty(id))
                    continue;

                if(!ids.Contains(id))
                {
                    ids.Add(id);
                    changed = true;
                }

                EditorApplication.delayCall += () => BindWhenReady(autoBind, 0);
            }

            if(changed)
            {
                SaveIds(ids);
            }
        }

        private static List<string> GetIds()
        {
            string raw = EditorPrefs.GetString(PendingKey, "");
            if(string.IsNullOrEmpty(raw))
                return new List<string>();

            return raw.Split(new[]
            {
                ';'
            }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        private static void SaveIds(List<string> ids)
        {
            EditorPrefs.SetString(PendingKey, string.Join(";", ids));
        }

        private static void OnAfterAssemblyReload()
        {
            ProcessPending();
        }

        private static void ProcessPending()
        {
            List<string> ids = GetIds();
            if(ids.Count == 0)
                return;

            SaveIds(new List<string>());

            foreach (string id in ids)
            {
                if(!GlobalObjectId.TryParse(id, out GlobalObjectId globalId))
                    continue;

                UiAutoBind uiAutoBind = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(globalId) as UiAutoBind;
                if(uiAutoBind != null)
                {
                    BindWhenReady(uiAutoBind, 0);
                }
            }
        }


        private static void BindWhenReady(UiAutoBind uiAutoBind, int attempt)
        {
            if(uiAutoBind == null)
                return;

            if(EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                if(attempt < 100)
                {
                    EditorApplication.delayCall += () => BindWhenReady(uiAutoBind, attempt + 1);
                }
                else
                {
                    Debug.LogWarning($"AutoBind: 编译超时，未能自动绑定 '{uiAutoBind.gameObject.name}'。请手动点击『绑定组件』重试。");
                }
                return;
            }

            UiBindConfig config = ConfigManager.LoadOrCreateConfig();
            if(config == null)
            {
                Debug.LogWarning("AutoBind: 无法加载配置文件，自动绑定已跳过。");
                return;
            }

            CodeBinder codeBinder = new CodeBinder(config);
            bool success = codeBinder.BindComponents(uiAutoBind);
            if(!success)
            {
                Debug.LogWarning($"AutoBind: 自动绑定失败 '{uiAutoBind.gameObject.name}'，请检查控制台日志。");
            }
        }
    }
}