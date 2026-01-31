using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace CUiAutoBind
{
    /// <summary>
    ///     CUIBind 主编辑器窗口
    /// </summary>
    public class AutoBindWindow : EditorWindow
    {
        private static UiBindConfig config;
        private Vector2 scrollPosition;
        private Vector2 suffixScrollPosition;

        private void OnGUI()
        {
            // 加载配置
            if(config == null)
            {
                config = ConfigManager.LoadConfig();
            }

            // 标题
            GUILayout.Label("CUIBind - UI 自动绑定系统", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // 配置部分
            DrawConfigSection();

            EditorGUILayout.Space();

            // 批量生成部分
            DrawBatchGenerateSection();
        }

        [MenuItem("Tools/CUIBind/打开窗口", false, 10)]
        public static void ShowWindow()
        {
            GetWindow<AutoBindWindow>("CUIBind");
        }

        /// <summary>
        ///     绘制配置部分
        /// </summary>
        private void DrawConfigSection()
        {
            GUILayout.Label("配置", EditorStyles.boldLabel);

            if(config != null)
            {
                SerializedObject serializedConfig = new SerializedObject(config);

                EditorGUI.BeginChangeCheck();

                EditorGUILayout.PropertyField(serializedConfig.FindProperty("namespaceName"), new GUIContent("命名空间"));
                EditorGUILayout.PropertyField(serializedConfig.FindProperty("basePath"), new GUIContent("基础路径"));
                EditorGUILayout.PropertyField(serializedConfig.FindProperty("baseClass"), new GUIContent("基类"));
                EditorGUILayout.PropertyField(serializedConfig.FindProperty("interfaces"), new GUIContent("接口"));
                EditorGUILayout.PropertyField(serializedConfig.FindProperty("additionalNamespaces"), new GUIContent("额外命名空间"));

                EditorGUILayout.Space();

                // 绘制后缀配置
                DrawSuffixConfigs(serializedConfig);

                EditorGUILayout.Space();

                if(EditorGUI.EndChangeCheck())
                {
                    serializedConfig.ApplyModifiedProperties();
                    EditorUtility.SetDirty(config);
                    AssetDatabase.SaveAssets();
                }

                EditorGUILayout.Space();

                // 重新生成配置按钮
                if(GUILayout.Button("重新生成默认配置", GUILayout.Height(25)))
                {
                    if(EditorUtility.DisplayDialog("确认", "确定要重新生成配置吗？当前配置将被覆盖。", "确定", "取消"))
                    {
                        config = ConfigManager.RegenerateConfig();
                    }
                }
            }
            else
            {
                EditorGUILayout.HelpBox("未找到配置文件", MessageType.Warning);

                if(GUILayout.Button("创建默认配置", GUILayout.Height(30)))
                {
                    config = ConfigManager.CreateDefaultConfig();
                }
            }
        }

        /// <summary>
        ///     绘制后缀配置部分
        /// </summary>
        private void DrawSuffixConfigs(SerializedObject serializedConfig)
        {
            EditorGUILayout.LabelField("后缀命名规则", EditorStyles.boldLabel);

            SerializedProperty suffixConfigsProp = serializedConfig.FindProperty("suffixConfigs");

            // 显示配置数量
            EditorGUILayout.HelpBox($"当前配置了 {suffixConfigsProp.arraySize} 个后缀规则", MessageType.Info);

            EditorGUILayout.Space();

            // 使用滚动视图显示列表
            suffixScrollPosition = EditorGUILayout.BeginScrollView(suffixScrollPosition, GUILayout.Height(200));

            for(var i = 0; i < suffixConfigsProp.arraySize; i++)
            {
                SerializedProperty elementProp = suffixConfigsProp.GetArrayElementAtIndex(i);

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                // 使用自定义的 SuffixConfigDrawer 绘制每个元素
                EditorGUILayout.PropertyField(elementProp, new GUIContent($"规则 {i + 1}"), true);

                // 删除按钮
                if(GUILayout.Button("删除此规则", GUILayout.Height(20)))
                {
                    if(EditorUtility.DisplayDialog("确认删除", $"确定要删除规则 '{i + 1}' 吗？", "确定", "取消"))
                    {
                        suffixConfigsProp.DeleteArrayElementAtIndex(i);
                        break;
                    }
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(4);
            }

            EditorGUILayout.EndScrollView();

            // 添加新规则按钮
            if(GUILayout.Button("添加后缀规则", GUILayout.Height(25)))
            {
                suffixConfigsProp.arraySize++;
                SerializedProperty newElement = suffixConfigsProp.GetArrayElementAtIndex(suffixConfigsProp.arraySize - 1);
                newElement.FindPropertyRelative("suffix").stringValue = "new";
                newElement.FindPropertyRelative("componentType").FindPropertyRelative("fullTypeName").stringValue = "UnityEngine.UI.Button";
            }
        }

        /// <summary>
        ///     绘制批量生成部分
        /// </summary>
        private void DrawBatchGenerateSection()
        {
            GUILayout.Label("批量生成", EditorStyles.boldLabel);

            if(config == null || !config.IsValid())
            {
                EditorGUILayout.HelpBox("请先配置有效的生成路径", MessageType.Warning);
                return;
            }

            // 扫描场景中的 AutoBind 组件
            UiAutoBind[] autoBinds = FindObjectsOfType<UiAutoBind>();

            if(autoBinds.Length == 0)
            {
                EditorGUILayout.HelpBox("场景中没有 AutoBind 组件", MessageType.Info);
                return;
            }

            EditorGUILayout.HelpBox($"找到 {autoBinds.Length} 个 AutoBind 组件", MessageType.Info);

            EditorGUILayout.Space();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            foreach (UiAutoBind autoBind in autoBinds)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                EditorGUILayout.LabelField(autoBind.gameObject.name, EditorStyles.boldLabel);

                List<AutoBindData> validBindings = autoBind.GetValidBindings();
                EditorGUILayout.LabelField($"绑定数量: {validBindings.Count}");

                if(GUILayout.Button($"生成代码: {autoBind.gameObject.name}", GUILayout.Height(25)))
                {
                    GenerateCodeFor(autoBind);
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(5);
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();

            // 全部生成按钮
            if(GUILayout.Button("全部生成", GUILayout.Height(35)))
            {
                GenerateAllCode(autoBinds);
            }

            EditorGUILayout.Space();

            // 批量绑定按钮
            if(GUILayout.Button("批量重新绑定", GUILayout.Height(35)))
            {
                BatchRebind(autoBinds);
            }

            // 批量按命名约定自动绑定按钮
            if(GUILayout.Button("批量按命名约定自动绑定", GUILayout.Height(35)))
            {
                BatchAutoBindByNamingConvention(autoBinds);
            }
        }

        /// <summary>
        ///     为指定 AutoBind 生成代码
        /// </summary>
        private void GenerateCodeFor(UiAutoBind uiAutoBind)
        {
            CodeGenerator generator = new CodeGenerator(config);
            List<AutoBindData> validBindings = uiAutoBind.GetValidBindings();

            if(validBindings.Count == 0)
            {
                EditorUtility.DisplayDialog("提示", $"'{uiAutoBind.gameObject.name}' 没有有效的绑定数据", "确定");
                return;
            }

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
                message.AppendLine("如未绑定，可点击『批量重新绑定』重试");

                EditorUtility.DisplayDialog("完成", message.ToString(), "确定");
            }
            catch (Exception e)
            {
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("错误", $"代码生成失败: {e.Message}", "确定");
                Debug.LogError(e);
            }
        }

        /// <summary>
        ///     生成所有代码
        /// </summary>
        private void GenerateAllCode(UiAutoBind[] autoBinds)
        {
            var successCount = 0;
            var failCount = 0;
            List<string> errorMessages = new List<string>();

            // 先生成所有代码
            foreach (UiAutoBind autoBind in autoBinds)
            {
                try
                {
                    CodeGenerator generator = new CodeGenerator(config);
                    List<AutoBindData> validBindings = autoBind.GetValidBindings();

                    if(validBindings.Count > 0)
                    {
                        generator.GenerateCode(autoBind.gameObject, validBindings);
                        successCount++;
                    }
                }
                catch (Exception e)
                {
                    failCount++;
                    errorMessages.Add($"{autoBind.gameObject.name}: {e.Message}");
                    Debug.LogError(e);
                }
            }

            // 等待脚本编译完成后自动绑定
            if(successCount > 0)
            {
                AssetDatabase.Refresh();
                AutoBindBindingScheduler.EnqueueAll(autoBinds);
            }


            var message = $"生成完成！\n成功: {successCount}\n失败: {failCount}";
            if(errorMessages.Count > 0)
            {
                message += "\n\n错误信息:\n" + string.Join("\n", errorMessages);
            }

            EditorUtility.DisplayDialog(failCount > 0 ? "完成但有错误" : "完成", message, "确定");
        }

        /// <summary>
        ///     批量重新绑定
        /// </summary>
        private void BatchRebind(UiAutoBind[] autoBinds)
        {
            if(autoBinds == null || autoBinds.Length == 0)
            {
                EditorUtility.DisplayDialog("提示", "没有找到 AutoBind 组件", "确定");
                return;
            }

            CodeBinder codeBinder = new CodeBinder(config);
            BindingResult result = codeBinder.BindMultiple(autoBinds);

            StringBuilder message = new StringBuilder();
            message.AppendLine("批量绑定完成！");
            message.AppendLine($"成功: {result.successCount}");
            message.AppendLine($"失败: {result.failureCount}");

            if(result.failureList.Count > 0)
            {
                message.AppendLine();
                message.AppendLine("失败列表:");
                foreach (string name in result.failureList)
                {
                    message.AppendLine($"  - {name}");
                }
            }

            if(result.errors.Count > 0)
            {
                message.AppendLine();
                message.AppendLine("错误详情:");
                foreach (string error in result.errors)
                {
                    message.AppendLine($"  - {error}");
                }
            }

            EditorUtility.DisplayDialog(result.failureCount > 0 ? "完成但有错误" : "完成", message.ToString(), "确定");
        }

        /// <summary>
        ///     批量按命名约定自动绑定
        /// </summary>
        private void BatchAutoBindByNamingConvention(UiAutoBind[] autoBinds)
        {
            if(autoBinds == null || autoBinds.Length == 0)
            {
                EditorUtility.DisplayDialog("提示", "没有找到 AutoBind 组件", "确定");
                return;
            }

            // 加载配置
            UiBindConfig config = ConfigManager.LoadConfig();
            if(config == null || config.suffixConfigs == null || config.suffixConfigs.Length == 0)
            {
                EditorUtility.DisplayDialog("错误", "请先在配置文件中添加命名规则", "确定");
                return;
            }

            var totalAdded = 0;
            var totalSkipped = 0;
            var totalNotFound = 0;

            // 遍历所有AutoBind组件
            foreach (UiAutoBind autoBind in autoBinds)
            {
                try
                {
                    Undo.RecordObject(autoBind, "Batch Auto Bind By Naming Convention");

                    // 执行自动绑定
                    var addedCount = 0;
                    var skippedCount = 0;
                    var notFoundCount = 0;

                    AutoBindByNamingConventionRecursive(autoBind.transform, autoBind, config, ref addedCount, ref skippedCount, ref notFoundCount);

                    if(addedCount > 0 || skippedCount > 0 || notFoundCount > 0)
                    {
                        EditorUtility.SetDirty(autoBind);
                    }

                    totalAdded += addedCount;
                    totalSkipped += skippedCount;
                    totalNotFound += notFoundCount;
                }
                catch (Exception e)
                {
                    Debug.LogError($"批量绑定失败: {autoBind.gameObject.name} - {e.Message}");
                }
            }

            // 保存更改
            AssetDatabase.SaveAssets();

            // 显示结果
            StringBuilder message = new StringBuilder();
            message.AppendLine("批量自动绑定完成！");
            message.AppendLine();
            message.AppendLine("总计:");
            message.AppendLine($"  ✓ 新增绑定: {totalAdded}");
            message.AppendLine($"  ○ 已存在（跳过）: {totalSkipped}");
            if(totalNotFound > 0)
            {
                message.AppendLine($"  ✗ 未找到组件: {totalNotFound}");
            }
            message.AppendLine($"  处理对象数: {autoBinds.Length}");

            EditorUtility.DisplayDialog("完成", message.ToString(), "确定");
        }

        /// <summary>
        ///     递归按命名约定自动绑定
        /// </summary>
        private void AutoBindByNamingConventionRecursive(Transform current, UiAutoBind parentUiAutoBind, UiBindConfig config, ref int addedCount, ref int skippedCount, ref int notFoundCount)

        {
            AutoBindUtility.AutoBindByNamingConventionRecursive(current, parentUiAutoBind, config, ref addedCount, ref skippedCount, ref notFoundCount);
        }
    }
}