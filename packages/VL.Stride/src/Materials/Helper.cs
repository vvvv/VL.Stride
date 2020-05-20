using Stride.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace VL.Stride.Materials
{
    static class Helper
    {
        static readonly Regex FSpaceAndCharRegex = new Regex(" [a-zA-Z]", RegexOptions.Compiled);
        static readonly Regex FLowerAndUpperRegex = new Regex("[a-z0-9][A-Z0-9]", RegexOptions.Compiled);

        public static string UpperCaseAfterSpace(this string name)
        {
            return FSpaceAndCharRegex.Replace(name, m => $" {char.ToUpper(m.Value[1])}");
        }

        public static string InsertSpaces(this string name)
        {
            return FLowerAndUpperRegex.Replace(name, m => $"{m.Value[0]} {m.Value[1]}");
        }

        public static object GetValue(this MemberInfo member, object instance)
        {
            if (member is PropertyInfo p)
                return p.GetValue(instance);
            else if (member is FieldInfo f)
                return f.GetValue(instance);
            else
                throw new NotImplementedException();
        }

        public static void SetValue(this MemberInfo member, object instance, object value)
        {
            if (member is PropertyInfo p)
                p.SetValue(instance, value);
            else if (member is FieldInfo f)
                f.SetValue(instance, value);
            else
                throw new NotImplementedException();
        }

        public static Type GetPropertyType(this MemberInfo member)
        {
            if (member is PropertyInfo p)
                return p.PropertyType;
            else if (member is FieldInfo f)
                return f.FieldType;
            else
                throw new NotImplementedException();
        }

        public static IEnumerable<(MemberInfo Property, int? Order, string Name, string Category)> GetStrideProperties(this Type type, List<string> properties = default)
        {
            if (properties != null)
            {
                return properties.Select(n => type.GetProperty(n, BindingFlags.Public | BindingFlags.Instance) as MemberInfo ?? type.GetField(n, BindingFlags.Public | BindingFlags.Instance))
                    .Select((p, i) =>
                    {
                        var display = p.GetCustomAttribute<DisplayAttribute>();
                        return (
                            Property: p,
                            Order: new int?(i),
                            Name: p.Name.InsertSpaces(),
                            Category: display?.Category?.UpperCaseAfterSpace());
                    });
            }
            else
            {
                return type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.GetMethod != null && p.GetMethod.IsPublic && ((p.SetMethod != null && p.SetMethod.IsPublic) || !p.PropertyType.IsValueType))
                    .Select(p =>
                    {
                        // Do not include properties which have explicit ignore flags set
                        if (p.GetCustomAttribute<DataMemberIgnoreAttribute>() != null)
                            return default;

                        var display = p.GetCustomAttribute<DisplayAttribute>();
                        if (display != null && !display.Browsable)
                            return default;

                        // At least one of the following attributes must be set
                        var dataMember = p.GetCustomAttribute<DataMemberAttribute>();
                        var customSerializer = p.GetCustomAttribute<DataMemberCustomSerializerAttribute>();
                        if (display is null && dataMember is null && customSerializer is null)
                            return default;

                        return (
                            Property: (MemberInfo)p,
                            Order: dataMember?.Order,
                            Name: display?.Name?.UpperCaseAfterSpace() ?? p.Name.InsertSpaces(),
                            Category: display?.Category?.UpperCaseAfterSpace());
                    })
                    .Where(p => p != default);
            }
        }
    }
}
