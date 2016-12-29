namespace Restract.Core
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    // To make GetCustomAttributes<T>() testable
    internal class DefaultAttributeFinder : IAttributeFinder
    {
        public IEnumerable<Attribute> GetAttributes(ICustomAttributeProvider owner, Type attributeType, bool inherit = false)
        {
            return (IEnumerable<Attribute>)owner.GetCustomAttributes(attributeType, inherit);
        }

        public IEnumerable<Attribute> GetAttributes(ICustomAttributeProvider owner, bool inherit = false)
        {
            return (IEnumerable<Attribute>)owner.GetCustomAttributes(inherit);
        }

        public IEnumerable<T> GetAttributes<T>(MemberInfo owner, bool inherit = false) where T : Attribute
        {
            return owner.GetCustomAttributes<T>(inherit);
        }

        public IEnumerable<T> GetAttributes<T>(ParameterInfo owner, bool inherit = false) where T : Attribute
        {
            return owner.GetCustomAttributes<T>(inherit);
        }
    }
}