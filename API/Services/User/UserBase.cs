using Azure;
using Core.Extension;
using Data.Model;
using Data.ViewModel;
using Data.ViewModel.User;
using Infra.UnitOfWork;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Security.Cryptography.Xml;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace API.Services.User
{
    public class UserBase : IUser
    {
        private readonly AppDBContext _context;
        UnitOfWork _uow;
        public UserBase(AppDBContext context)
        {
            this._context = context;
            this._uow = new UnitOfWork(_context);
        }

        private async Task<tbUser> GetByID(int id)
        {
            tbUser user = await _uow.userRepo.GetAll().Where(a => a.ID == id && a.IsDeleted != true).FirstOrDefaultAsync() ?? new tbUser();
            return user;
        }
        public async Task<tbUser> Upsert(tbUser data)
        {

            if (data.ID > 0)
            {
                tbUser user = _uow.userRepo.GetById(data.ID);
                user.Name = data.Name;
                user.Email = data.Email;
                user.AccessTime = MyExtension.getLocalTime();
                data = await _uow.userRepo.UpdateAsync(user);
            }
            else
            {
                data.Password = HashPassword(data.Password);
                data.CreatedAt = MyExtension.getLocalTime();
                data.AccessTime = MyExtension.getLocalTime();
                data.IsDeleted = false;
                data.IsEmailVerified = false;
                data = await _uow.userRepo.InsertReturnAsync(data);
            }
            data.Password = "";
            return data;
        }

        public async Task<ResponseModel> Register(UserModel data)
        {
            ResponseModel response = new ResponseModel();
            tbUser user = await _uow.userRepo.GetAll().Where(a => a.Name == data.Name && a.IsDeleted != true && a.IsEmailVerified != false).FirstOrDefaultAsync() ?? new tbUser();
            if (user.ID != 0)
            {
                response.ReturnMessage = "Username has been already existed.";
                
            }
            else
            {
                tbUser entity = new tbUser
                {
                    Name = data.Name,
                    Email = data.Email,
                    Password = HashPassword(data.Password),
                    IsEmailVerified = false,
                    CreatedAt = DateTime.Now,
                    AccessTime = DateTime.Now,
                    IsDeleted = false
                };
                var result = await _uow.userRepo.InsertReturnAsync(entity);
                if(result != null)
                {
                    bool isSent = SendVerifyEmail();
                    if (isSent)
                    {
                        response.ReturnMessage = "You have been successfully registered and verify your email.";
                    }
                }
                else
                {
                    response.ReturnMessage = "Register Failed!";

                }
            }
            return response;
        }

        private bool SendVerifyEmail()
        {
            return true;
        }


        //for profile
        public async Task<tbUser> GetProfile(int ID)
        {
            tbUser user = await _uow.userRepo.GetAll().Where(a => a.ID == ID && a.IsDeleted != true && a.IsEmailVerified != false).FirstOrDefaultAsync() ?? new tbUser();
               
            user.Password = "";
            return user;
        }


        public async Task<bool> ChangePassword(ChangePasswordRequest request)
        {

            var user =  _uow.userRepo.GetById(request.UserID);
            if (user == null || !VerifyPassword(request.CurrentPassword, user.Password))
            {
                return false;
            }
            else
            {
                user.Password = HashPassword(request.NewPassword);
                user.AccessTime = MyExtension.getLocalTime();
                tbUser updatedEntity = await _uow.userRepo.UpdateAsync(user) ?? new tbUser();

                if (updatedEntity.ID == 0)
                    return false;

                return true;
            }
            
        }

        private bool VerifyPassword(string inputPassword, string storedHash)
        {
            
            return BCrypt.Net.BCrypt.Verify(inputPassword, storedHash);
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public async Task<bool> VerifyEmail(int id)
        {
            tbUser user = await GetByID(id);
            user.IsEmailVerified = true;
            user.AccessTime = DateTime.Now;
            var updatedEntity = await _uow.userRepo.UpdateAsync(user) ;
            if (updatedEntity != null)
                return true;
            return false;
        }

        public async Task<ResponseModel> CheckEmailExists(int userID, string email)
        {
            ResponseModel response = new ResponseModel();
            tbUser user = await _uow.userRepo.GetAll().Where(a => a.ID == userID && a.Email == email && a.IsDeleted != true && a.IsEmailVerified != false).FirstOrDefaultAsync() ?? new tbUser();
            if(user.ID == 0)
            {
                response.ReturnMessage = "The user with that email does not exist.";
                return response;
            }

            //user exists
            bool isSuccess = SendEmailToReset();
            if (isSuccess)
            {
                response.ReturnMessage = "The link to reset password has been sent, check your email.";
                return response;
            }
            else
            {
                response.ReturnMessage = "Something went wrong to send email to the user.";
                return response;
            }

        }

        private bool SendEmailToReset()
        {
            return true;
        }


        public async Task<ResponseModel> ResetPassword(ResetPasswordRequest request)
        {
            ResponseModel response = new ResponseModel();
            tbUser user = await _uow.userRepo.GetAll().Where(a => a.ID == request.UserID && a.IsDeleted != true && a.IsEmailVerified != false).FirstOrDefaultAsync() ?? new tbUser();
            if(user.ID == 0)
            {
                response.ReturnMessage = "That user does not exist.";
                return response;
            }

            //user exists
            if (request.NewPassword != request.ConfirmPassword)
            {
                response.ReturnMessage = "New password and comfim password do not match each other.";
                return response;
            }

            else
            {
                user.Password = HashPassword(request.NewPassword);
                user.AccessTime = DateTime.Now;
                tbUser result = await _uow.userRepo.UpdateAsync(user);
                if (result != null)
                {
                    response.ReturnMessage = "Password has been reset successfully.";
                    return response;
                }
                else
                {
                    response.ReturnMessage = "Something went wrong!";
                    return response;
                }
            }
        }
    }
}
