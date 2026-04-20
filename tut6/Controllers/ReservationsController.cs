using Microsoft.AspNetCore.Mvc;
using tut6.Models;

namespace tut6.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReservationsController : ControllerBase
{
    private static List<Reservation> _reservations = new();
    private static int _nextReservationId = 1;

    static ReservationsController()
    {
        _reservations = new List<Reservation>
        {
            new Reservation { Id = _nextReservationId++, RoomId = 1, OrganizerName = "John Smith", Topic = "Project Kickoff", Date = DateTime.Today.AddDays(5), StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(11, 0, 0), Status = ReservationStatus.Confirmed },
            new Reservation { Id = _nextReservationId++, RoomId = 2, OrganizerName = "Sarah Johnson", Topic = "Team Meeting", Date = DateTime.Today.AddDays(3), StartTime = new TimeSpan(14, 0, 0), EndTime = new TimeSpan(15, 30, 0), Status = ReservationStatus.Planned },
            new Reservation { Id = _nextReservationId++, RoomId = 1, OrganizerName = "Mike Brown", Topic = "Client Presentation", Date = DateTime.Today.AddDays(7), StartTime = new TimeSpan(13, 0, 0), EndTime = new TimeSpan(16, 0, 0), Status = ReservationStatus.Confirmed },
            new Reservation { Id = _nextReservationId++, RoomId = 3, OrganizerName = "Emily Davis", Topic = "Training Session", Date = DateTime.Today.AddDays(2), StartTime = new TimeSpan(10, 0, 0), EndTime = new TimeSpan(12, 0, 0), Status = ReservationStatus.Planned },
            new Reservation { Id = _nextReservationId++, RoomId = 5, OrganizerName = "Robert Wilson", Topic = "Workshop Planning", Date = DateTime.Today.AddDays(10), StartTime = new TimeSpan(15, 0, 0), EndTime = new TimeSpan(17, 0, 0), Status = ReservationStatus.Cancelled },
            new Reservation { Id = _nextReservationId++, RoomId = 1, OrganizerName = "Lisa Anderson", Topic = "Strategy Session", Date = DateTime.Today.AddDays(15), StartTime = new TimeSpan(11, 0, 0), EndTime = new TimeSpan(13, 0, 0), Status = ReservationStatus.Confirmed }
        };
    }

    // GET: /api/reservations (with optional filters)
    [HttpGet]
    public IActionResult GetReservations([FromQuery] DateTime? date, [FromQuery] string? status, [FromQuery] int? roomId)
    {
        var query = _reservations.AsEnumerable();

        if (date.HasValue)
            query = query.Where(r => r.Date.Date == date.Value.Date);
        if (roomId.HasValue)
            query = query.Where(r => r.RoomId == roomId.Value);
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<ReservationStatus>(status, true, out var statusEnum))
            query = query.Where(r => r.Status == statusEnum);

        return Ok(query.ToList());
    }

    // GET: /api/reservations/{id}
    [HttpGet("{id}")]
    public IActionResult GetReservationById(int id)
    {
        var reservation = _reservations.FirstOrDefault(r => r.Id == id);
        if (reservation == null)
            return NotFound(new { message = $"Reservation with Id {id} not found." });
        
        return Ok(reservation);
    }

    // POST: /api/reservations
    [HttpPost]
    public IActionResult CreateReservation([FromBody] Reservation reservation)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (reservation.EndTime <= reservation.StartTime)
            return BadRequest(new { message = "EndTime must be later than StartTime." });

        if (!RoomsController.RoomExists(reservation.RoomId))
            return BadRequest(new { message = $"Room with Id {reservation.RoomId} does not exist." });

        if (!RoomsController.IsRoomActive(reservation.RoomId))
            return Conflict(new { message = $"Cannot create reservation for inactive room with Id {reservation.RoomId}." });

        var overlappingReservation = _reservations.FirstOrDefault(r =>
            r.RoomId == reservation.RoomId && r.OverlapsWith(reservation) && r.Status != ReservationStatus.Cancelled);

        if (overlappingReservation != null)
            return Conflict(new { message = "Time slot overlaps with existing reservation." });

        reservation.Id = _nextReservationId++;
        _reservations.Add(reservation);
        
        return CreatedAtAction(nameof(GetReservationById), new { id = reservation.Id }, reservation);
    }

    // PUT: /api/reservations/{id}
    [HttpPut("{id}")]
    public IActionResult UpdateReservation(int id, [FromBody] Reservation updatedReservation)
    {
        var existingReservation = _reservations.FirstOrDefault(r => r.Id == id);
        if (existingReservation == null)
            return NotFound(new { message = $"Reservation with Id {id} not found." });

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (updatedReservation.EndTime <= updatedReservation.StartTime)
            return BadRequest(new { message = "EndTime must be later than StartTime." });

        if (!RoomsController.RoomExists(updatedReservation.RoomId))
            return BadRequest(new { message = $"Room with Id {updatedReservation.RoomId} does not exist." });

        if (!RoomsController.IsRoomActive(updatedReservation.RoomId))
            return Conflict(new { message = $"Cannot update reservation for inactive room with Id {updatedReservation.RoomId}." });

        var overlappingReservation = _reservations.FirstOrDefault(r =>
            r.Id != id && r.RoomId == updatedReservation.RoomId && r.OverlapsWith(updatedReservation) && r.Status != ReservationStatus.Cancelled);

        if (overlappingReservation != null)
            return Conflict(new { message = "Time slot overlaps with existing reservation." });

        existingReservation.RoomId = updatedReservation.RoomId;
        existingReservation.OrganizerName = updatedReservation.OrganizerName;
        existingReservation.Topic = updatedReservation.Topic;
        existingReservation.Date = updatedReservation.Date;
        existingReservation.StartTime = updatedReservation.StartTime;
        existingReservation.EndTime = updatedReservation.EndTime;
        existingReservation.Status = updatedReservation.Status;

        return Ok(existingReservation);
    }

    // DELETE: /api/reservations/{id}
    [HttpDelete("{id}")]
    public IActionResult DeleteReservation(int id)
    {
        var reservation = _reservations.FirstOrDefault(r => r.Id == id);
        if (reservation == null)
            return NotFound(new { message = $"Reservation with Id {id} not found." });

        _reservations.Remove(reservation);
        return NoContent();
    }

    public static List<Reservation> GetAllReservations()
    {
        return _reservations;
    }
}