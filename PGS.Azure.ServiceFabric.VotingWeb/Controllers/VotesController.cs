using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PGS.Azure.ServiceFabric.VotingWeb.Models;

namespace PGS.Azure.ServiceFabric.VotingWeb.Controllers
{
    [Route("api/[Controller]")]
    public class VotesController : ControllerBase
    {
        [HttpGet]
        public Task<IEnumerable<KeyValuePair<string, long>>> Get(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
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
    }
}