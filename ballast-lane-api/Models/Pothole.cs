using System.Text.Json.Serialization;

namespace BallastLaneApi.Models
{
    public class Pothole
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Status { get; set; } = "Reported"; // e.g., Reported, In Progress, Fixed
        public DateTime CreatedDate { get; set; }

        // Relationship config
        public string UserId { get; set; }
        public User User { get; set; }
    }
}