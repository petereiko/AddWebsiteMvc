using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AddWebsiteMvc.Business.Enums
{
    public static class EnumHelper
    {
        public static string GetEnumDescription(Enum enumVal)
        {
            FieldInfo fieldInfo = enumVal.GetType().GetField(enumVal.ToString());
            DescriptionAttribute descriptionAttribute = fieldInfo?.GetCustomAttribute<DescriptionAttribute>();

            return descriptionAttribute?.Description ?? enumVal.ToString();
        }

        public static List<string> GetEnumDescriptions<T>() where T : Enum
        {
            return Enum.GetValues(typeof(T))
                       .Cast<T>()
                       .Select(value => GetEnumDescription(value))
                       .ToList();
        }
    }
}
