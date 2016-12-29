namespace Restract.Contract.Attributes
{
    using System;

    using Restract.Contract.Parameter;

    public class ParameterBindingAttribute : Attribute
    {
        public HttpParameterType ParameterType { get; private set; }

        public string Name { get; set; }

        public string Value { get; set; }

        public Type ValueResolver { get; set; }

        public ParameterBindingAttribute(HttpParameterType parameterType)
        {
            ParameterType = parameterType;
        }

        public ParameterBindingAttribute(HttpParameterType parameterType, string name)
            : this(parameterType)
        {
            Name = name;
        }
    }
}
