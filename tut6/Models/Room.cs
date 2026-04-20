using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace tut6.Models
{
    public class Room
    {
        [JsonIgnore]
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "BuildingCode is required")]
        public string BuildingCode { get; set; } = string.Empty;

        public int Floor { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Capacity must be greater than zero")]
        public int Capacity { get; set; }

        public bool HasProjector { get; set; }

        public bool IsActive { get; set; } = true;
    }
}