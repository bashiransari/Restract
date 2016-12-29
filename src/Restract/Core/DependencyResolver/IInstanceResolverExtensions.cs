namespace Restract.Core.DependencyResolver
{
    using Restract.Core.DependencyResolver.InstanceResolvers;

    internal static class InstanceResolverExtensions
    {
        internal static IInstanceResolver AsSingletone(this IInstanceResolver instanceResolver)
        {
            return new SingletoneInstaceResolver(instanceResolver);
        }
    }
}