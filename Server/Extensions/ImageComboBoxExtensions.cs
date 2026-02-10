using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors.Repository;
using Library;
using Server.Helpers;
using System;

namespace Server.Extensions
{
    /// <summary>
    /// ImageComboBox 控件扩展方法
    /// </summary>
    public static class ImageComboBoxExtensions
    {
        /// <summary>
        /// 添加枚举项，支持中文名称显示
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <param name="comboBox">下拉框控件</param>
        public static void AddEnumWithChineseName<T>(this RepositoryItemImageComboBox comboBox) 
            where T : Enum
        {
            foreach (T value in Enum.GetValues(typeof(T)))
            {
                string displayName = value.ToString(); // 默认使用英文名
                
                // 如果是 MagicType 且有中文映射，使用中文名称
                if (typeof(T) == typeof(MagicType))
                {
                    MagicType magicType = (MagicType)(object)value;
                    displayName = MagicTypeNames.GetChineseName(magicType);
                }
                
                // 添加项：显示名称 + 枚举值
                comboBox.Items.Add(new ImageComboBoxItem(displayName, value));
            }
        }
    }
}
