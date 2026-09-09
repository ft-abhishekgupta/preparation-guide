# StackOverflow
## Requirements
```
Users can post questions, answer questions, and comment on questions and answers.
Users can vote on questions and answers.
Questions should have tags associated with them.
Users can search for questions based on keywords, tags, or user profiles.
The system should assign reputation score to users based on their activity and the quality of their contributions.
The system should handle concurrent access and ensure data consistency.
```
## Class Design
```
class User
    - id: long
    - name: string
    - reputation: int

    + UpdateReputation(amount: int)

abstract class Post
    - id: long
    - author: User
    - body: string
    - createdAt: DateTime

    - votes: ConcurrentDictionary<long, Vote>
    - comments: List<Comment>

    + AddVote(vote: Vote): bool
    + RemoveVote(userId: long): bool
    + GetVotes(): List<Vote>

    + AddComment(comment: Comment)
    + GetComments(): List<Comment>

class Question : Post
    - title: string
    - tags: List<Tag>
    - answers: List<Answer>
    - acceptedAnswer: Answer?

    + AddAnswer(answer: Answer)
    + GetAnswers(): List<Answer>

    + AcceptAnswer(answer: Answer)
    + GetAcceptedAnswer(): Answer?

class Answer : Post
    - question: Question
    - isAccepted: bool

    + MarkAccepted()

class Comment
    - id: long
    - author: User
    - body: string
    - createdAt: DateTime

class Tag
    - name: string

class Vote
    - user: User
    - type: VoteType

enum VoteType
    Upvote
    Downvote

class VotingService
    - eventPublisher: PostEventPublisher

    + Vote(
        user: User,
        post: Post,
        type: VoteType
      )
    
class CommentService
    - commentIdGenerator

    + AddComment(
        user: User,
        post: Post,
        body: string
      ): Comment

class AnswerService
    - eventPublisher: PostEventPublisher

    + AddAnswer(
        user: User,
        question: Question,
        body: string
      ): Answer

    + AcceptAnswer(
        user: User,
        answer: Answer
      )

class SearchService
    - questions: ConcurrentDictionary<long, Question>

    + AddQuestion(question: Question)

    + SearchByKeyword(
        keyword: string
      ): List<Question>

    + SearchByTag(
        tag: string
      ): List<Question>

    + SearchByUser(
        user: User
      ): List<Question>

class VoteEvent
    - post: Post
    - voter: User
    - voteType: VoteType

interface IPostEventObserver
    + OnVote(event: VoteEvent)
    + OnAnswerAccepted(answer: Answer)

class PostEventPublisher
    - observers: List<IPostEventObserver>

    + Subscribe(observer: IPostEventObserver)
    + PublishVote(event: VoteEvent)
    + PublishAnswerAccepted(answer: Answer)

class ReputationService : IPostEventObserver
    + OnVote(event: VoteEvent)
    + OnAnswerAccepted(answer: Answer)

class StackOverflowSystem
    - votingService: VotingService
    - commentService: CommentService
    - answerService: AnswerService
    - searchService: SearchService

    + CreateQuestion(user: User, title: string, body: string, tags: List<Tag>): Question
    + AddAnswer(user: User, question: Question, body: string): Answer
    + AddComment(user: User, post: Post, body: string): Comment
    + Vote(user: User, post: Post, type: VoteType)
    + AcceptAnswer(user: User, answer: Answer)
    + SearchByKeyword(keyword: string): List<Question>
    + SearchByTag(tag: string): List<Question>
    + SearchByUser(user: User): List<Question>
```
```
                         StackOverflowSystem
                                  |
          +-----------------------+-----------------------+
          |                       |                       |
          ↓                       ↓                       ↓
   VotingService          CommentService           AnswerService
          |                                               |
          |                                               |
          ↓                                               ↓
   PostEventPublisher                               Question
          |
     +----+----------------+
     |                     |
     ↓                     ↓
ReputationService   NotificationService
     |
     ↓
    User

              ┌──────────────────┐
              │       Post       │
              │    <<abstract>>  │
              └────────┬─────────┘
                       |
             +---------+---------+
             |                   |
             ↓                   ↓
        ┌──────────┐       ┌──────────┐
        │ Question │       │  Answer  │
        └────┬─────┘       └────┬─────┘
             |                  |
             | 1               | 1
             |                 |
             | *               | *
             ↓                 ↓
          Answer             Comment
             |
             *
             ↓
            Tag


Post
 ├── votes: ConcurrentDictionary<UserId, Vote>
 └── comments: List<Comment>

Question
 ├── answers: List<Answer>
 ├── tags: List<Tag>
 └── acceptedAnswer: Answer?
```