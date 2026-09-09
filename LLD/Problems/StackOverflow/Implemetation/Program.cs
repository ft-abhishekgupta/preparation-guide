using StackOverflowLLD;

var system = new StackOverflowSystem();

var abhishek = new User(1, "Abhishek");
var rahul = new User(2, "Rahul");
var csharp = new Tag("csharp");

var question = system.CreateQuestion(
    100, abhishek, "How does async await work?",
    "Can someone explain async await?", new List<Tag> { csharp });

var answer = system.AddAnswer(
    101, rahul, question,
    "async/await allows asynchronous programming.");

system.Vote(abhishek, answer, VoteType.Upvote);
system.AddComment(abhishek, answer, "Thanks, this helped!");
system.AcceptAnswer(abhishek, answer);

Console.WriteLine($"Rahul reputation: {rahul.Reputation}");
