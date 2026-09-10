# Generic Problems

For LLD interviews, you can memorize the following **problem → core relationships → design pattern** cheat sheet. The goal is not to memorize every class, but to quickly identify **"who owns/uses/contains whom"**.

## 1. Parking Lot

```text
ParkingLot
 ├── * ParkingFloor
 │      └── * ParkingSpot
 │
 ├── * Entrance
 └── * Exit

Vehicle ──> ParkingSpot
Ticket ──> Vehicle + ParkingSpot
Payment ──> Ticket
```

**Patterns:**

- **Strategy** → pricing/payment calculation
- **Factory** → create different vehicle/spot types
- **Observer** → available spot notifications (optional)

---

# 2. Elevator System

```text
ElevatorSystem
 └── * Elevator
        ├── ElevatorState
        └── ElevatorDoor

Floor
 └── ElevatorRequest

ElevatorSystem ──> SchedulingStrategy
```

**Patterns:**

- **State** → Idle / Moving / Maintenance
- **Strategy** → elevator selection/scheduling
- **Command** → elevator requests (optional)

---

# 3. Vending Machine

```text
VendingMachine
 ├── * Product
 ├── Inventory
 ├── Payment
 └── VendingMachineState

VendingMachine ──> VendingMachineState
```

**Patterns:**

- ⭐ **State** → Idle, HasMoney, Dispensing, etc.
- **Strategy** → payment/change calculation
- **Factory** → product/payment creation (optional)

---

# 4. Library Management

```text
Library
 ├── BookCatalog
 │      └── * Book
 │
 ├── * Member
 │      └── * Loan
 │              └── Book
 │
 └── BorrowingPolicy
```

**Patterns:**

- **Strategy** → borrowing policies
- **Factory** → book/item creation (optional)
- **Observer** → due-date notifications (optional)

---

# 5. Movie Ticket Booking

```text
Movie
 └── * Showtime
        └── * Seat

Theater
 └── * Screen
        └── * Seat

Booking
 ├── User
 ├── Showtime
 └── * Seat
```

Important relationship:

```text
Booking ──> Showtime ──> Seats
```

**Patterns:**

- **Strategy** → pricing
- **State** → booking/payment state
- **Observer** → booking notifications
- **Factory** → payment method

**Critical:** Seat booking must be **thread-safe/atomic**.

---

# 6. Car Rental System

```text
RentalSystem
 ├── * Vehicle
 ├── * Location
 ├── * Reservation
 │       ├── User
 │       └── Vehicle
 └── Payment
```

```text
Vehicle
 └── VehicleStatus
```

**Patterns:**

- **Strategy** → pricing
- **Factory** → vehicle creation
- **State** → Available / Reserved / Rented / Maintenance

---

# 7. Hotel Booking

```text
Hotel
 └── * Room

Room
 └── RoomType

Reservation
 ├── User
 ├── Room
 └── Payment
```

**Patterns:**

- **Strategy** → pricing
- **State** → room availability
- **Factory** → room/reservation creation
- **Observer** → notifications

---

# 8. Food Delivery

```text
User
 └── * Order
        ├── * OrderItem
        │      └── RestaurantItem
        ├── Restaurant
        ├── Payment
        └── Delivery

Restaurant
 └── * MenuItem

Delivery
 └── DeliveryPartner
```

**Patterns:**

- **State** → Order lifecycle
- **Strategy** → delivery partner assignment
- **Strategy** → pricing
- **Observer** → order status notifications

---

# 9. Ride Sharing / Uber

```text
User
 ├── Rider
 └── Driver

Ride
 ├── Rider
 ├── Driver
 └── Location

RideService
 └── MatchingStrategy
```

**Patterns:**

- ⭐ **Strategy** → driver matching
- **State** → Requested → Accepted → Started → Completed
- **Observer** → ride status
- **Factory** → ride creation

---

# 10. ATM

```text
ATM
 ├── CardReader
 ├── CashDispenser
 ├── BankService
 └── ATMState

Transaction
 ├── Withdrawal
 ├── Deposit
 └── BalanceInquiry
```

**Patterns:**

- ⭐ **State** → CardInserted / Authenticated / etc.
- **Strategy** → cash dispensing algorithm
- **Chain of Responsibility** → denomination dispensing
- **Command** → transactions

---

# 11. Splitwise

```text
User
 └── * Expense

Expense
 ├── PaidBy → User
 └── * Split

Split
 └── User

Group
 └── * User
```

**Patterns:**

- ⭐ **Strategy** → Equal / Exact / Percentage split
- **Factory** → split creation

```text
ISplitStrategy
 ├── EqualSplit
 ├── ExactSplit
 └── PercentageSplit
```

---

# 12. Chess

```text
Game
 ├── Board
 │      └── * Cell
 │             └── Piece
 │
 ├── Player
 └── GameState
```

```text
Piece
 ├── King
 ├── Queen
 ├── Rook
 ├── Bishop
 └── Knight
```

**Patterns:**

- **Strategy** → movement rules
- **State** → game state
- **Command** → moves / undo
- **Factory** → piece creation

---

# 13. Tic-Tac-Toe

```text
Game
 ├── Board
 │      └── * Cell
 └── * Player

Player
 └── Symbol
```

**Patterns:**

- **Strategy** → winning strategy
- **State** → game state
- **Factory** → player creation

---

# 14. Snake & Ladder

```text
Game
 ├── Board
 │      └── * Cell
 ├── * Player
 └── Dice

Board
 ├── * Snake
 └── * Ladder
```

**Patterns:**

- **Strategy** → dice strategy
- **State** → game state
- **Factory** → board/player creation

---

