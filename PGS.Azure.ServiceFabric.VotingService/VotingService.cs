using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
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
        private static readonly Uri VotesDictionaryUri = new Uri("store:/votes");

        public VotingService(StatefulServiceContext context) : base(context) { }

        /// <summary>
        /// Optional override to create listeners (e.g., HTTP, Service Remoting, WCF, etc.) for this service replica to handle client or user requests.
        /// </summary>
        /// <remarks>
        /// For more information on service communication, see https://aka.ms/servicefabricservicecommunication
        /// </remarks>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners() => this.CreateServiceRemotingReplicaListeners();

        public async Task<KeyValuePair<string, long>[]> GetAll(CancellationToken cancellationToken = default(CancellationToken))
        {
            ConditionalValue<IReliableDictionary<string, long>> tryGetResult = 
                await StateManager.TryGetAsync<IReliableDictionary<string, long>>(VotesDictionaryUri);

            if (!tryGetResult.HasValue)
            {
                return new KeyValuePair<string, long>[0];
            }

            var result = new List<KeyValuePair<string, long>>();
            using (ITransaction tx = StateManager.CreateTransaction())
            {
                IAsyncEnumerable<KeyValuePair<string, long>> enumerable = await tryGetResult.Value.CreateEnumerableAsync(tx);
                IAsyncEnumerator<KeyValuePair<string, long>> enumerator = enumerable.GetAsyncEnumerator();

                while (await enumerator.MoveNextAsync(cancellationToken))
                {
                    result.Add(enumerator.Current);
                }
            }

            return result.ToArray();
        }

        public async Task Add(string id, CancellationToken cancellationToken)
        {
            IReliableDictionary<string, long> dictionary = await StateManager.GetOrAddAsync<IReliableDictionary<string, long>>(VotesDictionaryUri);

            using (ITransaction tx = StateManager.CreateTransaction())
            {
                await dictionary.AddOrUpdateAsync(tx, id, 1, (_, count) => count + 1, TimeSpan.FromMinutes(1), cancellationToken);
                await tx.CommitAsync();
            }
        }

        public async Task Remove(string id, CancellationToken cancellationToken)
        {
            ConditionalValue<IReliableDictionary<string, long>> tryGetResult = 
                await StateManager.TryGetAsync<IReliableDictionary<string, long>>(VotesDictionaryUri);

            if (!tryGetResult.HasValue)
            {
                return;
            }

            using (ITransaction tx = StateManager.CreateTransaction())
            {
                await tryGetResult.Value.TryRemoveAsync(tx, id, TimeSpan.FromMinutes(1), cancellationToken);
                await tx.CommitAsync();
            }
        }
    }
}
