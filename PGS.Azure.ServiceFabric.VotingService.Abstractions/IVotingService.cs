using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.FabricTransport;

[assembly: FabricTransportServiceRemotingProvider(RemotingListener = RemotingListener.V2Listener, RemotingClient = RemotingClient.V2Client)]

namespace PGS.Azure.ServiceFabric.VotingService.Abstractions
{    
    public interface IVotingService : IService
    {
        Task<KeyValuePair<string, long>[]> GetAll(CancellationToken cancellationToken = default(CancellationToken));
    }
}