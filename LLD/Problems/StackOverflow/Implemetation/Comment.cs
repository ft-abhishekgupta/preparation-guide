namespace StackOverflowLLD;
public class Comment
{
    public long Id { get; }
    public User Author { get; }
    public string Body { get; }
    public DateTime CreatedAt { get; }

    public Comment(long id, User author, string body)
    {
        Id = id; Author = author; Body = body; CreatedAt = DateTime.UtcNow;
    }
}
