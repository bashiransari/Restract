using Microsoft.Extensions.DependencyInjection;

namespace Restract.Microsoft.Extensions.DI
{
    public static class ServiceCollectionServiceExtensions
    {
        public static IServiceCollection AddRestClient<T>(this IServiceCollection services, RestClientFactory restClientFactory) where T : class
        {
            return services.AddTransient(p => restClientFactory.CreateClient<T>());
        }
    }
}