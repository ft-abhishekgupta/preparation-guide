# Uber (Ride Hailing)

A rider requests a ride, the system finds a nearby driver, tracks the ride through its lifecycle, and computes the fare at the end.

## Prompt

```
"Design a ride hailing system like Uber. A rider requests a ride from a pickup to a drop
location. The system matches a nearby available driver, moves the ride through its
lifecycle (assigned -> arrived -> started -> completed), and charges a fare.

Fare rules differ by ride type:

{
  "rideType": "UberGo",
  "baseFare": 50,
  "perKm": 12,
  "perMinute": 2,
  "minimumFare": 80
}

Surge pricing may apply during high demand.
Focus on ride state transitions and the pricing model."
```

## Questions

```
"Is this a single-city, in-memory system or distributed?"
> Single process, in-memory

"How do we find nearby drivers - real geo index or simple distance?"
> Simple straight-line distance is fine

"Are there multiple ride types with different pricing?"
> Yes: UberGo, UberXL, Pool

"Who can cancel and when?"
> Rider or driver, any time before the ride starts

"Do we simulate movement or does an external service push location updates?"
> External updates via updateLocation()

"Is payment in scope?"
> Only fare calculation, not gateway integration

"Do we need to handle concurrent ride requests?"
> Assume single-threaded matching for now, discuss later
```

## Requirements

```
Requirements:
1. Rider requests a ride with (riderId, pickup, drop, rideType)
2. System matches the nearest AVAILABLE driver within a max radius
3. If no driver is found, the request fails with a clear reason
4. Ride lifecycle:
   REQUESTED -> ASSIGNED -> ARRIVED -> IN_PROGRESS -> COMPLETED
   Any pre-start state -> CANCELLED
5. Invalid transitions must be rejected (cannot complete a ride that never started)
6. Fare = base + (perKm * distance) + (perMinute * duration), floored at minimumFare,
   multiplied by a surge factor
7. Each ride type has its own pricing configuration
8. Driver becomes AVAILABLE again after ride completion or cancellation

Out of scope:
- Payments, wallets, refunds
- Real map/routing/ETA services
- Driver ratings, pooling algorithm
- Persistence and distributed matching
```

## Core Entities

```
RideService     : Orchestrator (API surface)
Ride            : Aggregate root, holds state
Driver / Rider  : Actors
Location        : Value object (lat, lng)
DriverMatcher   : Strategy - how to pick a driver
PricingStrategy : Strategy - how to price a ride
FareBreakdown   : Result object
```

**Why these patterns**

| Concern | Pattern | Reason |
|---|---|---|
| Ride lifecycle | State machine | Transitions are rule-heavy; avoids a giant `switch` |
| Fare per ride type | Strategy | New ride type = new class, no edits elsewhere |
| Driver selection | Strategy | Nearest today, ML-ranked tomorrow |
| Status updates | Observer | Notify rider/driver without coupling |

## Class Design

```
class RideService:
    - rides: Map<string, Ride>
    - drivers: Map<string, Driver>
    - matcher: DriverMatcher
    - pricing: Map<RideType, PricingStrategy>

    + requestRide(riderId, pickup, drop, rideType) -> Ride
    + driverArrived(rideId) -> void
    + startRide(rideId) -> void
    + completeRide(rideId, distanceKm, durationMin) -> FareBreakdown
    + cancelRide(rideId) -> void
    + updateDriverLocation(driverId, location) -> void

class Ride:
    - id: string
    - riderId: string
    - driver: Driver | null
    - pickup: Location
    - drop: Location
    - rideType: RideType
    - status: RideStatus
    - fare: FareBreakdown | null

    + assign(driver) -> void
    + transitionTo(status) -> void      // validated by the transition table
    + getStatus() -> RideStatus

enum RideStatus:
    REQUESTED, ASSIGNED, ARRIVED, IN_PROGRESS, COMPLETED, CANCELLED

interface DriverMatcher:
    + findDriver(pickup, rideType, drivers) -> Driver | null

class NearestDriverMatcher implements DriverMatcher:
    - maxRadiusKm: double

interface PricingStrategy:
    + calculate(distanceKm, durationMin, surgeMultiplier) -> FareBreakdown

class StandardPricingStrategy implements PricingStrategy:
    - baseFare, perKm, perMinute, minimumFare: double

class FareBreakdown:
    - baseFare, distanceFare, timeFare, surgeMultiplier, total: double

class Driver:
    - id: string
    - location: Location
    - rideType: RideType
    - status: DriverStatus     // AVAILABLE, ON_TRIP, OFFLINE

class Location:
    - lat, lng: double
    + distanceTo(other) -> double
```

