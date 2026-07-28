using BallastLaneApi.Data;
using BallastLaneApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

namespace BallastLaneApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PotholesController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly BallastLaneApi.Services.IPotholeService _potholes;
        private readonly IMapper _mapper;

        public PotholesController(ApplicationDbContext db, BallastLaneApi.Services.IPotholeService potholes, IMapper mapper)
        {
            _db = db;
            _potholes = potholes;
            _mapper = mapper;
        }

        // GET: api/potholes (Public list of all potholes)
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var potholes = await _potholes.GetAllAsync();
            var dtos = _mapper.Map<List<PotholeDto>>(potholes);
            return Ok(dtos);
        }

        // GET: api/potholes/{id} (For the detail view page)
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var pothole = await _potholes.GetByIdAsync(id);
            if (pothole == null) return NotFound();
            var dto = _mapper.Map<PotholeDto>(pothole);
            return Ok(dto);
        }

        // POST: api/potholes (Create new location marker)
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] PotholeToCreateDto pothole)
        {
            try
            {
                // Ensure the user is the authenticated user
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrWhiteSpace(userId)) return Forbid();
                pothole.UserId = userId;
                var item = _mapper.Map<Pothole>(pothole); item.Id = userId;

                var created = await _potholes.CreateAsync(item);
                var dto = _mapper.Map<PotholeDto>(created);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, dto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }


        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete([FromRoute] string id)
        {
            try
            {
                // Ensure the user is the authenticated user
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrWhiteSpace(userId)) return Forbid();


                await _potholes.DeleteAsync(id);

                return Ok();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }
    }
}