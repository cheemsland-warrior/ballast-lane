using Microsoft.AspNetCore.Mvc;

namespace ballast_lane_api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FallbackController : Controller
    {
        // Minimal Index action used by MapFallbackToController("Index", "Fallback")
        [HttpGet]
        public IActionResult Index()
        {
            // Return 404 by default to avoid requiring static SPA assets.
            // You can change this to return a file or redirect as needed.
            return NotFound(new { message = "Fallback endpoint reached. No content available." });
        }
    }
}
