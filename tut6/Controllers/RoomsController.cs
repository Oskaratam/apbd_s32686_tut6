using Microsoft.AspNetCore.Mvc;
using tut6.Models;

namespace tut6.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoomsController : ControllerBase
{
    private static List<Room> _rooms = new();
    private static int _nextRoomId = 1;

    static RoomsController()
    {
        InitializeData();
    }

    private static void InitializeData()
    {
        _rooms = new List<Room>
        {
            new Room { Id = _nextRoomId++, Name = "Conference Room A", BuildingCode = "A", Floor = 1, Capacity = 30, HasProjector = true, IsActive = true },
            new Room { Id = _nextRoomId++, Name = "Meeting Room B", BuildingCode = "B", Floor = 2, Capacity = 15, HasProjector = false, IsActive = true },
            new Room { Id = _nextRoomId++, Name = "Training Hall C", BuildingCode = "A", Floor = 3, Capacity = 50, HasProjector = true, IsActive = true },
            new Room { Id = _nextRoomId++, Name = "Workshop Room D", BuildingCode = "C", Floor = 1, Capacity = 20, HasProjector = true, IsActive = false },
            new Room { Id = _nextRoomId++, Name = "Seminar Room E", BuildingCode = "B", Floor = 2, Capacity = 10, HasProjector = false, IsActive = true }
        };
    }

    // GET: /api/rooms (with optional filters)
    [HttpGet]
    public IActionResult GetRooms([FromQuery] int? minCapacity, [FromQuery] bool? hasProjector, [FromQuery] bool? activeOnly)
    {
        if (!minCapacity.HasValue && !hasProjector.HasValue && !activeOnly.HasValue)
            return Ok(_rooms);

        var query = _rooms.AsEnumerable();

        if (minCapacity.HasValue)
            query = query.Where(r => r.Capacity >= minCapacity.Value);

        if (hasProjector.HasValue)
            query = query.Where(r => r.HasProjector == hasProjector.Value);

        if (activeOnly.HasValue && activeOnly.Value)
            query = query.Where(r => r.IsActive);

        return Ok(query.ToList());
    }

    // GET: /api/rooms/{id}
    [HttpGet("{id}")]
    public IActionResult GetRoomById(int id)
    {
        var room = _rooms.FirstOrDefault(r => r.Id == id);
        if (room == null)
            return NotFound(new { message = $"Room with Id {id} not found." });
        
        return Ok(room);
    }

    // GET: /api/rooms/building/{buildingCode}
    [HttpGet("building/{buildingCode}")]
    public IActionResult GetRoomsByBuilding(string buildingCode)
    {
        var rooms = _rooms.Where(r => r.BuildingCode.Equals(buildingCode, StringComparison.OrdinalIgnoreCase)).ToList();
        return Ok(rooms);
    }

    // POST: /api/rooms
    [HttpPost]
    public IActionResult CreateRoom([FromBody] Room room)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        room.Id = _nextRoomId++;
        _rooms.Add(room);
        
        return CreatedAtAction(nameof(GetRoomById), new { id = room.Id }, room);
    }

    // PUT: /api/rooms/{id}
    [HttpPut("{id}")]
    public IActionResult UpdateRoom(int id, [FromBody] Room updatedRoom)
    {
        var existingRoom = _rooms.FirstOrDefault(r => r.Id == id);
        if (existingRoom == null)
            return NotFound(new { message = $"Room with Id {id} not found." });

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        existingRoom.Name = updatedRoom.Name;
        existingRoom.BuildingCode = updatedRoom.BuildingCode;
        existingRoom.Floor = updatedRoom.Floor;
        existingRoom.Capacity = updatedRoom.Capacity;
        existingRoom.HasProjector = updatedRoom.HasProjector;
        existingRoom.IsActive = updatedRoom.IsActive;

        return Ok(existingRoom);
    }

    // DELETE: /api/rooms/{id}
    [HttpDelete("{id}")]
    public IActionResult DeleteRoom(int id)
    {
        var room = _rooms.FirstOrDefault(r => r.Id == id);
        if (room == null)
            return NotFound(new { message = $"Room with Id {id} not found." });

        var reservations = ReservationsController.GetAllReservations();
        var hasFutureReservations = reservations.Any(r => r.RoomId == id && r.Date >= DateTime.Today);
        
        if (hasFutureReservations)
            return Conflict(new { message = $"Cannot delete room with Id {id} because it has future reservations." });

        _rooms.Remove(room);
        return NoContent();
    }

    public static Room? GetRoomByIdStatic(int id)
    {
        return _rooms.FirstOrDefault(r => r.Id == id);
    }
    
    public static bool RoomExists(int id)
    {
        return _rooms.Any(r => r.Id == id);
    }
    
    public static bool IsRoomActive(int id)
    {
        return _rooms.Any(r => r.Id == id && r.IsActive);
    }
}