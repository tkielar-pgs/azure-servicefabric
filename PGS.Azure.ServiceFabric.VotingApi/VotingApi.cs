using System.Collections.Generic;
using System.Fabric;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace PGS.Azure.ServiceFabric.VotingApi
{
    /// <summary>
    /// The FabricRuntime creates an instance of this class for each service type instance. 
    /// </summary>
    internal sealed class VotingApi : StatefulService
    {
        public VotingApi(StatefulServiceContext context) : base(context) { }

        /// <summary>
        /// Optional override to create listeners (like tcp, http) for this service instance.
        /// </summary>
        /// <returns>The collection of listeners.</returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            yield return new ServiceReplicaListener(serviceContext =>
                new KestrelCommunicationListener(serviceContext, (url, listener) =>
                {
                    return new WebHostBuilder()
                        .UseKestrel()
                        .ConfigureServices(
                            services => services
                                .AddSingleton(serviceContext)
                                .AddSingleton(StateManager))
                        .UseContentRoot(Directory.GetCurrentDirectory())
                        .UseStartup<Startup>()
                        .UseServiceFabricIntegration(listener, ServiceFabricIntegrationOptions.UseUniqueServiceUrl)
                        .UseUrls(url)
                        .Build();
                }));
        }
    }
}
