using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Data;
using PGS.Azure.ServiceFabric.VotingActor.Interfaces;

namespace PGS.Azure.ServiceFabric.VotingActor
{
    [StatePersistence(StatePersistence.Persisted)]
    internal class VotingActor : Actor, IVotingActor
    {
        private const string StateName = "count";

        public VotingActor(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
        }

        protected override Task OnActivateAsync() => StateManager.TryAddStateAsync(StateName, 0L);

        public async Task<long> GetCount(CancellationToken cancellationToken)
        {
            ConditionalValue<long> tryGetResult = await StateManager.TryGetStateAsync<long>(StateName, cancellationToken);
            return tryGetResult.HasValue ? tryGetResult.Value : 0L;
        }

        public Task Add(CancellationToken cancellationToken) => StateManager.AddOrUpdateStateAsync(StateName, 1L, (_, count) => count + 1, cancellationToken);
    }
}
