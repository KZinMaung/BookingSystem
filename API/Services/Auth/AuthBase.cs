using API.Services.User;
using Data.Model;
using Data.ViewModel;
using Data.ViewModel.Auth;
using Infra.UnitOfWork;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;
using System.Text;

namespace API.Services.Auth
{
    public class AuthBase : IAuth
    {
        private readonly AppDBContext _context;
        private readonly IConfiguration _configuration;
        UnitOfWork _uow;
        public AuthBase(AppDBContext context, IConfiguration configuration)
        {
            this._context = context;
            this._uow = new UnitOfWork(_context);
            this._configuration = configuration;
        }

        private bool VerifyPassword(string inputPassword, string storedHash)
        {

            return BCrypt.Net.BCrypt.Verify(inputPassword, storedHash);
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public async Task<object> Login(LoginRequest loginRequest)
        {
            ResponseModel response = new ResponseModel();
            var user = _uow.userRepo.GetAll().Where(a => a.Name == loginRequest.Username && a.IsDeleted != true).FirstOrDefault() ?? new tbUser();
            if(user.ID != 0 && VerifyPassword(loginRequest.Password, user.Password))
            {
                var token = GenerateJwtToken(loginRequest.Username);
                return new
                {
                    Token = token
                };
            }
            else
            {
                response.ReturnMessage = "Login Failed";
                return response;
            }

        }


        private string GenerateJwtToken(string username)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSettings["Secret"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
            new Claim(ClaimTypes.Name, username)
        };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
