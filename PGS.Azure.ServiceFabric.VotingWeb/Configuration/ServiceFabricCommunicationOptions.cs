namespace PGS.Azure.ServiceFabric.VotingWeb.Configuration
{
    public class ServiceFabricCommunicationOptions
    {
        public int ProxyPort { get; set; }
        public string VotingActorServiceName { get; set; }
    }
}