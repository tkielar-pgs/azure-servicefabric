using Microsoft.Extensions.Configuration;

namespace PGS.Azure.ServiceFabric.VotingWeb.Configuration
{
    public static class ServiceFabricConfigExtensions
    {
        public static IConfigurationBuilder AddServiceFabricConfig(this IConfigurationBuilder builder, string packageName) =>
            builder.Add(new ServiceFabricConfigSource(packageName));
    }
}