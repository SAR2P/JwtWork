using IdentityManagerServerApi.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using SharedClassLibrary.Contracts;
using SharedClassLibrary.DTOs_ViewModels;
using SharedClassLibrary.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static SharedClassLibrary.Services.ServiceResponses;

namespace IdentityManagerServerApi.Repository
{
    public class AccountRepository(UserManager<ApplicationUser> UserManager,
        RoleManager<IdentityRole> roleManager, IConfiguration config) : IUserAccount
    {
        public async Task<GeneralResponse> CreateAccount(UserDTO userDTO)
        {
            if (userDTO == null)
                return new GeneralResponse(false, "model is empty");

            var newUser = new ApplicationUser()
            {
                FullName = userDTO.FullName,
                Email = userDTO.Email,
                PasswordHash = userDTO.Password,
                UserName = userDTO.Email
            };
            var user = await UserManager.FindByEmailAsync(newUser.Email);
            if (user != null)
                return new GeneralResponse(false, "User Register Already");

            var createdUser = await UserManager.CreateAsync(newUser!, userDTO.Password);
            if (!createdUser.Succeeded)
                return new GeneralResponse(false, "error ocurd, please try again");

            //user Created!!! Now we giv Role To User


            var checkAdmin = await roleManager.FindByNameAsync("Admin");//if the first user Come in Create admin role & make the firs user admin
            if (checkAdmin == null)
            {
                await roleManager.CreateAsync(new IdentityRole() { Name = "Admin" });
                await UserManager.AddToRoleAsync(newUser, "Admin");
                return new GeneralResponse(true, "Account Created By Admin Role");
            }
            else
            {
                var checkUser = await roleManager.FindByNameAsync("User");
                if (checkUser == null)
                    await roleManager.CreateAsync(new IdentityRole() { Name = "User" });

                await UserManager.AddToRoleAsync(newUser, "user");
                return new GeneralResponse(true, "Account Created By User Role");
            }

        }

        public async Task<LoginResponse> Login(LoginDTO loginDTO)
        {
            if (loginDTO == null)
                return new LoginResponse(false, null!, "Login container is empty");

            var getUser = await UserManager.FindByEmailAsync(loginDTO.Email);
            if (getUser == null)
            return new LoginResponse(false, null!, "User not found");

            bool checkUserPass = await UserManager.CheckPasswordAsync(getUser, loginDTO.Password);
            if (!checkUserPass)
                return new LoginResponse(false, null!, "invalid email/password");

            var getUserRole = await UserManager.GetRolesAsync(getUser);
            var userSession = new UserSession(getUser.Id, getUser.FullName, getUser.Email, getUserRole.First());
            string token = GenerateToken(userSession);
            return new LoginResponse(true, token!, "Login Compleated");
        }



        //private

        private string GenerateToken(UserSession user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var userClaims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier,user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Role, user.Role)
            };
            var token = new JwtSecurityToken(
                issuer: config["Jwt:Issuer"],
                audience: config["Jwt:Audience"],
                claims: userClaims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: credentials
                );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}
