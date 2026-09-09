namespace StackOverflowLLD;

public class Question : Post
{
    public string Title { get; }
    private readonly List<Tag> _tags = new();
    private readonly List<Answer> _answers = new();
    private Answer? _acceptedAnswer;

    public Question(long id, User author, string title, string body, IEnumerable<Tag> tags)
        : base(id, author, body)
    {
        Title = title; _tags.AddRange(tags);
    }

    public void AddAnswer(Answer answer)
    {
        lock (SyncLock) _answers.Add(answer);
    }

    public IReadOnlyList<Answer> GetAnswers()
    {
        lock (SyncLock) return _answers.ToList();
    }

    public void AcceptAnswer(Answer answer)
    {
        lock (SyncLock)
        {
            if (!_answers.Contains(answer))
                throw new InvalidOperationException("Answer does not belong to this question.");
            if (_acceptedAnswer != null)
                throw new InvalidOperationException("Question already has an accepted answer.");

            _acceptedAnswer = answer;
            answer.MarkAccepted();
        }
    }

    public Answer? GetAcceptedAnswer()
    {
        lock (SyncLock) return _acceptedAnswer;
    }

    public IReadOnlyList<Tag> GetTags()
    {
        lock (SyncLock) return _tags.ToList();
    }
}
