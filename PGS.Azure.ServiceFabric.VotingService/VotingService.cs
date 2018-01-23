using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using PGS.Azure.ServiceFabric.VotingService.Abstractions;

namespace PGS.Azure.ServiceFabric.VotingService
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class VotingService : StatefulService, IVotingService
    {
        public VotingService(StatefulServiceContext context) : base(context) { }

        /// <summary>
        /// Optional override to create listeners (e.g., HTTP, Service Remoting, WCF, etc.) for this service replica to handle client or user requests.
        /// </summary>
        /// <remarks>
        /// For more information on service communication, see https://aka.ms/servicefabricservicecommunication
        /// </remarks>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners() => this.CreateServiceRemotingReplicaListeners();

        public Task<KeyValuePair<string, long>[]> GetAll(CancellationToken cancellationToken = default(CancellationToken)) => 
            throw new System.NotImplementedException();
    }
}
