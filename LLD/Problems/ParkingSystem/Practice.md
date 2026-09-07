## Prompt

"Design a parking lot system where different types of vehicles can park. The system manages spot assignment and calculates fees upon exit."

## CLARIFYING QUESTIONS

- How many different types of vehicles are there. Is it fixed or configurable. Are the parking spot also of different types

> Motorcycle, Car, Large
> Same types for parking spot

- Can small size vehicle be assigned large parking spots if exact small size is unavailable

> Type specific spots

- Are there different floors in parking area? Are the number of parking spots fixed per floor or in total?

> Multiple floors, Total spots fixed. Predefined different number/types per floor

- What happens if the parking spots are not available

> Reject

- Are the vehicles assigned parking tickets?

> Yes

- Are the fees calculation fixed?

> Hourly Rate, Rounded to nearest hour, Same rate for all vehicles, rate fixed at init

- Is there an expiry of parking time?

> No

- Are there multiple entry/exit gates

> Can be, but Concurrency handling not needed

## REQUIREMENTS

- Design a parking system with predefined number of floors, parking spots per floor, hourly rate
- 3 Types of vehicle and parking spots needs to be supported - Motorcycle, Car, Large Vehicle.
- Vehicle and parking spot type should be exactly same to be compatible
- If the specific type of parking spot is unavailable, request should be rejected
- On entry, vehicle should be assigned parking ticket, spot to park - Floor Number and Parking Spot Number
- On exit, parking ticket should be used to calculate the fees, and make the parking spot available
- On invalid parking ticket on exit, system should throw exception with correct error message
- There can be only one vehicle parked at a parking spot at a time

## OUT OF SCOPE

- Payment System
- Concurrency
- Parking Expiry

## ENTITIES

- ParkingSystem : Orchestrator
- ParkingSpot
- ParkingTicket
- Vehicle

## ENUMS

- Type: Motorcycle, Car, HeavyVehicle
- Status: Available, Unavailable

## RELATIONSHIPS

```
ParkingSystem has ParkingSpots
ParkingSpot knows Vehicle
Vehicle has ParkingTicket
```

## CLASS DESIGN

```cs
class ParkingSystem

- IList<ParkingSpot> parkingSpots
- static hourlyRate
- Dictionary<string, ParkingTicket> activeTickets

- Entry(Vehicle vehicle) -> ParkingTicket
- Exit(Vehicle vehicle) -> int

class ParkingSpot

- int floor
- int spotNumber
- Type type
- Status status
- Vehicle vehicle

- GetFloor()
- GetSpotNumber()
- GetSpotType()
- IsAvailable() -> bool
- Reserve(Vehicle vehicle) -> ParkingTicket
- MakeAvailable() -> void
- GetVehicle() -> Vehicle

class ParkingTicket

- string id
- DateTime dataTime
- int parkingFloor
- int parkingSpot

class Vehicle

- Type type
- ParkingTicket parkingTicket

- GetVehicleType()
- GetParkingTicket()
- AddParkingTicket(ParkingTicket parkingTicket) -> void

enum Type

- Motorcycle
- Car
- HeavyVehicle

enum Status

- Available
- Unavailable
```

## IMPLEMENTATION

```cs
class ParkingSystem {
private readonly IList<ParkingSpot> parkingSpots;
private readonly Dictionary<string, (ParkingTicket pt, ParkingSpot ps)> activeTickets;
private static hourlyRate = 10;

    public ParkingTicket Entry(Vehicle vehicle){
        var type = vehicle.GetVehicleType();
        var availableSpot = GetAvailableSpotByType(type);
        if(!availableSpot)
            throw new NoSpotAvailableException("No spots available")
        var ticket = availableSpot.Reserve(vehicle);
        activeTickets[ticket.getId()] = (ticket, availableSpot);
        return ticket;
    }
    public int Exit(string ticketId){
        if(!activeTickets.Contains(ticketId))
            throw new InvalidTicketException("Invalid ticket");
        var fees = GetTotalFees(ticket.dateTime, DateTime.UtcNow());
        activeTickets[ticketId].ps.MakeAvailable();
        activeTickets.Remove(ticketId);
        return fees;
    }
    private ParkingSpot GetAvailableSpotByType(Type type){
        foreach(var ps in parkingSpots)
            if(ps.IsAvailable() && ps.GetSpotType() == type)
                return ps;
        return null;
    }
    private int GetTotalFees(DateTime entryDt, DateTime exitDt){
        int hours = (int)Math.Ceiling((exitDt - entryDt).TotalHours);
        return hours * hourlyRate;
    }

}
```

## EXTENSIBILITY

### Supporting different spot allocation strategies

Strategy interface - new strategies can be added without modifying `ParkingSystem`.

```cs
ISpotAllocationStrategy
FindSpot(ISortedDictionary<string, ParkingFloor> floors, Type vehicleType) -> ParkingSpot

NearestToEntryStrategy : ISpotAllocationStrategy
FindSpot(ISortedDictionary<string, ParkingFloor> floors, Type vehicleType) -> ParkingSpot

FloorWiseFillStrategy : ISpotAllocationStrategy
FindSpot(ISortedDictionary<string, ParkingFloor> floors, Type vehicleType) -> ParkingSpot

ParkingSystem

- ISortedDictionary<string, ParkingFloor> floorNumberToFloorMap
- ISpotAllocationStrategy allocationStrategy // injected at construction

ParkingFloor

- ISortedDictionary<string, ParkingSpot> spotNumberToSpotMap
  GetNearestAvailableSpot(int spotNumber, Type type)
  GetFirstAvailableSpot(Type type)
```

### Supporting different rates by vehicle type

```cs
class ParkingSystem
...
IDictionary<Type, int> perTypeRate;

class ParkingTicket
...

- ratePerHour: int

var fees = GetTotalFees(ticket.dateTime, DateTime.UtcNow(), ticket.ratePerHour);

private int GetTotalFees(DateTime entryDt, DateTime exitDt, int rate)
```

### Preventing race conditions during spot allocation

```cs
class ParkingSpot{
private readonly object \_lock = new object();

    public ParkingTicket Reserve(Vehicle vehicle){
        lock (_lock){
            if(IsAvailable()){
                setStatusUnavailable();
                var ticket = CreateNewParkingTicket(vehicle)
                vehicle.AddParkingTicket(ticket);
                return ticket;
            }
            else throw new NoSpotAvailableException("Spot unavailable")
        }
    }

}
```

However, another request can still fail after selecting the same spot. To prevent this, add the lock in `ParkingSystem` around `findAndReserveSpot() { lock (_lock) }`.
