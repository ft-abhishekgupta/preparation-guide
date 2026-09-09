namespace StackOverflowLLD;
public class ReputationService : IPostEventObserver
{
    public void OnVote(VoteEvent voteEvent)
    {
        int change = voteEvent.Post switch
        {
            Question => voteEvent.VoteType == VoteType.Upvote ? 5 : -2,
            Answer => voteEvent.VoteType == VoteType.Upvote ? 10 : -2,
            _ => 0
        };
        voteEvent.Post.Author.UpdateReputation(change);
    }

    public void OnAnswerAccepted(Answer answer) => answer.Author.UpdateReputation(15);
}
