using Data.Model;
using Data.ViewModel.Auth;

namespace API.Services.Auth
{
    public interface IAuth
    {
        Task<object> Login(LoginRequest loginRequest);
    }
}
