namespace StackOverflowLLD;
public interface IPostEventObserver
{
    void OnVote(VoteEvent voteEvent);
    void OnAnswerAccepted(Answer answer);
}