# 15. Deck of Cards

```text
Deck
 └── * Card

Card
 ├── Suit
 └── Rank

Game
 └── Deck
```

For Blackjack:

```text
Game
 ├── Dealer
 ├── * Player
 └── Deck
```

**Patterns:**

- **Strategy** → scoring
- **Factory** → card/deck creation

---

# 16. Logger

```text
Logger
 └── LogHandler
        └── next → LogHandler
```

```text
LogHandler
 ├── ConsoleHandler
 ├── FileHandler
 └── DatabaseHandler
```

**Pattern:**

- ⭐ **Chain of Responsibility**

Also:

- **Singleton** → logger instance, if explicitly required
- **Factory** → handler creation

---

# 17. Notification System

```text
NotificationService
 └── NotificationSender

NotificationSender
 ├── EmailSender
 ├── SMSSender
 └── PushSender
```

**Patterns:**

- ⭐ **Strategy** → notification channel
- **Factory** → sender creation
- **Observer** → event-based notifications

```text
INotificationSender
 ├── Email
 ├── SMS
 └── Push
```

---

# 18. File System

```text
FileSystem
 └── Directory

Directory
 └── * FileSystemEntity

FileSystemEntity
 ├── File
 └── Directory
```

**Pattern:**

- ⭐ **Composite**

Because:

```text
Directory
 ├── File
 ├── File
 └── Directory
       ├── File
       └── File
```

---

# 19. Coffee Machine

```text
CoffeeMachine
 ├── Inventory
 ├── Payment
 └── CoffeeMachineState

Coffee
 ├── Espresso
 ├── Latte
 └── Cappuccino
```

**Patterns:**

- ⭐ **State**
- **Strategy** → pricing
- ⭐ **Factory** → coffee creation

---

# 20. Meeting Room Booking

```text
Building
 └── * Room

Room
 └── * Booking

Booking
 ├── User
 ├── Room
 └── TimeSlot
```

**Patterns:**

- **Strategy** → room selection
- **Observer** → notifications
- **State** → booking status

**Important:** concurrent booking requires locking/atomic reservation.

---

# 21. Shopping Cart / E-Commerce

```text
User
 └── Cart
       └── * CartItem
              └── Product

Order
 └── * OrderItem
        └── Product

Order
 ├── Payment
 └── Shipment
```

**Patterns:**

- ⭐ **State** → Order lifecycle
- **Strategy** → pricing/discount
- **Strategy** → payment
- **Factory** → payment method
- **Observer** → order notifications

---

# 22. Stock Trading System

```text
TradingSystem
 ├── * User
 ├── OrderBook
 │      ├── BuyOrders
 │      └── SellOrders
 └── * Order

Order
 └── User
```

**Patterns:**

- ⭐ **Strategy** → order matching
- **State** → order lifecycle
- **Observer** → price/order notifications
- **Command** → place/cancel order

Concurrency is particularly important here.

---

# 23. Cache — LRU / LFU

### LRU

```text
LRUCache
 ├── Dictionary<Key, Node>
 └── LinkedList<Node>
```

**Pattern:**

- Usually **no formal GoF pattern**
- **Strategy** can be used if supporting multiple eviction policies.

```text
IEvictionPolicy
 ├── LRUPolicy
 ├── LFUPolicy
 └── FIFOPolicy
```

---

# 24. Parking / Vending / Elevator — remember these three

These are extremely common because they test different concepts:

```text
Parking Lot
→ Strategy + Factory

Vending Machine
→ ⭐ State + Strategy

Elevator
→ ⭐ State + Strategy
```

---

# Pattern → Problems Cheat Sheet

This is probably the **most useful thing to memorize** before an LLD interview.

| Pattern                        | When to identify it                    | Common LLD                                 |
| ------------------------------ | -------------------------------------- | ------------------------------------------ |
| ⭐ **Strategy**                | Multiple interchangeable algorithms    | Parking, Uber, Splitwise, Payment, Pricing |
| ⭐ **State**                   | Object behavior changes based on state | Vending, Elevator, ATM, Order              |
| ⭐ **Factory**                 | Object creation varies                 | Payment, Vehicle, Notification             |
| ⭐ **Observer**                | One event → many subscribers           | Notification, Stock, Order                 |
| ⭐ **Composite**               | Tree / part-whole hierarchy            | File System                                |
| ⭐ **Chain of Responsibility** | Request passes through handlers        | Logger, ATM                                |
| **Command**                    | Encapsulate an action/request          | Chess, ATM, Trading                        |
| **Decorator**                  | Dynamically add behavior               | Coffee, Pizza, Notifications               |
| **Adapter**                    | Make incompatible interfaces work      | Payment integrations                       |
| **Template Method**            | Same algorithm, varying steps          | Payment/processing workflows               |
| **Singleton**                  | Exactly one shared instance required   | Logger, Configuration                      |

## The relationship patterns you should memorize

When designing any LLD, first ask these five questions:

```text
1. IS-A?
   ↓
Inheritance

2. HAS-A?
   ↓
Composition / Aggregation

3. USES?
   ↓
Association / Dependency

4. MANY implementations of same behavior?
   ↓
Interface + Strategy

5. Object behavior changes based on state?
   ↓
State Pattern
```

And the **most common relationship structure** you'll see is:

```text
Service
   │
   ├── manages → Entities
   │
   ├── uses → Repository
   │
   ├── uses → Strategy
   │
   └── notifies → Observers
```

For your interview preparation, I'd prioritize **Strategy, State, Factory, Observer, Composite, and Chain of Responsibility**. Those six cover a very large percentage of the pattern-oriented LLD questions you're likely to encounter.
