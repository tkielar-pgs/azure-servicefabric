using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using PGS.Azure.ServiceFabric.VotingApi.Models;

namespace PGS.Azure.ServiceFabric.VotingApi.Controllers
{
    [Route("api/[Controller]")]
    public class VotesController
    {
        private static readonly Uri VotesDictionaryName = new Uri("store:/votes");

        private readonly IReliableStateManager _stateManager;

        public VotesController(IReliableStateManager stateManager) => _stateManager = stateManager;

        [HttpGet]
        public async Task<IEnumerable<KeyValuePair<string, long>>> Get(CancellationToken cancellationToken)
        {
            ConditionalValue<IReliableDictionary<string, long>> tryGetResult = await _stateManager.TryGetAsync<IReliableDictionary<string, long>>(VotesDictionaryName);

            if (!tryGetResult.HasValue)
            {
                return Enumerable.Empty<KeyValuePair<string, long>>();
            }

            List<KeyValuePair<string, long>> result = new List<KeyValuePair<string, long>>();

            using (ITransaction tx = _stateManager.CreateTransaction())
            {
                IAsyncEnumerable<KeyValuePair<string, long>> enumerable = await tryGetResult.Value.CreateEnumerableAsync(tx);
                IAsyncEnumerator<KeyValuePair<string, long>> enumerator = enumerable.GetAsyncEnumerator();

                while (await enumerator.MoveNextAsync(cancellationToken))
                {
                    result.Add(enumerator.Current);
                }
            }

            return result;
        }

        [HttpPost]
        public async Task Post([FromBody] VoteKey voteKey, CancellationToken cancellationToken)
        {
            var dictionary = await _stateManager.GetOrAddAsync<IReliableDictionary<string, long>>(VotesDictionaryName);

            using (ITransaction tx = _stateManager.CreateTransaction())
            {
                await dictionary.TryAddAsync(tx, voteKey.Id, 0);
                await tx.CommitAsync();
            }
        }

        [HttpDelete]
        public async Task Delete(string id, CancellationToken cancellationToken)
        {
            var tryGetResult = await _stateManager.TryGetAsync<IReliableDictionary<string, long>>(VotesDictionaryName);

            if (!tryGetResult.HasValue)
            {
                return;
            }

            using (ITransaction tx = _stateManager.CreateTransaction())
            {
                await tryGetResult.Value.TryRemoveAsync(tx, id);
                await tx.CommitAsync();
            }
        }
    }
}