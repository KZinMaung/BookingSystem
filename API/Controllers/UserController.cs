using API.Services.User;
using Data.Model;
using Data.ViewModel;
using Data.ViewModel.User;
using Microsoft.AspNetCore.Authorization;
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

        [HttpPost("api/user/register")]
        public async Task<IActionResult> Register(UserModel data)
        {
            var result = await this._iuser.Register(data);
            return Ok(result);
        }


        [Authorize]
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


        [Authorize]
        [HttpGet("api/user/get_profile")]
        public async Task<IActionResult> GetProfile(int id)

        {
            var result = await this._iuser.GetProfile(id);
            return Ok(result);
        }

        [Authorize]
        [HttpGet("api/user/verify_email")]
        public async Task<IActionResult> VerifyEmail(int id)

        {
            var result = await this._iuser.VerifyEmail(id);
            return Ok(result);
        }

        [Authorize]
        [HttpGet("api/user/check_email_exists")]
        public async Task<IActionResult> CheckEmailExists(int userID, string email)
        {
            var result = await this._iuser.CheckEmailExists(userID, email);
            return Ok(result);
        }

        [Authorize]
        [HttpGet("api/user/reset_password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
        {
            var result = await this._iuser.ResetPassword(request);
            return Ok(result);
        }

    }
}
