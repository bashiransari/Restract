namespace Restract.Contract
{
    using System.Reflection;

    using Restract.Contract.Parameter;

    public interface IResourceDescriptor
    {
        HttpParameterCollection Parameters { get; set; }
        TemplateUri ResourceUrl { get; set; }

        IResourceActionDescriptor GetActionDescriptor(MethodInfo methodInfo);
    }
}