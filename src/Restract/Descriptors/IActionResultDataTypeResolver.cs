namespace Restract.Descriptors
{
    using System.Reflection;
    using Restract.Contract;

    public interface IActionResultDataTypeResolver
    {
        ActionResultDataType Resolve(MethodInfo methodInfo);
    }
}