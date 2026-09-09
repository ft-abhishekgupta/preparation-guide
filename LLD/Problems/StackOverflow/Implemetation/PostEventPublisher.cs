namespace StackOverflowLLD;
public class PostEventPublisher
{
    private readonly List<IPostEventObserver> _observers = new();

    public void Subscribe(IPostEventObserver observer) => _observers.Add(observer);

    public void PublishVote(VoteEvent voteEvent)
    {
        foreach (var observer in _observers) observer.OnVote(voteEvent);
    }

    public void PublishAnswerAccepted(Answer answer)
    {
        foreach (var observer in _observers) observer.OnAnswerAccepted(answer);
    }
}
