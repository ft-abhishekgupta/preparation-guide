using System.Collections.Concurrent;

namespace StackOverflowLLD;

public abstract class Post
{
    public long Id { get; }
    public User Author { get; }
    public string Body { get; }
    public DateTime CreatedAt { get; }

    private readonly ConcurrentDictionary<long, Vote> _votes = new();
    private readonly List<Comment> _comments = new();
    protected readonly object SyncLock = new();

    protected Post(long id, User author, string body)
    {
        Id = id; Author = author; Body = body; CreatedAt = DateTime.UtcNow;
    }

    public bool AddVote(Vote vote) => _votes.TryAdd(vote.User.Id, vote);
    public bool RemoveVote(long userId) => _votes.TryRemove(userId, out _);
    public IReadOnlyCollection<Vote> GetVotes() => _votes.Values.ToList();

    public void AddComment(Comment comment)
    {
        lock (SyncLock) _comments.Add(comment);
    }

    public IReadOnlyList<Comment> GetComments()
    {
        lock (SyncLock) return _comments.ToList();
    }
}
