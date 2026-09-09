using System.Collections.Concurrent;

namespace StackOverflowLLD;
public class SearchService
{
    private readonly ConcurrentDictionary<long, Question> _questions = new();

    public void AddQuestion(Question question) => _questions.TryAdd(question.Id, question);

    public List<Question> SearchByKeyword(string keyword) =>
        _questions.Values.Where(q =>
            q.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
            q.Body.Contains(keyword, StringComparison.OrdinalIgnoreCase)).ToList();

    public List<Question> SearchByTag(string tag) =>
        _questions.Values.Where(q =>
            q.GetTags().Any(t => t.Name.Equals(tag, StringComparison.OrdinalIgnoreCase))).ToList();

    public List<Question> SearchByUser(User user) =>
        _questions.Values.Where(q => q.Author.Id == user.Id).ToList();
}
