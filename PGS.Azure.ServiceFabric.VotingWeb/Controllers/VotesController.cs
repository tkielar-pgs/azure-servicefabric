using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Query;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Actors.Query;
using PGS.Azure.ServiceFabric.VotingActor.Interfaces;
using PGS.Azure.ServiceFabric.VotingWeb.Configuration;
using PGS.Azure.ServiceFabric.VotingWeb.Models;

namespace PGS.Azure.ServiceFabric.VotingWeb.Controllers
{
    [Route("api/[Controller]")]
    public class VotesController : ControllerBase
    {
        private readonly Uri _actorServiceUri;
        private readonly FabricClient _fabricClient;

        public VotesController(IOptionsSnapshot<ServiceFabricCommunicationOptions> options, StatelessServiceContext context, FabricClient fabricClient)
        {
            _fabricClient = fabricClient;
            _actorServiceUri = new Uri($"{context.CodePackageActivationContext.ApplicationName}/{options.Value.VotingActorServiceName}");
        }

        [HttpGet]
        public async Task<IEnumerable<KeyValuePair<string, long>>> Get(CancellationToken cancellationToken)
        {
            ServicePartitionList partitions = 
                await _fabricClient.QueryManager.GetPartitionListAsync(_actorServiceUri, null, TimeSpan.FromMinutes(1), cancellationToken);

            KeyValuePair<string, long>[][] votes = await Task.WhenAll(partitions.Select(partition => GetAllVotes(partition, cancellationToken)));
            return votes.SelectMany(x => x);
        }

        [HttpPost]
        public Task Post([FromBody] VoteKey voteKey, CancellationToken cancellationToken) => GetActor(new ActorId(voteKey.Id)).Add(cancellationToken);

        [HttpDelete("{id}")]
        public Task Delete(string id, CancellationToken cancellationToken)
        {
            var actorId = new ActorId(id);
            return ActorServiceProxy.Create(_actorServiceUri, actorId).DeleteActorAsync(actorId, cancellationToken);
        }

        private async Task<KeyValuePair<string, long>[]> GetAllVotes(Partition partition, CancellationToken cancellationToken)
        {
            IActorService actorServiceProxy = ActorServiceProxy.Create(_actorServiceUri, ((Int64RangePartitionInformation) partition.PartitionInformation).LowKey);
            ContinuationToken continuationToken = null;

            var result = new List<KeyValuePair<string, long>>();
            do
            {
                PagedResult<ActorInformation> queryResult = await actorServiceProxy.GetActorsAsync(continuationToken, cancellationToken);
                continuationToken = queryResult.ContinuationToken;

                result.AddRange(await Task.WhenAll(queryResult.Items.Select(actor => GetActorVotes(actor, cancellationToken))));
            } while (continuationToken != null);

            return result.ToArray();
        }

        private async Task<KeyValuePair<string, long>> GetActorVotes(ActorInformation actor, CancellationToken cancellationToken)
        {
            IVotingActor actorProxy = GetActor(actor.ActorId);
            return new KeyValuePair<string, long>(actor.ActorId.GetStringId(), await actorProxy.GetCount(cancellationToken));
        }

        private static IVotingActor GetActor(ActorId id) => ActorProxy.Create<IVotingActor>(id);
    }
}