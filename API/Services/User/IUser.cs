using Data.Model;
using Data.ViewModel;
using Data.ViewModel.User;

namespace API.Services.User
{
    public interface IUser
    {
        Task<tbUser> Upsert(tbUser data);
        Task<ResponseModel> Register(UserModel data);
        Task<tbUser> GetProfile(int ID);
        Task<bool> ChangePassword(ChangePasswordRequest request);
        Task<bool> VerifyEmail(int id);
        Task<ResponseModel> CheckEmailExists(int userID, string email);

        Task<ResponseModel> ResetPassword(ResetPasswordRequest request);
    }
}
