namespace StackOverflowLLD;
public class AnswerService
{
    private readonly PostEventPublisher _publisher;
    public AnswerService(PostEventPublisher publisher) => _publisher = publisher;

    public Answer AddAnswer(User user, Question question, string body, long answerId)
    {
        var answer = new Answer(answerId, user, body, question);
        question.AddAnswer(answer);
        return answer;
    }

    public void AcceptAnswer(User user, Answer answer)
    {
        if (answer.Question.Author.Id != user.Id)
            throw new InvalidOperationException("Only question author can accept an answer.");

        answer.Question.AcceptAnswer(answer);
        _publisher.PublishAnswerAccepted(answer);
    }
}
