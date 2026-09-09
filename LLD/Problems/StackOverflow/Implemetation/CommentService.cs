namespace StackOverflowLLD;
public class CommentService
{
    private long _commentId;

    public Comment AddComment(User user, Post post, string body)
    {
        var id = Interlocked.Increment(ref _commentId);
        var comment = new Comment(id, user, body);
        post.AddComment(comment);
        return comment;
    }
}
