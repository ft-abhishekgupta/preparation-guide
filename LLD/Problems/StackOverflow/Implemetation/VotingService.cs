namespace StackOverflowLLD;
public class VotingService
{
    private readonly PostEventPublisher _publisher;
    public VotingService(PostEventPublisher publisher) => _publisher = publisher;

    public void Vote(User user, Post post, VoteType voteType)
    {
        if (post.Author.Id == user.Id)
            throw new InvalidOperationException("Cannot vote on your own post.");

        var vote = new Vote(user, voteType);
        if (!post.AddVote(vote))
            throw new InvalidOperationException("User has already voted.");

        _publisher.PublishVote(new VoteEvent(post, user, voteType));
    }
}
