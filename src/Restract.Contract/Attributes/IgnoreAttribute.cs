namespace Restract.Contract.Attributes
{
    using System;

    using Restract.Contract.Parameter;

    [AttributeUsage(AttributeTargets.Parameter)]
    public class IgnoreAttribute : ParameterBindingAttribute
    {
        public IgnoreAttribute()
            : base(HttpParameterType.None)
        {

        }
    }
}