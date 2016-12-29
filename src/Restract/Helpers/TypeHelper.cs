namespace Restract.Helpers
{
    using System;
    using System.ComponentModel;
    using System.Reflection;

    internal static class TypeHelper
    {
        internal static bool IsSimpleType(Type type)
        {
            return type.GetTypeInfo().IsPrimitive || type == typeof(string) || type == typeof(DateTime) || type == typeof(decimal) || type == typeof(Guid) 
                || type == typeof(DateTimeOffset) || type == typeof(TimeSpan);
        }

        internal static bool IsSimpleUnderlyingType(Type type)
        {
            Type underlyingType = Nullable.GetUnderlyingType(type);
            if (underlyingType != null)
            {
                type = underlyingType;
            }
            return IsSimpleType(type);
        }

        internal static bool CanConvertFromString(Type type)
        {
            return IsSimpleUnderlyingType(type) || HasStringConverter(type);
        }

        internal static bool HasStringConverter(Type type)
        {
            return TypeDescriptor.GetConverter(type).CanConvertFrom(typeof(string));
        }

    }
}