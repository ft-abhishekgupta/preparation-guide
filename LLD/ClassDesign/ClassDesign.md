# Class Design

## Entities

- Find nouns in the system
- Check if it has its own state and behaviour or not

## Relationships

| Relationship | Meaning                                        | Example                                   |
| ------------ | ---------------------------------------------- | ----------------------------------------- |
| Association  | A **knows/works with** B; both are independent | `Teacher` works with `Student`            |
| Aggregation  | A **has** B; B can exist independently         | `Department` has externally created staff |
| Composition  | A **owns** B and controls its lifetime         | `Classroom` creates and owns its desks    |
| Dependency   | A temporarily **uses** B                       | A method receives B as a parameter        |
| Realization  | A **implements** interface B                   | `EmailNotifier` implements `INotifier`    |
| Inheritance  | A **is a** B                                   | `GraduateStudent` derives from `Student`  |

```
For a has-a relationship, can the child exist independently?
       |
       YES ───→ Aggregation
       |
       NO  ───→ Composition
```

## C# Example

```csharp
public static class Program
{
 public static void Main()
 {
  var student = new Student("Ava");
  Student graduateStudent = new GraduateStudent("Liam"); // Inheritance
  var teacher = new Teacher("Noah");

  teacher.Teach(student);                                // Association
  teacher.Teach(graduateStudent);

  var department = new Department();
  department.AddTeacher(teacher);                        // Aggregation

  var classroom = new Classroom(deskCount: 20);          // Composition

  INotifier notifier = new EmailNotifier();              // Realization
  var enrollmentService = new EnrollmentService();
  enrollmentService.Enroll(student, notifier);           // Dependency
 }
}
```

## Properties

- Class tracks what
- Class owns what

## Method

- Class does what
- Getters and Setters, not needed explicitly

```cs
public string id {get; private set;}
```

- Only create methods that actually serve some requirement
