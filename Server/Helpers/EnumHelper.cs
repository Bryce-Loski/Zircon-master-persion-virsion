using System;
using System.ComponentModel;
using System.Reflection;

namespace Server.Helpers
{
    public static class EnumHelper
    {
        public static string GetDescription(Enum value)
        {
            if (value == null) return string.Empty;

            Type type = value.GetType();
            MemberInfo[] infos = type.GetMember(value.ToString());
            if (infos.Length == 0) return value.ToString();

            var desc = infos[0].GetCustomAttribute<DescriptionAttribute>();
            return desc?.Description ?? value.ToString();
        }
    }
}
