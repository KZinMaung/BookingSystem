using API.Services.Auth;
using Data.ViewModel.Auth;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
   
    [ApiController]
    public class AuthController : ControllerBase
    {
         IAuth _iauth;
        public AuthController(IAuth iauth)
        {
            this._iauth = iauth;
        }

        [HttpPost("api/auth/login")]
        public async Task<IActionResult> Login(LoginRequest loginRequest)
        {
            var result = await this._iauth.Login(loginRequest);
            return Ok(result);
        }
    }
}
