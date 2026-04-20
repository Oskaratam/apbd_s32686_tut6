using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace tut6.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ReservationStatus
    {
        Planned,
        Confirmed,
        Cancelled
    }

    public class Reservation
    {
        [JsonIgnore]
        public int Id { get; set; }

        [Required(ErrorMessage = "RoomId is required")]
        public int RoomId { get; set; }

        [Required(ErrorMessage = "OrganizerName is required")]
        public string OrganizerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Topic is required")]
        public string Topic { get; set; } = string.Empty;

        [Required(ErrorMessage = "Date is required")]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "StartTime is required")]
        public TimeSpan StartTime { get; set; }

        [Required(ErrorMessage = "EndTime is required")]
        public TimeSpan EndTime { get; set; }

        public ReservationStatus Status { get; set; } = ReservationStatus.Planned;

        public bool OverlapsWith(Reservation other)
        {
            return Date.Date == other.Date.Date &&
                   StartTime < other.EndTime &&
                   EndTime > other.StartTime;
        }
    }
}