**State transition table**

| From | Allowed To |
|---|---|
| REQUESTED | ASSIGNED, CANCELLED |
| ASSIGNED | ARRIVED, CANCELLED |
| ARRIVED | IN_PROGRESS, CANCELLED |
| IN_PROGRESS | COMPLETED |
| COMPLETED | — |
| CANCELLED | — |

## Implementation

RideService.requestRide

```
requestRide(riderId, pickup, drop, rideType)
    driver = matcher.findDriver(pickup, rideType, drivers.values())
    if driver == null
        throw new NoDriverAvailableException()

    ride = new Ride(generateId(), riderId, pickup, drop, rideType)
    ride.assign(driver)                 // REQUESTED -> ASSIGNED
    driver.status = ON_TRIP

    rides[ride.id] = ride
    return ride
```

Ride state guard

```
ALLOWED = {
    REQUESTED   : [ASSIGNED, CANCELLED],
    ASSIGNED    : [ARRIVED, CANCELLED],
    ARRIVED     : [IN_PROGRESS, CANCELLED],
    IN_PROGRESS : [COMPLETED],
    COMPLETED   : [],
    CANCELLED   : []
}

transitionTo(next)
    if next not in ALLOWED[status]
        throw new InvalidStateTransitionException(status, next)
    status = next
```

completeRide

```
completeRide(rideId, distanceKm, durationMin)
    ride = rides[rideId]
    ride.transitionTo(COMPLETED)

    strategy = pricing[ride.rideType]
    surge = surgeProvider.getMultiplier(ride.pickup)
    ride.fare = strategy.calculate(distanceKm, durationMin, surge)

    ride.driver.status = AVAILABLE
    return ride.fare
```

NearestDriverMatcher

```
findDriver(pickup, rideType, drivers)
    best = null
    bestDistance = INFINITY

    for driver in drivers
        if driver.status != AVAILABLE: continue
        if driver.rideType != rideType: continue

        d = driver.location.distanceTo(pickup)
        if d <= maxRadiusKm && d < bestDistance
            best = driver
            bestDistance = d

    return best
```

StandardPricingStrategy

```
calculate(distanceKm, durationMin, surge)
    distanceFare = perKm * distanceKm
    timeFare     = perMinute * durationMin
    subtotal     = baseFare + distanceFare + timeFare
    subtotal     = max(subtotal, minimumFare)
    total        = subtotal * surge

    return new FareBreakdown(baseFare, distanceFare, timeFare, surge, total)
```

## Code

Location & FareBreakdown

```cs
using System;

public class Location
{
    public double Lat { get; }
    public double Lng { get; }

    public Location(double lat, double lng)
    {
        Lat = lat;
        Lng = lng;
    }

    // Equirectangular approximation - accurate enough for city-scale matching
    public double DistanceTo(Location other)
    {
        const double KmPerDegree = 111.0;
        var dLat = (Lat - other.Lat) * KmPerDegree;
        var dLng = (Lng - other.Lng) * KmPerDegree * Math.Cos(Lat * Math.PI / 180);
        return Math.Sqrt(dLat * dLat + dLng * dLng);
    }
}

public class FareBreakdown
{
    public double BaseFare { get; }
    public double DistanceFare { get; }
    public double TimeFare { get; }
    public double SurgeMultiplier { get; }
    public double Total { get; }

    public FareBreakdown(double baseFare, double distanceFare, double timeFare,
                         double surgeMultiplier, double total)
    {
        BaseFare = baseFare;
        DistanceFare = distanceFare;
        TimeFare = timeFare;
        SurgeMultiplier = surgeMultiplier;
        Total = total;
    }
}
```

Ride

