using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;

        public AuthController(IConfiguration config)
        {
            _config = config;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            try
            {
                var connStr = _config.GetConnectionString("DefaultConnection");
                using var conn = new MySqlConnection(connStr);
                conn.Open();

                string query = "SELECT id, email, first_name, role FROM users WHERE email = @email LIMIT 1";
                var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@email", request.Email);

                using var reader = cmd.ExecuteReader();
                if (!reader.Read())
                {
                    return Unauthorized(new { message = "Invalid email or password" });
                }

                var userId = reader.GetInt32("id");
                var email = reader.GetString("email");
                var firstName = reader.GetString("first_name");
                var role = reader.GetString("role");

                var token = GenerateJwtToken(userId, email, firstName, role);

                return Ok(new { accessToken = token, userId, email, firstName, role });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("signup")]
        public IActionResult Signup([FromBody] SignupRequest request)
        {
            try
            {
                var connStr = _config.GetConnectionString("DefaultConnection");
                using var conn = new MySqlConnection(connStr);
                conn.Open();

                string query = @"INSERT INTO users (first_name, last_name, email, password, role)
                                VALUES (@firstName, @lastName, @email, SHA2(@password, 256), 'user')";
                var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@firstName", request.FirstName ?? "");
                cmd.Parameters.AddWithValue("@lastName", request.LastName ?? "");
                cmd.Parameters.AddWithValue("@email", request.Email);
                cmd.Parameters.AddWithValue("@password", request.Password);

                cmd.ExecuteNonQuery();

                return Ok(new { message = "User created successfully" });
            }
            catch (MySqlException ex) when (ex.Message.Contains("Duplicate"))
            {
                return Conflict(new { message = "Email already exists" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("admin-login")]
        public IActionResult AdminLogin([FromBody] AdminLoginRequest request)
        {
            // Simple admin login - in production use proper credentials
            if (request.Username != "admin" || request.Password != "admin")
            {
                return Unauthorized(new { message = "Invalid admin credentials" });
            }

            var token = GenerateJwtToken(0, "admin@portal.com", "Admin", "admin");
            return Ok(new { accessToken = token, role = "admin" });
        }

        private string GenerateJwtToken(int userId, string email, string firstName, string role)
        {
            var jwtSettings = _config.GetSection("Jwt");
            var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

            var claims = new[]
            {
                new Claim("userId", userId.ToString()),
                new Claim("email", email),
                new Claim("firstName", firstName),
                new Claim("role", role)
            };

            var creds = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(int.Parse(jwtSettings["DurationInMinutes"] ?? "60")),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    public class LoginRequest
    {
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
    }

    public class SignupRequest
    {
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
    }

    public class AdminLoginRequest
    {
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
    }
}
