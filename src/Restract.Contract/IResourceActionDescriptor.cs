namespace Restract.Contract
{
    using System.Net.Http;

    using Restract.Contract.Parameter;

    public interface IResourceActionDescriptor
    {
        TemplateUri ActionPath { get; set; }
        HttpMethod Method { get; set; }
        HttpParameterCollection Parameters { get; set; }
        IResourceDescriptor ResourceDescriptor { get; set; }
        ActionResultDataType ResultDataType { get; set; }
    }
}