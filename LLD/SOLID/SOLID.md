# SOLID Principles

- Set of design principles to make software designs more understandable, flexible, and maintainable.
- Given by _Robert C. Martin (Uncle Bob)_

| Principle | Full Form                       | Focus         | Key Idea                                                     | Benefit                                                         |
| --------- | ------------------------------- | ------------- | ------------------------------------------------------------ | --------------------------------------------------------------- |
| SRP       | Single Responsibility Principle | Class         | One Class -> One Responsibility                              | Easier to maintain and understand                               |
| OCP       | Open/Closed Principle           | Extensibility | Open for extension but closed for modification               | Easier to add new features without breaking existing code       |
| LSP       | Liskov Substitution Principle   | Inheritance   | Subtypes must be substitutable for their base types          | Subclasses should work in place of their base classes           |
| ISP       | Interface Segregation Principle | Interface     | Clients should depend only on the methods they use           | Reduces the impact of changes and improves code maintainability |
| DIP       | Dependency Inversion Principle  | Dependencies  | Depends on abstractions rather than concrete implementations | Reduces coupling between high-level and low-level modules       |

## Single Responsibility Principle (SRP)

A class should have only one reason to change, meaning it should have only one job or responsibility.

```cs
// BEFORE
class User {
    public void CreateUser() { /* ... */ }
    public void DeleteUser() { /* ... */ }
    public void SendEmail() { /* ... */ } // Violates SRP
}
// AFTER
class User {
    public void CreateUser() { /* ... */ }
    public void DeleteUser() { /* ... */ }
}
class EmailService {
    public void SendEmail() { /* ... */ }
}
```

## Open/Closed Principle (OCP)

Software entities (classes, modules, functions, etc.) should be open for extension but closed for modification.

- Usually done by adding interface or abstract class

```cs
// BEFORE
class Payment{
    public void Pay(string type) {
        if (type == "UPI")
            Console.WriteLine("UPI");
        else if (type == "Card")
            Console.WriteLine("Card");
    }
}
// AFTER
interface IPayment {
    void Pay();
}
class UpiPayment : IPayment {
    public void Pay() {
    }
}
class CardPayment : IPayment {
    public void Pay() {
    }
}
```

## Liskov Substitution Principle (LSP)

Objects of a superclass should be replaceable with objects of a subclass without affecting the correctness of the program.

- Subclass should not break any functionality already provided by parent class

```cs
// BEFORE
class Bird {
    public virtual void Fly() {
    }
}
class Penguin : Bird {
    public override void Fly() {
        throw new Exception();
    }
}
// AFTER
class Bird {
    public void Eat() { }
}
interface IFlyingBird {
    void Fly();
}
class Sparrow : Bird, IFlyingBird {
    public void Fly() { }
}
class Penguin : Bird {
    // No Fly method, as penguins cannot fly
}
```

## Interface Segregation Principle (ISP)

Clients should not be forced to depend on interfaces they do not use. Interfaces should be specific to the needs of the client. Small, focussed Interfaces

```cs
// BEFORE
interface IWorker {
    void Code();
    void Manage();
}
class Developer : IWorker {
    public void Code() {
    }
    public void Manage() {
        throw new NotImplementedException();
    }
}
// AFTER
interface ICoder {
    void Code();
}
interface IManager {
    void Manage();
}
class Developer : ICoder {
    public void Code() {
    }
}
```

## Dependency Inversion Principle (DIP)

High-level modules should not depend on low-level modules. Both should depend on abstractions.

- Code should depend on abstraction, not concrete implementation
- Abstractions should not depend on details. Details should depend on abstractions.
- Helps in Unit Testing and Flexibility

```cs
// BEFORE
public class EmailSender {
    public void Send(string message) { /* send email */ }
}
public class NotificationService {
    private readonly EmailSender _emailSender = new();
    public void Notify(string message) {
        _emailSender.Send(message);
    }
}
// AFTER
public interface IMessageSender {
    void Send(string message);
}
public class EmailSender : IMessageSender {
    public void Send(string message) { /* send email */ }
}
public class NotificationService {
    private readonly IMessageSender _sender;
    public NotificationService(IMessageSender sender) {
        _sender = sender;
    }
    public void Notify(string message) {
        _sender.Send(message);
    }
}
```

- Both NotificationService and EmailSender depends on IMessageSender(Abstraction)

### Dependency Injection (DI)

Passing the dependency to the dependent class instead of creating it inside the class.

- Technique of achieving DIP
- This can be done via constructor injection, setter injection, or interface injection.

```cs
// Constructor Injection
class FileManager {
    private IFileReader fileReader;
    public FileManager(IFileReader fileReader) {
        this.fileReader = fileReader;
    }
    public void ReadFile() {
        fileReader.Read();
    }
}
// Setter Injection
class FileManager {
    private IFileReader fileReader;
    public void SetFileReader(IFileReader fileReader) {
        this.fileReader = fileReader;
    }
    public void ReadFile() {
        fileReader.Read();
    }
}
// Interface Injection
interface IFileReader {
    void Read();
}
```
