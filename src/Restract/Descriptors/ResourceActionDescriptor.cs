namespace Restract.Descriptors
{
    using System.Net.Http;
    using Restract.Contract;
    using Restract.Contract.Parameter;

    public class ResourceActionDescriptor: IResourceActionDescriptor
    {
        public HttpMethod Method { get; set; }
        public TemplateUri ActionPath { get; set; }
        public HttpParameterCollection Parameters { get; set; }
        public ActionResultDataType ResultDataType { get; set; }
        public IResourceDescriptor ResourceDescriptor { get; set; }

        public ResourceActionDescriptor(IResourceDescriptor resourceDescriptor)
        {
            ResourceDescriptor = resourceDescriptor;
            Parameters = new HttpParameterCollection();
        }
    }

}