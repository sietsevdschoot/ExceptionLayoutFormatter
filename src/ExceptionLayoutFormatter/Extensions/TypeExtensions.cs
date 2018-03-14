using System;
using System.Linq;

namespace ExceptionLayoutFormatter.Extensions
{
    public static class TypeExtensions
    {
        internal static string GetTypeName(this Type type)
        {
            string name;
            if (type.IsGenericType  && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                name = $"{GetTypeName(type.GetGenericArguments()[0])}?";
            }
            else if (type.IsGenericType)
            {
                name = $"{type.Name}<{string.Join(", ", type.GetGenericArguments().Select(GetTypeName))}>";
                name = name.Replace(name.Substring(name.IndexOf('`'), 2), string.Empty);
            }
            else
            {
                name = type.Name;
            }

            return name;
        }
    }
}