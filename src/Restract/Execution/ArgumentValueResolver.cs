namespace Restract.Execution
{
    using Restract.Contract;
    using Restract.Contract.Parameter;

    internal class ArgumentValueResolver : ParameterValueResolver
    {
        private readonly int _paramterIndex;

        public ArgumentValueResolver(int paramterIndex)
        {
            _paramterIndex = paramterIndex;
        }

        public override object GetValue(string parameterName, MethodCallInfo callInfo)
        {
            return callInfo.Arguments[_paramterIndex];
        }
    }
}