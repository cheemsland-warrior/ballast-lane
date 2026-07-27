namespace BallastLaneApi.Models
{
    public class PotholeToCreateDto
    {
        public string Description { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Status { get; set; } = "Reported";
        public string UserId { get; set; }
    }
}
