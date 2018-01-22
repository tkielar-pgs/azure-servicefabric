namespace PGS.Azure.ServiceFabric.VotingWeb.Models
{
    public class VoteKey
    {
        public string Id { get; set; }

        protected bool Equals(VoteKey other) => string.Equals(Id, other?.Id);

        public override bool Equals(object obj) => ReferenceEquals(this, obj) || Equals(obj as VoteKey);

        public override int GetHashCode() => Id?.GetHashCode() ?? 0;
    }
}