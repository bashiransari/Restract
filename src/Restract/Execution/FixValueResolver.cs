namespace Restract.Execution
{
    using Restract.Contract;
    using Restract.Contract.Parameter;

    internal class FixValueResolver : ParameterValueResolver
    {
        private readonly string _value;
        public FixValueResolver(string value)
        {
            _value = value;
        }
        public override object GetValue(string parameterName, MethodCallInfo callInfo)
        {
            return _value;
        }
    }
}