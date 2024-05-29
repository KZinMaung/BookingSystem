using API.Services.User;
using Data.Model;
using Data.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace API.Controllers
{
   
    [ApiController]
    public class UserController : ControllerBase
    {
        IUser _iuser;
        public UserController(IUser iuser)
        {
            this._iuser = iuser;
        }

        [HttpPost("api/user/upsert")]
        public async Task<IActionResult> Upsert(tbUser data)
        {
            var result = await this._iuser.Upsert(data);
            return Ok(result);
        }

        [HttpPost("api/user/change_password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.CurrentPassword) || string.IsNullOrWhiteSpace(request.NewPassword))
            {
                return BadRequest("Invalid request.");
            }

            var result = await this._iuser.ChangePassword(request);

            if (!result)
            {
                return Unauthorized("Current password is incorrect or user does not exist.");
            }

            return Ok("Password changed successfully.");
        }

        
        
        [HttpGet("api/user/get_profile")]
        public async Task<IActionResult> GetProfile(int id)

        {
            var result = await this._iuser.GetProfile(id);
            return Ok(result);
        }


    }
}
