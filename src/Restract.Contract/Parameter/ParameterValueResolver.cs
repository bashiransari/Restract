namespace Restract.Contract.Parameter
{
    public abstract class ParameterValueResolver
    {
        public abstract object GetValue(string parameterName, MethodCallInfo callInfo);
    }
}