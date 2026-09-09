namespace StackOverflowLLD;
public class StackOverflowSystem
{
    private readonly VotingService _votingService;
    private readonly CommentService _commentService;
    private readonly AnswerService _answerService;
    private readonly SearchService _searchService;

    public StackOverflowSystem()
    {
        var publisher = new PostEventPublisher();
        publisher.Subscribe(new ReputationService());

        _votingService = new VotingService(publisher);
        _commentService = new CommentService();
        _answerService = new AnswerService(publisher);
        _searchService = new SearchService();
    }

    public Question CreateQuestion(long id, User user, string title, string body, List<Tag> tags)
    {
        var question = new Question(id, user, title, body, tags);
        _searchService.AddQuestion(question);
        return question;
    }

    public Answer AddAnswer(long id, User user, Question question, string body) =>
        _answerService.AddAnswer(user, question, body, id);

    public Comment AddComment(User user, Post post, string body) =>
        _commentService.AddComment(user, post, body);

    public void Vote(User user, Post post, VoteType voteType) =>
        _votingService.Vote(user, post, voteType);

    public void AcceptAnswer(User user, Answer answer) =>
        _answerService.AcceptAnswer(user, answer);

    public List<Question> SearchByKeyword(string keyword) =>
        _searchService.SearchByKeyword(keyword);

    public List<Question> SearchByTag(string tag) =>
        _searchService.SearchByTag(tag);

    public List<Question> SearchByUser(User user) =>
        _searchService.SearchByUser(user);
}
