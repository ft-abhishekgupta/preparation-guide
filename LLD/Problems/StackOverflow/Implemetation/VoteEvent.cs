namespace StackOverflowLLD;
public class VoteEvent
{
    public Post Post { get; }
    public User Voter { get; }
    public VoteType VoteType { get; }

    public VoteEvent(Post post, User voter, VoteType voteType)
    {
        Post = post; Voter = voter; VoteType = voteType;
    }
}
