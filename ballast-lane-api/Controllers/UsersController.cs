using AutoMapper;
using BallastLaneApi.Data;
using BallastLaneApi.Models;
using BallastLaneApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace BallastLaneApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _users;
        private readonly IMapper _mapper;
        private readonly IAuthService _auth; // add this
        private readonly IConfiguration _config; // add this

        public UsersController(IUserService users, IMapper mapper, IAuthService auth, IConfiguration config)
        {
            _users = users;
            _mapper = mapper;
            _auth = auth;
            _config = config;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var users = await _users.GetAllAsync();
            var dtos = _mapper.Map<List<UserDto>>(users);
            return Ok(dtos);
        }
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequest req)
        {
            try
            {
                var user = new User { Email = req.Email, DisplayName = req.DisplayName };
                var created = await _users.CreateAsync(user, req.Password);

                var dto = _mapper.Map<UserDto>(created);

                // Authenticate immediately and get token
                var authResult = await _auth.AuthenticateAsync(req.Email, req.Password);
                // authResult.Token is assumed to be the JWT (adjust to your AuthResponse)

                // Return 201 with user DTO and token
                var response = new
                {
                    user = dto,
                    token = authResult.Token
                };

                return CreatedAtAction(nameof(Get), new { id = created.Id }, response);
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

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
                return BadRequest("Email and password required.");

            var user = await _users.AuthenticateAsync(req.Email, req.Password);
            if (user == null) return Unauthorized();

            var secret = _config["Jwt:Key"];
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenKey = Encoding.UTF8.GetBytes(secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Email, user.Email)
                }),
                Expires = DateTime.UtcNow.AddHours(8),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(tokenKey), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwt = tokenHandler.WriteToken(token);

            return Ok(new { Id = user.Id, token = jwt });
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            // If cookie authentication is used, this will sign out the cookie.
            // For JWT bearer tokens (stateless) there is no server-side session to clear,
            // so clients should simply discard the token. Keep SignOutAsync for cookie support.
            await HttpContext.SignOutAsync();
            return NoContent();
        }
    }
}
