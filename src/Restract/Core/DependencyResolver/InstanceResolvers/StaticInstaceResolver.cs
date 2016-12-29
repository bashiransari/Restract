namespace Restract.Core.DependencyResolver.InstanceResolvers
{
    using System;

    public class StaticInstaceResolver : BaseInstaceResolver
    {
        private readonly object _object;

        public StaticInstaceResolver(object @object)
        {
            _object = @object;
        }

        public override object Resolve()
        {
            return _object;
        }

        public override Type GetObjectType()
        {
            return _object.GetType();
        }
    }
}