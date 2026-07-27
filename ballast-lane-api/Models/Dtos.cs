using System;

namespace BallastLaneApi.Models
{
    public class UserDto
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string DisplayName { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class PotholeDto
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public string UserId { get; set; }
        public UserDto User { get; set; }
    }

   
}
