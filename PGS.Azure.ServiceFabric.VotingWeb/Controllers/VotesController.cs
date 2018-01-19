using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Query;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PGS.Azure.ServiceFabric.VotingWeb.Configuration;

namespace PGS.Azure.ServiceFabric.VotingWeb.Controllers
{
    [Route("api/[Controller]")]
    public class VotesController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly StatelessServiceContext _serviceContext;        
        private readonly FabricClient _fabricClient;
        private readonly ServiceFabricCommunicationOptions _communicationOptions;

        public VotesController(
            HttpClient httpClient, 
            StatelessServiceContext serviceContext, 
            FabricClient fabricClient, 
            IOptionsSnapshot<ServiceFabricCommunicationOptions> communicationOptions)
        {
            _httpClient = httpClient;
            _serviceContext = serviceContext;
            _fabricClient = fabricClient;
            _communicationOptions = communicationOptions.Value;
        }

        public async Task<IEnumerable<KeyValuePair<string, long>>> Get(CancellationToken cancellationToken)
        {
            string serviceUri = $"{_serviceContext.CodePackageActivationContext.ApplicationName}/{_communicationOptions.VotingApiServiceName}";
            ServicePartitionList partitions = await _fabricClient.QueryManager.GetPartitionListAsync(new Uri(serviceUri), null, TimeSpan.FromMinutes(1), cancellationToken);
            IEnumerable<KeyValuePair<string, long>>[] responses = await Task.WhenAll(partitions.Select(partition => GetAllVotes(partition, cancellationToken)));
            return responses.SelectMany(x => x);
        }

        private async Task<IEnumerable<KeyValuePair<string, long>>> GetAllVotes(Partition partition, CancellationToken cancellationToken)
        {
            long partitionKey = ((Int64RangePartitionInformation) partition.PartitionInformation).LowKey;
            string proxyUrl =
                $"{ProxyBaseUrl}/{_communicationOptions.VotingApiServiceName}/api/votes?PartitionKind={partition.PartitionInformation.Kind}&PartitionKey={partitionKey}";

            HttpResponseMessage response = await _httpClient.GetAsync(proxyUrl, cancellationToken);
            response.EnsureSuccessStatusCode();

            return JsonConvert.DeserializeObject<KeyValuePair<string, long>[]>(await response.Content.ReadAsStringAsync());
        }

        private string ProxyBaseUrl =>
            $"http://localhost:{_communicationOptions.ProxyPort}/{_serviceContext.CodePackageActivationContext.ApplicationName.Replace("fabric:/", "")}/";
    }
}