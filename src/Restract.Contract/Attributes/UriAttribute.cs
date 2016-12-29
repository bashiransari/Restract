namespace Restract.Contract.Attributes
{
    using System;

    using Restract.Contract.Parameter;

    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Interface | AttributeTargets.Method, AllowMultiple = true)]
    public class UriAttribute : ParameterBindingAttribute
    {
        public UriAttribute()
            : base(HttpParameterType.Uri)
        {

        }

        public UriAttribute(string name)
            : base(HttpParameterType.Uri, name)
        {

        }
    }
}