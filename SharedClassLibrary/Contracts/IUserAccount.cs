using SharedClassLibrary.DTOs_ViewModels;
using static SharedClassLibrary.Services.ServiceResponses;

namespace SharedClassLibrary.Contracts
{
    public interface IUserAccount
    {
        Task<GeneralResponse> CreateAccount(UserDTO userDTO);

        Task<LoginResponse> Login(LoginDTO loginDTO);

    }
}
