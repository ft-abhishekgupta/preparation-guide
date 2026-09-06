# Design Principles

Design principles guide in decision making to create clean, extensible, and maintainable code.

**Types**
_General Principles_

- KISS → Start simple, add complexity only when needed
- DRY → Reduce duplication, simplify maintenance
- YAGNI → Build for today, not hypothetical futures
- Separation of Concerns → Enable independent testing and changes
- Law of Demeter → Reduce coupling, hide internal structure

_OOPS/SOLID Principles_

- SRP → Keep classes focused on one responsibility
- OCP → Support future requirements without modifying existing code
- LSP → Prevent brittle hierarchies that break at runtime
- ISP → Keep interfaces clean and focused
- DIP → Decouple business logic from implementation details

## KISS - Keep It Simple, Stupid

- Start simple, single class
- Apply design pattern when required

## DRY - Don't Repeat Yourself

- Code reusability
- Shared code, Utility functions
- But can cause coupling, change in one cause other to break

## YAGNI - You Aren't Gonna Need It

- Design with extension in mind, but only implement what's needed now.

## Separation of Concerns

- Different parts of your code should handle different responsibilities, and they shouldn't know about each other's internals.

## Law of Demeter / Principle of Least Knowledge

- A method should only talk to its immediate friends, not reach through objects to access distant parts of the system
- Method chaining on same object type is fine
- For operation on different object type, create specialized methods

```
order.getCustomer().getAddress().getZipCode()
->
getCustomerZipCode() in Order Class
```

## SOLID

Used in OOP languages like Jave, C#.
Modern Language favor simpler approaches instead

- composition over class hierarchies
- functions over interfaces
