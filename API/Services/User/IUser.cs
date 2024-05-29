using Data.Model;
using Data.ViewModel;

namespace API.Services.User
{
    public interface IUser
    {
        Task<tbUser> Upsert(tbUser data);
        Task<tbUser> GetProfile(int ID);
        Task<bool> ChangePassword(ChangePasswordRequest request);
    }
}
