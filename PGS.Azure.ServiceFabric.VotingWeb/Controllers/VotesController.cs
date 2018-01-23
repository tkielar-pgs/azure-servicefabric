using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Query;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using PGS.Azure.ServiceFabric.VotingService.Abstractions;
using PGS.Azure.ServiceFabric.VotingWeb.Configuration;
using PGS.Azure.ServiceFabric.VotingWeb.Models;

namespace PGS.Azure.ServiceFabric.VotingWeb.Controllers
{
    [Route("api/[Controller]")]
    public class VotesController : ControllerBase
    {
        private readonly Uri _votingServiceUri;
        private readonly FabricClient _fabricClient;

        public VotesController(IOptionsSnapshot<ServiceFabricCommunicationOptions> options, StatelessServiceContext ctx, FabricClient fabricClient)
        {
            _fabricClient = fabricClient;
            _votingServiceUri = new Uri($"{ctx.CodePackageActivationContext.ApplicationName}/{options.Value.VotingServiceName}");            
        }

        [HttpGet]
        public async Task<IEnumerable<KeyValuePair<string, long>>> Get(CancellationToken cancellationToken)
        {
            ServicePartitionList servicePartitions = 
                await _fabricClient.QueryManager.GetPartitionListAsync(_votingServiceUri, null, TimeSpan.FromMinutes(1), cancellationToken);

            var responses = await Task.WhenAll(servicePartitions.Select(partition => GetAllVotes(partition, cancellationToken)));

            return responses.SelectMany(x => x);
        }

        [HttpPost]
        public Task Post([FromBody] VoteKey voteKey, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        [HttpDelete("{id}")]
        public Task Delete(string id, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private Task<KeyValuePair<string, long>[]> GetAllVotes(Partition partition, CancellationToken cancellationToken)
        {
            var service = GetService(((Int64RangePartitionInformation)partition.PartitionInformation).LowKey);
            return service.GetAll(cancellationToken);
        }

        private IVotingService GetService(long partitionKey) => ServiceProxy.Create<IVotingService>(_votingServiceUri, new ServicePartitionKey(partitionKey));
    }
}