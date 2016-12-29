namespace Restract.Core
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public interface IAttributeFinder
    {
        IEnumerable<Attribute> GetAttributes(ICustomAttributeProvider owner, Type attributeType, bool inherit = false);

        IEnumerable<Attribute> GetAttributes(ICustomAttributeProvider owner, bool inherit = false);

        IEnumerable<T> GetAttributes<T>(MemberInfo owner, bool inherit = false) where T : Attribute;

        IEnumerable<T> GetAttributes<T>(ParameterInfo owner, bool inherit = false) where T : Attribute;
    }
}