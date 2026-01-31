using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace CUiAutoBind
{
    /// <summary>
    ///     字符串工具类
    /// </summary>
    public static class StringUtil
    {
        /// <summary>
        ///     获取安全的类名
        /// </summary>
        public static string GetSafeClassName(string gameObjectName)
        {
            if(string.IsNullOrEmpty(gameObjectName))
                return "AutoBindUI";

            string className = gameObjectName;

            // 移除不合法的字符
            char[] invalidChars = Path.GetInvalidFileNameChars();
            className = new string(className.Where(c => !invalidChars.Contains(c)).ToArray());

            // 移除空格
            className = className.Replace(" ", "");

            // 首字母大写（确保字符串不为空）
            if(!string.IsNullOrEmpty(className))
            {
                className = char.ToUpper(className[0]) + className[1..];
            }

            return className;
        }

        /// <summary>
        ///     转换为驼峰命名
        /// </summary>
        public static string ToCamelCase(string str)
        {
            if(string.IsNullOrEmpty(str))
                return str;

            return char.ToLower(str[0]) + str[1..];
        }

        /// <summary>
        ///     获取组件类型名称
        /// </summary>
        public static string GetComponentTypeName(Type type)
        {
            if(type == null)
                return "Component";

            // 处理 Unity 内置类型
            if(type == typeof(Button)) return "Button";
            if(type == typeof(Text)) return "Text";
            if(type == typeof(Image)) return "Image";
            if(type == typeof(Toggle)) return "Toggle";
            if(type == typeof(Slider)) return "Slider";
            if(type == typeof(ScrollRect)) return "ScrollRect";
            if(type == typeof(Transform)) return "Transform";
            if(type == typeof(GameObject)) return "GameObject";

            // 处理泛型类型
            if(type.IsGenericType)
            {
                return type.Name.Split('`')[0];
            }

            // 返回完整类型名
            return type.Name;
        }
    }
}