namespace Restract.Contract.Attributes
{
    using System;

    using Restract.Contract.Parameter;

    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Interface | AttributeTargets.Method,AllowMultiple = true)]
    public class HeaderAttribute : ParameterBindingAttribute
    {
        public HeaderAttribute()
            : base(HttpParameterType.Header)
        {

        }
        public HeaderAttribute(string name)
           : base(HttpParameterType.Header, name)
        {

        }

        public HeaderAttribute(string name, string value)
           : base(HttpParameterType.Header, name)
        {
            Value = value;
        }
    }
}