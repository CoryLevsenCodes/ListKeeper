using ListKeeperWebApi.WebApi.Models.ViewModels;

namespace ListKeeperWebApi.WebApi.Services
{
    public interface IUserService
    {
        Task<UserViewModel?> AuthenticateAsync(LoginViewModel loginViewModel);
        Task<UserViewModel?> CreateUserAsync(UserViewModel createUserVm);
        Task<bool> DeleteUserAsync(int id);
        Task<bool> DeleteUserAsync(UserViewModel userVm);
        Task<IEnumerable<UserViewModel>> GetAllUsersAsync();
        Task<UserViewModel?> GetUserByIdAsync(int id);
        Task<UserViewModel?> LoginAsync(string email, string password);
        Task<UserViewModel?> SignupAsync(SignupViewModel signupViewModel);
        Task<UserViewModel?> UpdateUserAsync(UserViewModel userVm);
        
        /// <summary>
        /// Verifies a plain text password against a stored password hash
        /// </summary>
        /// <param name="plainTextPassword">The plain text password to verify</param>
        /// <param name="hashedPassword">The stored hashed password</param>
        /// <returns>True if the password matches, false otherwise</returns>
        bool VerifyPassword(string plainTextPassword, string hashedPassword);
    }
}
