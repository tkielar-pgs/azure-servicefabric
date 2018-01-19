using Microsoft.Extensions.Configuration;

namespace PGS.Azure.ServiceFabric.VotingWeb.Configuration
{
    public class ServiceFabricConfigSource : IConfigurationSource
    {
        public ServiceFabricConfigSource(string packageName) => PackageName = packageName;

        public string PackageName { get; set; }

        public IConfigurationProvider Build(IConfigurationBuilder builder) => new ServiceFabricConfigurationProvider(PackageName);
    }
}