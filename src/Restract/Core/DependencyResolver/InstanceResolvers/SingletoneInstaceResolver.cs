namespace Restract.Core.DependencyResolver.InstanceResolvers
{
    using System;

    public class SingletoneInstaceResolver : BaseInstaceResolver
    {
        private readonly Lazy<object> _lazyInstance;
        private readonly Type _objectType;

        public SingletoneInstaceResolver(IInstanceResolver instanceResolver)
        {
            _lazyInstance = new Lazy<object>(instanceResolver.Resolve);
            _objectType = instanceResolver.GetObjectType();
        }

        public override object Resolve()
        {
            return _lazyInstance.Value;
        }

        public override Type GetObjectType()
        {
            return _objectType;
        }
    }
}