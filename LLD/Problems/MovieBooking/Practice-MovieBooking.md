## Prompt

"Design a movie ticket booking system similar to BookMyShow that allows users to browse movies, select theaters and showtimes, book tickets, and manage reservations."

## REQUIREMENTS

```
1. Movie Booking Sytem to view movies then its theatre and then its showtime
2. Admin needs to setup / manage the booking system - Movies, Theatre and Showtime, in independent operations. Only add supported.
3. Users can browse available seat numbers
4. All theatres are of same size, seat numbered rows A-Z and seat 0-20
5. Users can select multiple seats during booking, and will receive the confirmation id on success booking
6. Even if one seat is unavailable, booking request is cancelled
7. System needs to handle concurrent booking requests, only one booking should confirm in case of multiple requests, other requests should fail
8. User can cancel an existing booking via reservation management by providing confirmation id, and receive appropriate error response if uses incorrect confirmation id
9. Invalid requests should fail with apropriate error response
10. Users can search the movie by title. Search to be implemented as simple substring matching.

OUT OF SCOPE

- Payment Processing
- Distributed System
- UI Interactions
- Deletion of movies, showtime, theatres
```

## ENTITIES

1. Booking System
2. User
3. Movie
4. Theatre
5. Showtime
6. Reservation

## RELATIONSHIPS

```
BookingSystem
- List of Movie
- List of User
  User
- List of Reservation
  Movie
- List of Theatres
  Theatre
- List of Showtime
  Showtime
- Movie
- Theatre
- List of Reservation
  Reservation
- User
- Showtime
- List of Seats
```

## Class Design

```
class BookingSystem

- List<Movie>
- List<User>

- BookingSystem()
- AddMovie(Movie movie)
- AddUser(User user)
- SearchMovie(string name) -> List<Movie>
- GetMovies() -> List<Movie>
- GetMovie(string movieId) -> Movie
- BookSeats(User user, List<string> seats, Showtime showtime) -> string
- CancelReservation(User user, String confirmationId) -> void

class User

- userId: string
- email: string
- Dictionary<confirmationId, Reservation>

- GetId() -> string
- GetEmail() -> string
- AddReservation(Reservation reservation)
- DeleteReservation(Reservation reservation)
- GetReservationByConfirmationId(id: string) -> Reservation
- GetReservations() -> List<Reservation>

class Movie

- id: string
- title: string
- List<Theatre>

- AddTheatre(Theatre theatre)
- GetId()
- GetTitle()
- GetTheatres()

class Theatre

- id: string
- name: string
- List<Showtime>

- AddShowtime(Showtime showtime)
- GetShowtimes()

class Showtime

- List<Reservation>
- movie: Movie
- theatre: Theatre
- seats: List<string>
- dateTime: DateTime

- getAvailableSeats()
- IsSeatAvailable(string seatId) -> bool
- BookSeats(User user, List<string> seats) -> Reservation
- CancelReservation(Reservation reservation)

class Reservation

- user: User
- confirmationId: string
- seats: List<string>
- showtime: Showtime
- status: ReservationStatus

- cancel()
- isCancelled() -> bool

enum ReservationStatus
CONFIRMED
CANCELLED
```

## IMPLEMENTATION

```cs
class BookingSystem
{
    public string BookSeats(User user, IList<string> seats, Showtime showtime)
    {
        if (user == null)
            throw new Exception("Invalid user");

        if (showtime == null)
            throw new Exception("Invalid showtime");

        if (seats.Count == 0)
            throw new Exception("No seats selected");

        var reservation = showtime.BookSeats(user, seats);
        return reservation.confirmationId;
    }
}

class Showtime
{
    private readonly IList<Reservation> reservations;
    private readonly Movie movie;
    private readonly Theatre theatre;
    private readonly List<string> seats;
    private readonly DateTime dateTime;
    private readonly object _lock = new object();
    private HashSet<string> availableSeats;

    // BookSeats(User user, List<string> seats) -> Reservation
    public Reservation BookSeats(User user, IList<string> seats)
    {
        Reservation reservation;

        lock (_lock)
        {
            foreach (var seat in seats)
            {
                if (!IsSeatValid(seat))
                    throw new Exception("Selected seats not valid");

                if (!IsSeatAvailable(seat))
                    throw new Exception("Selected seats not available");
            }

            ReserveSeats(seats); // Removes seats from availableSeats list
            reservation = new Reservation(user, GenerateConfirmationId(), seats, this);
            user.AddReservation(reservation);
            reservations.Add(reservation);
        }

        return reservation;
    }

    public void CancelReservation(Reservation reservation)
    {
        if (reservation == null)
            throw new Exception("Invalid reservation");

        if (!reservations.Contains(reservation))
            throw new Exception("Invalid reservation - Does not belong to this showtime");

        if (reservation.Showtime != this)
            throw new Exception("Invalid reservation - Does not belong to this showtime");

        lock (_lock)
        {
            reservation.Cancel();
            ReleaseSeats(reservation.seats);
        }
    }

    private bool IsSeatAvailable(string seatId)
    {
        return availableSeats.Contains(seatId);
    }

    private void ReserveSeats(IList<string> seats)
    {
        foreach (var seat in seats)
            availableSeats.Remove(seat);
    }

    private void ReleaseSeats(IList<string> seats)
    {
        foreach (var seat in seats)
            availableSeats.Add(seat);
    }
}
```

## EXTENSIBILITY

### Supporting concurrent bookings

```cs
class Showtime
{
    private readonly IDictionary<string, object> _locks = new Dictionary<string, object>();

    Showtime(...)
    {
        ...

        foreach (var seat in availableSeats)
        {
            _locks[seat] = new object();
        }

        ...
    }

    public Reservation BookSeats(User user, IList<string> seats)
    {
        var orderedSeats = new List<string>(seats);
        orderedSeats.Sort();

        var locks = new List<object>();
        foreach (var seat in orderedSeats)
            locks.Add(_locks[seat]);

        foreach (var seatLock in locks)
            Monitor.Enter(seatLock);

        try
        {
            ... booking flow
        }
        finally
        {
            for (var index = locks.Count - 1; index >= 0; index--)
                Monitor.Exit(locks[index]);
        }
    }
}
```

### Supporing Hold Feature

```cs
private Dictionary<string, DateTime> heldSeats;

public Reservation BookSeats(User user, IList<string> seats)
{
    ...
    HoldSeats(seats);
    ...
}

private bool IsSeatAvailable(string seatId)
{
    if (heldSeats.ContainsKey(seatId) && heldSeats[seatId] > DateTime.Now.AddMinutes(5))
    {
        availableSeats.Add(seatId);
        heldSeats.Remove(seatId);
    }

    return availableSeats.Contains(seatId);
}
```

### Supporting deletion of Showtime

```cs
public void CancelShowtime(Showtime showtime)
{
    if (showtime.GetReservations().Count > 0)
    {
        // Notify and refund users. Cancel all reservations.
        ...
    }

    // Mark showtime cancelled and send a poison pill to all threads working on it.
    ...

    // Delete showtime.
    ...
}
```
