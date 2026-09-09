namespace StackOverflowLLD;
public class Vote
{
    public User User { get; }
    public VoteType Type { get; }
    public Vote(User user, VoteType type) { User = user; Type = type; }
}
