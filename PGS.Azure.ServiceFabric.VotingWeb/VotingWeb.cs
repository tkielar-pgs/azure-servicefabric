using System;
using System.Collections.Generic;
using System.Fabric;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using PGS.Azure.ServiceFabric.VotingWeb.Configuration;

namespace PGS.Azure.ServiceFabric.VotingWeb
{
    /// <summary>
    /// The FabricRuntime creates an instance of this class for each service type instance. 
    /// </summary>
    internal sealed class VotingWeb : StatelessService
    {
        public VotingWeb(StatelessServiceContext context) : base(context) { }

        /// <summary>
        /// Optional override to create listeners (like tcp, http) for this service instance.
        /// </summary>
        /// <returns>The collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            yield return new ServiceInstanceListener(serviceContext =>
                new KestrelCommunicationListener(serviceContext, "ServiceEndpoint", (url, listener) => new WebHostBuilder()
                    .UseKestrel()
                    .ConfigureAppConfiguration(builder => builder.AddServiceFabricConfig("Config"))
                    .ConfigureServices(services => services
                        .AddSingleton(serviceContext)
                        .AddSingleton(new HttpClient())
                        .AddSingleton(new FabricClient()))
                    .UseContentRoot(Directory.GetCurrentDirectory())
                    .UseStartup<Startup>()
                    .UseServiceFabricIntegration(listener, ServiceFabricIntegrationOptions.None)
                    .UseUrls(url)
                    .Build()));
        }
    }
}
