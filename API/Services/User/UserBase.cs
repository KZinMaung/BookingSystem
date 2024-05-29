using Core.Extension;
using Data.Model;
using Data.ViewModel;
using Infra.UnitOfWork;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography.Xml;

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

        public async Task<tbUser> Upsert(tbUser data)
        {
           
            if (data.ID > 0)
            {
                tbUser user =  _uow.userRepo.GetById(data.ID);
                user.Name  = data.Name;
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
            return data;
        }

        //for profile
        public async Task<tbUser> GetProfile(int ID)
        {
            tbUser user = await _uow.userRepo.GetAll()
                .FirstOrDefaultAsync(a => a.ID == ID && a.IsDeleted != true) ?? new tbUser();

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


    }
}
