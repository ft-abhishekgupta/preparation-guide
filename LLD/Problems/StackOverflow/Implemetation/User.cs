namespace StackOverflowLLD;
public class User
{
    public long Id { get; }
    public string Name { get; }
    private int _reputation;
    public int Reputation => Volatile.Read(ref _reputation);

    public User(long id, string name) { Id = id; Name = name; }
    public void UpdateReputation(int amount) => Interlocked.Add(ref _reputation, amount);
}
