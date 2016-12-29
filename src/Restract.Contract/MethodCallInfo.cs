namespace Restract.Contract
{
    using System.Reflection;

    public class MethodCallInfo
    {
        public MethodInfo MethodInfo { get; set; }

        public object[] Arguments { get; set; }
    }
}