```cs
using System;
using System.Collections.Generic;

public enum RideStatus { Requested, Assigned, Arrived, InProgress, Completed, Cancelled }
public enum RideType { UberGo, UberXL, Pool }

public class Ride
{
    private static readonly Dictionary<RideStatus, RideStatus[]> Allowed = new()
    {
        [RideStatus.Requested]  = new[] { RideStatus.Assigned, RideStatus.Cancelled },
        [RideStatus.Assigned]   = new[] { RideStatus.Arrived, RideStatus.Cancelled },
        [RideStatus.Arrived]    = new[] { RideStatus.InProgress, RideStatus.Cancelled },
        [RideStatus.InProgress] = new[] { RideStatus.Completed },
        [RideStatus.Completed]  = Array.Empty<RideStatus>(),
        [RideStatus.Cancelled]  = Array.Empty<RideStatus>()
    };

    public string Id { get; }
    public string RiderId { get; }
    public Location Pickup { get; }
    public Location Drop { get; }
    public RideType Type { get; }
    public Driver Driver { get; private set; }
    public RideStatus Status { get; private set; } = RideStatus.Requested;
    public FareBreakdown Fare { get; private set; }

    public Ride(string id, string riderId, Location pickup, Location drop, RideType type)
    {
        Id = id;
        RiderId = riderId;
        Pickup = pickup;
        Drop = drop;
        Type = type;
    }

    public void Assign(Driver driver)
    {
        TransitionTo(RideStatus.Assigned);
        Driver = driver;
    }

    public void SetFare(FareBreakdown fare) => Fare = fare;

    public void TransitionTo(RideStatus next)
    {
        if (Array.IndexOf(Allowed[Status], next) < 0)
        {
            throw new InvalidOperationException($"Cannot move ride from {Status} to {next}");
        }
        Status = next;
    }
}
```

Driver

```cs
public enum DriverStatus { Available, OnTrip, Offline }

public class Driver
{
    public string Id { get; }
    public RideType Type { get; }
    public Location Location { get; private set; }
    public DriverStatus Status { get; set; } = DriverStatus.Available;

    public Driver(string id, RideType type, Location location)
    {
        Id = id;
        Type = type;
        Location = location;
    }

    public void UpdateLocation(Location location) => Location = location;
}
```

Pricing

```cs
using System;

public interface IPricingStrategy
{
    FareBreakdown Calculate(double distanceKm, double durationMin, double surgeMultiplier);
}

public class StandardPricingStrategy : IPricingStrategy
{
    private readonly double _baseFare;
    private readonly double _perKm;
    private readonly double _perMinute;
    private readonly double _minimumFare;

    public StandardPricingStrategy(double baseFare, double perKm, double perMinute, double minimumFare)
    {
        _baseFare = baseFare;
        _perKm = perKm;
        _perMinute = perMinute;
        _minimumFare = minimumFare;
    }

    public FareBreakdown Calculate(double distanceKm, double durationMin, double surgeMultiplier)
    {
        var distanceFare = _perKm * distanceKm;
        var timeFare = _perMinute * durationMin;
        var subtotal = Math.Max(_baseFare + distanceFare + timeFare, _minimumFare);
        return new FareBreakdown(_baseFare, distanceFare, timeFare, surgeMultiplier,
                                 subtotal * surgeMultiplier);
    }
}
```

Matcher

```cs
using System.Collections.Generic;

public interface IDriverMatcher
{
    Driver FindDriver(Location pickup, RideType type, IEnumerable<Driver> drivers);
}

public class NearestDriverMatcher : IDriverMatcher
{
    private readonly double _maxRadiusKm;

    public NearestDriverMatcher(double maxRadiusKm) => _maxRadiusKm = maxRadiusKm;

    public Driver FindDriver(Location pickup, RideType type, IEnumerable<Driver> drivers)
    {
        Driver best = null;
        var bestDistance = double.MaxValue;

        foreach (var driver in drivers)
        {
            if (driver.Status != DriverStatus.Available || driver.Type != type)
            {
                continue;
            }

            var distance = driver.Location.DistanceTo(pickup);
            if (distance <= _maxRadiusKm && distance < bestDistance)
            {
                best = driver;
                bestDistance = distance;
            }
        }

        return best;
    }
}
```

RideService

