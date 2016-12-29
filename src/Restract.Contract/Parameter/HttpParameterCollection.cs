namespace Restract.Contract.Parameter
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    public class HttpParameterCollection : Collection<HttpParameter>
    {
        public HttpParameter BodyParameter => this.SingleOrDefault(p => p.Type == HttpParameterType.Body);
        public IEnumerable<HttpParameter> Headers => this.Where(p => p.Type == HttpParameterType.Header);
        public IEnumerable<HttpParameter> Uris => this.Where(p => p.Type == HttpParameterType.Uri);
        public IEnumerable<HttpParameter> Cookies => this.Where(p => p.Type == HttpParameterType.Cookie);
        public HttpParameter CancellationToken => this.SingleOrDefault(p => p.Type == HttpParameterType.CancellationToken);

        protected override void InsertItem(int index, HttpParameter item)
        {
            if (item.Type == HttpParameterType.Body || item.Type == HttpParameterType.CancellationToken)
            {
                if (this.Any(p => p.Type == item.Type))
                {
                    throw new InvalidOperationException($"Cannot have multiple parameters of type {item.Type}");
                }
            }

            if (this.Any(p => p.Name == item.Name && p.Type == item.Type))
            {
                throw new InvalidOperationException($"Cannot have multiple parameters with same type {item.Type} and same name {item.Name}");
            }

            base.InsertItem(index, item);
        }

        public void AppendCollection(HttpParameterCollection collection)
        {
            foreach (var item in collection)
            {
                Add(item);
            }
        }

        public HttpParameterCollection Clone()
        {
            var collection = new HttpParameterCollection();
            foreach (var item in this)
            {
                collection.Add(new HttpParameter()
                {
                    Type = item.Type,
                    ValueResolver = item.ValueResolver,
                    Name = item.Name
                });
            }
            return collection;
        }
    }
}