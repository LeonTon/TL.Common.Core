using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TL.Common.Core
{
    /// <summary>
    /// 枚举辅助类
    /// </summary>
    public static class EnumUtility
    {
        /// <summary> 
        /// 获得枚举类型数据项（不包括空项）
        /// </summary> 
        /// <param name="enumType">枚举类型</param> 
        /// <returns></returns> 
        public static IList<object> GetItems(Type enumType)
        {
            if (!enumType.IsEnum)
                throw new InvalidOperationException();

            IList<object> list = new List<object>();

            // 获取Description特性 
            Type typeDescription = typeof(DescriptionAttribute);
            // 获取枚举字段
            FieldInfo[] fields = enumType.GetFields();
            foreach (FieldInfo field in fields)
            {
                if (!field.FieldType.IsEnum)
                    continue;

                // 获取枚举值
                int value = (int)enumType.InvokeMember(field.Name, BindingFlags.GetField, null, null, null);

                // 不包括空项
                if (value > 0)
                {
                    string text = string.Empty;
                    object[] array = field.GetCustomAttributes(typeDescription, false);

                    if (array.Length > 0) text = ((DescriptionAttribute)array[0]).Description;
                    else text = field.Name; //没有描述，直接取值

                    //添加到列表
                    list.Add(new { Value = value, Text = text });
                }
            }
            return list;
        }
        private static string GetFieldDesc(FieldInfo field_info)
        {
            object[] attrs = field_info.GetCustomAttributes(typeof(DescriptionAttribute), false);
            return (attrs.Length > 0) ? ((DescriptionAttribute)attrs[0]).Description : field_info.Name;
        }


        /// <summary>
        /// 获取一个枚举类型的列表，用于在dropdownList中显示
        /// </summary>
        /// <returns></returns>
        public static List<EnumItem> GetEnumItems(Type type)
        {
            //System.Type type = typeof(T);
            FieldInfo[] fields = type.GetFields();

            List<EnumItem> itemList = new List<EnumItem>(fields.Length);
            itemList.AddRange(from fi in fields
                              where fi.FieldType == type
                              select new EnumItem
                              {
                                  Name = fi.Name,
                                  Value = Convert.ToInt32(fi.GetRawConstantValue()),
                                  Description = GetFieldDesc(fi)
                              });

            return itemList;
        }

        /// <summary>
        /// 获取按照枚举值划分的枚举项列表
        /// </summary>
        /// <param name="type">枚举类型</param>
        /// <param name="flag">比较方式，0为小于，1为等于，2为大于</param>
        /// <param name="value">比较值</param>
        /// <returns>返回经过比较的枚举项列表</returns>
        public static List<EnumItem> GetEnumItems(Type type, int flag, int value)
        {
            FieldInfo[] fields = type.GetFields();

            List<EnumItem> itemList = new List<EnumItem>(fields.Length);
            itemList.AddRange(from fi in fields
                              where fi.FieldType == type && CompareItem(fi, flag, value)
                              select new EnumItem
                              {
                                  Name = fi.Name,
                                  Value = Convert.ToInt32(fi.GetRawConstantValue()),
                                  Description = GetFieldDesc(fi)
                              });

            return itemList;
        }

        private static bool CompareItem(FieldInfo fi, int flag, int value)
        {
            int currValue = Convert.ToInt32(fi.GetRawConstantValue());
            //判断是大于还是小于
            bool toCompare = true;
            switch (flag)
            {
                case 0:
                    toCompare = currValue < value;
                    break;
                case 1:
                    toCompare = currValue == value;
                    break;
                case 2:
                    toCompare = currValue > value;
                    break;
                default:
                    break;
            }
            return toCompare;
        }

        /// <summary>
        /// 获取枚举值的描述
        /// </summary>
        /// <param name="enumType">指定的枚举类型</param>
        /// <param name="enumValue">枚举类型的值</param>
        /// <returns>枚举值的描述,需用DescriptionAttribute进行描述</returns>
        public static string GetDescription(Type enumType, int enumValue)
        {
            if (enumType == null)
            {
                return string.Empty;
            }
            if (!enumType.IsEnum)
            {
                return string.Empty;
            }
            List<EnumItem> items = GetEnumItems(enumType);
            EnumItem item = items.Find(p => p.Value == enumValue);
            if (item == null)
            {
                return string.Empty;
            }
            return item.Description;
        }

        /// <summary>
        /// 获取枚举值的描述
        /// </summary>
        /// <param name="enumType">指定的枚举类型</param>
        /// <param name="enumValue">枚举类型的值</param>
        /// <returns>枚举值的描述,需用DescriptionAttribute进行描述</returns>
        public static EnumItem GetEnum(Type enumType, int enumValue)
        {
            if (enumType == null)
            {
                return null;
            }
            if (!enumType.IsEnum)
            {
                return null;
            }
            List<EnumItem> items = GetEnumItems(enumType);
            EnumItem item = items.Find(p => p.Value == enumValue);
            return item;
        }

        /// <summary>
        /// 获取枚举值的描述
        /// </summary>
        /// <param name="enumType">指定的枚举类型</param>
        /// <param name="enumName">枚举类型的Name</param>
        /// <returns>枚举值的描述,需用DescriptionAttribute进行描述</returns>
        public static string GetDescription(Type enumType, string enumName)
        {
            if (enumType == null)
            {
                return string.Empty;
            }
            if (!enumType.IsEnum)
            {
                return string.Empty;
            }
            List<EnumItem> items = GetEnumItems(enumType);
            EnumItem item = items.Find(p => p.Name == enumName);
            return item == null ? string.Empty : item.Description;
        }

        /// <summary>
        /// 获取枚举值的描述
        /// </summary>
        /// <param name="enumValue">指定的枚举值: 如SystemParameterEnum.ServerLevel</param>
        /// <returns>枚举值的描述,需用DescriptionAttribute进行描述</returns>
        public static string GetDescription(object enumValue)
        {
            if (enumValue == null)
            {
                return string.Empty;
            }

            if (enumValue.GetType().IsEnum)
            {
                return GetDescription(enumValue.GetType(),
                    Convert.ToInt32(enumValue));
            }
            return string.Empty;
        }

        public static EnumItem GetEnumItem(Type enumType, string enumName)
        {
            if (enumType == null || string.IsNullOrEmpty(enumName))
            {
                return null;
            }
            if (!enumType.IsEnum)
            {
                return null;
            }
            List<EnumItem> items = GetEnumItems(enumType);
            EnumItem item = items.Find(p => p.Name == enumName);
            return item;
        }

        public static EnumItem GetEnumItem(Type enumType, int enumValue)
        {
            if (enumType == null)
            {
                return null;
            }
            if (!enumType.IsEnum)
            {
                return null;
            }
            List<EnumItem> items = GetEnumItems(enumType);
            EnumItem item = items.Find(p => p.Value == enumValue);
            return item;
        }

        /// <summary>
        /// 获取枚举的描述信息
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static String GetEnumDesc(Enum e)
        {
            if (e == null)
                return "";

            FieldInfo EnumInfo = e.GetType().GetField(e.ToString());
            if (EnumInfo == null)
                return e.ToString();

            DescriptionAttribute[] EnumAttributes
                = (DescriptionAttribute[])EnumInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (EnumAttributes.Length > 0)
            {
                return EnumAttributes[0].Description;
            }
            return e.ToString();

        }
    }

    public class EnumItem
    {
        /// <summary>
        /// 枚举名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 枚举值
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// 枚举描述
        /// </summary>
        public string Description { get; set; }
    }

}