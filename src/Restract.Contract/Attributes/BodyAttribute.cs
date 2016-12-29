namespace Restract.Contract.Attributes
{
    using System;

    using Restract.Contract.Parameter;

    [AttributeUsage(AttributeTargets.Parameter)]
    public class BodyAttribute : ParameterBindingAttribute
    {
        public BodyAttribute()
            : base(HttpParameterType.Body)
        {

        }
        public BodyAttribute(string name)
            : base(HttpParameterType.Body, name)
        {

        }
    }
}