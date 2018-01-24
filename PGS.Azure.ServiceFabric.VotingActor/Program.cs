using System.Threading;
using Microsoft.ServiceFabric.Actors.Runtime;

namespace PGS.Azure.ServiceFabric.VotingActor
{
    internal static class Program
    {
        private static void Main()
        {
            ActorRuntime.RegisterActorAsync<VotingActor>((context, actorType) => new ActorService(context, actorType)).GetAwaiter().GetResult();

            Thread.Sleep(Timeout.Infinite);
        }
    }
}