```cs
using System;
using System.Collections.Generic;

public class RideService
{
    private readonly Dictionary<string, Ride> _rides = new();
    private readonly Dictionary<string, Driver> _drivers = new();
    private readonly Dictionary<RideType, IPricingStrategy> _pricing;
    private readonly IDriverMatcher _matcher;
    private readonly Func<Location, double> _surgeProvider;

    public RideService(IDriverMatcher matcher,
                       Dictionary<RideType, IPricingStrategy> pricing,
                       Func<Location, double> surgeProvider)
    {
        _matcher = matcher;
        _pricing = pricing;
        _surgeProvider = surgeProvider;
    }

    public void RegisterDriver(Driver driver) => _drivers[driver.Id] = driver;

    public void UpdateDriverLocation(string driverId, Location location)
        => _drivers[driverId].UpdateLocation(location);

    public Ride RequestRide(string riderId, Location pickup, Location drop, RideType type)
    {
        var driver = _matcher.FindDriver(pickup, type, _drivers.Values);
        if (driver == null)
        {
            throw new InvalidOperationException("No driver available nearby");
        }

        var ride = new Ride(Guid.NewGuid().ToString(), riderId, pickup, drop, type);
        ride.Assign(driver);
        driver.Status = DriverStatus.OnTrip;

        _rides[ride.Id] = ride;
        return ride;
    }

    public void DriverArrived(string rideId) => _rides[rideId].TransitionTo(RideStatus.Arrived);

    public void StartRide(string rideId) => _rides[rideId].TransitionTo(RideStatus.InProgress);

    public FareBreakdown CompleteRide(string rideId, double distanceKm, double durationMin)
    {
        var ride = _rides[rideId];
        ride.TransitionTo(RideStatus.Completed);

        var fare = _pricing[ride.Type].Calculate(distanceKm, durationMin, _surgeProvider(ride.Pickup));
        ride.SetFare(fare);
        ride.Driver.Status = DriverStatus.Available;

        return fare;
    }

    public void CancelRide(string rideId)
    {
        var ride = _rides[rideId];
        ride.TransitionTo(RideStatus.Cancelled);
        if (ride.Driver != null)
        {
            ride.Driver.Status = DriverStatus.Available;
        }
    }
}
```

Usage

```cs
var service = new RideService(
    new NearestDriverMatcher(maxRadiusKm: 5),
    new Dictionary<RideType, IPricingStrategy>
    {
        [RideType.UberGo] = new StandardPricingStrategy(50, 12, 2, 80),
        [RideType.UberXL] = new StandardPricingStrategy(80, 18, 3, 120)
    },
    surgeProvider: _ => 1.0);

service.RegisterDriver(new Driver("d1", RideType.UberGo, new Location(12.97, 77.59)));

var ride = service.RequestRide("r1", new Location(12.98, 77.60), new Location(12.90, 77.65), RideType.UberGo);
service.DriverArrived(ride.Id);
service.StartRide(ride.Id);
var fare = service.CompleteRide(ride.Id, distanceKm: 9.2, durationMin: 25);
// fare.Total = max(50 + 110.4 + 50, 80) * 1.0 = 210.4
```

## Extensibility

### "How would you add a new ride type, say UberBlack?"

Add the enum value and register another `StandardPricingStrategy` with different constants.
If it needs a different formula (flat airport fare, waiting charges), add a new
`IPricingStrategy` implementation. `RideService` does not change.

### "How would you handle surge pricing properly?"

Promote the `Func` to an interface:

```
interface SurgeProvider:
    getMultiplier(location, time) -> double

class DemandSurgeProvider implements SurgeProvider:
    // multiplier = f(activeRequests / availableDrivers) per geo-cell, capped at 3.0
```

Snapshot the multiplier at request time and store it on the ride, so the rider is quoted the
same number they are charged.

### "Two riders request at the same moment - how do you avoid double-assigning a driver?"

Find-and-reserve must be atomic:

1. `lock` around match + status flip inside `RideService` (simplest)
2. Per-driver CAS: `Interlocked.CompareExchange` on driver status; on failure, retry with the
   next-nearest driver
3. Partition drivers by geo-cell and lock only the cell to reduce contention

### "Linear scan will not work for a million drivers - what changes?"

Only `IDriverMatcher`. Replace the scan with a geo index - QuadTree, Geohash buckets or S2
cells - and query just the cells covering the radius. Everything else is untouched.

### "How would you notify rider and driver on each status change?"

Observer. `Ride` raises `RideStatusChanged(rideId, from, to)`; push notifications, SMS and
analytics subscribe independently.

### "What if the driver never arrives?"

Schedule a per-state timeout keyed by rideId. On expiry, auto-cancel with reason
`DRIVER_TIMEOUT`, release the driver, and re-run matching while excluding that driver.
