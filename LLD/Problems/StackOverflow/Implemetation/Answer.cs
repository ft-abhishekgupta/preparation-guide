namespace StackOverflowLLD;

public class Answer : Post
{
    public Question Question { get; }
    private bool _isAccepted;

    public bool IsAccepted
    {
        get { lock (SyncLock) return _isAccepted; }
    }

    public Answer(long id, User author, string body, Question question)
        : base(id, author, body) => Question = question;

    internal void MarkAccepted() => _isAccepted = true;
}
