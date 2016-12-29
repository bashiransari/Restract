namespace Restract.Contract.Attributes
{
    using System;

    public class ResourceAttribute : Attribute
    {
        public string ResourcePath { get; set; }

        public ResourceAttribute(string resourcePath)
        {
            ResourcePath = resourcePath;
        }
    }
}