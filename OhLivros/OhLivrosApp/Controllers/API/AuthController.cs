using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OhLivrosApp.Controllers.Api
{
    public record LoginRequest(string Email, string Password);

    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _users;
        private readonly SignInManager<IdentityUser> _signIn;
        private readonly IConfiguration _config;

        public AuthController(UserManager<IdentityUser> users, SignInManager<IdentityUser> signIn, IConfiguration config)
        {
            _users = users;
            _signIn = signIn;
            _config = config;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginRequest req)
        {
            var user = await _users.FindByEmailAsync(req.Email);
            if (user == null) return Unauthorized();

            var ok = await _signIn.CheckPasswordSignInAsync(user, req.Password, lockoutOnFailure: false);
            if (!ok.Succeeded) return Unauthorized();

            var roles = await _users.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(ClaimTypes.Name, user.UserName ?? req.Email),
                new Claim(JwtRegisteredClaimNames.Email, req.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            var jwt = _config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwt["Issuer"],
                audience: jwt["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds);

            return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
        }
    }
}
