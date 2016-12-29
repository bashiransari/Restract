namespace Restract.Contract.Parameter
{
    public class HttpParameter
    {
        public string Name { get; set; }
        public HttpParameterType Type { get; set; }
        public ParameterValueResolver ValueResolver { get; set; }

        public HttpParameter()
        {
           
        }

        public HttpParameter(string name, HttpParameterType type, ParameterValueResolver valueResolver)
        {
            Name = name;
            Type = type;
            ValueResolver = valueResolver;
        }

        public object GetValue(MethodCallInfo methodCallInfo)
        {
            return ValueResolver.GetValue(Name, methodCallInfo);
        }

        public T GetValue<T>(MethodCallInfo methodCallInfo)
        {
            return (T)GetValue(methodCallInfo);
        }
    }
}