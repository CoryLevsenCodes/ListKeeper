using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using ListKeeperWebApi.WebApi.Models.ViewModels;
using ListKeeperWebApi.WebApi.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc; // Required for the [FromServices] attribute
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics; // Required for Debugger.Launch()

namespace ListKeeperWebApi.WebApi.Endpoints
{
    public static class UserEndpoints
    {
        public static RouteGroupBuilder MapUserApiEndpoints(this RouteGroupBuilder group)
        {
            // The endpoint mapping remains the same.
            group.MapGet("/", GetAllUsers)
                 .WithName("GetAllUsers")
                 .WithDescription("Gets all users")
                 .RequireAuthorization("Admin");

            group.MapGet("/{userId}", GetUser)
                 .WithName("GetUser")
                 .WithDescription("Gets a user by their ID")
                 .RequireAuthorization("Admin");

            group.MapPost("/", CreateUser)
                 .WithName("CreateUser")
                 .WithDescription("Creates a new user")
                 .RequireAuthorization("Admin");

            group.MapPut("/{userId}", UpdateUser)
                 .WithName("UpdateUser")
                 .WithDescription("Updates an existing user")
                 .RequireAuthorization("Admin");

            group.MapDelete("/{userId}", DeleteUser)
                 .WithName("DeleteUser")
                 .WithDescription("Deletes a user")
                 .RequireAuthorization("Admin");

            group.MapPost("/Authenticate", Authenticate)
                 .WithName("Authenticate")
                 .WithDescription("Authenticates a user and returns a token")
                 .AllowAnonymous();

            group.MapPost("/Signup", Signup)
                 .WithName("Signup")
                 .WithDescription("Registers a new user and returns a token")
                 .AllowAnonymous();

            return group;
        }

        // --- HANDLER METHODS WITH CORRECTED DEPENDENCY INJECTION ---

        private static async Task<IResult> GetAllUsers(
            // The [FromServices] attribute tells the API to get these objects
            // from the services container, not the request body. This is the fix.
            [FromServices] IUserService userService,
            [FromServices] ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger("Authenticate");
            try
            {
                logger.LogInformation("Getting all users");
                var users = await userService.GetAllUsersAsync();
                return Results.Ok(new { users });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving all users");
                return Results.Problem("An error occurred while retrieving users", statusCode: (int)HttpStatusCode.InternalServerError);
            }
        }

        private static async Task<IResult> GetUser(
            int userId,
            [FromServices] IUserService userService,
            [FromServices] ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger("GetUser");
            try
            {
                var user = await userService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return Results.NotFound($"User with ID {userId} not found");
                }
                return Results.Ok(user);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving user with ID {UserId}", userId);
                return Results.Problem("An error occurred while retrieving the user", statusCode: (int)HttpStatusCode.InternalServerError);
            }
        }

        private static async Task<IResult> CreateUser(
            UserViewModel model, // 'model' correctly comes from the request body, so it does NOT get [FromServices]
            [FromServices] IUserService userService,
            [FromServices] ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger("CreateUser");
            try
            {
                var newUser = await userService.CreateUserAsync(model);
                if (newUser == null)
                {
                    return Results.Conflict($"Could not create user. Email {model.Email} may already be in use.");
                }
                return Results.Created($"/api/users/{newUser.Id}", newUser);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating user with email {Email}", model?.Email);
                return Results.Problem("An error occurred while creating the user", statusCode: (int)HttpStatusCode.InternalServerError);
            }
        }

        // --- NOTE: Apply the [FromServices] fix to ALL your handlers ---

        private static async Task<IResult> UpdateUser(
            int userId,
            UserViewModel model,
            [FromServices] IUserService userService,
            [FromServices] ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger("UpdateUser");
            try
            {
                model.Id = userId;
                var updatedUser = await userService.UpdateUserAsync(model);
                if (updatedUser == null)
                {
                    return Results.NotFound($"User with ID {userId} not found");
                }
                return Results.Ok(updatedUser);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating user with ID {UserId}", userId);
                return Results.Problem("An error occurred while updating the user", statusCode: (int)HttpStatusCode.InternalServerError);
            }
        }

        private static async Task<IResult> DeleteUser(
            int userId,
            [FromServices] IUserService userService,
            [FromServices] ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger("DeleteUser");
            try
            {
                var success = await userService.DeleteUserAsync(userId);
                if (!success)
                {
                    return Results.NotFound($"User with ID {userId} not found");
                }
                return Results.Ok(new { status = "200", result = $"user: {userId} deleted" });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting user with ID {UserId}", userId);
                return Results.Problem("An error occurred while deleting the user", statusCode: (int)HttpStatusCode.InternalServerError);
            }
        }

        private static async Task<IResult> Authenticate(
            LoginViewModel model,
            [FromServices] IUserService userService,
            [FromServices] ILoggerFactory loggerFactory,
            [FromServices] IConfiguration config)
        {
            var logger = loggerFactory.CreateLogger("Authenticate");
            try
            {
                var user = await userService.AuthenticateAsync(model);
                if (user == null)
                {
                    return Results.Unauthorized();
                }

                // Check if MFA is enabled for this user
                if (user.IsMfaEnabled)
                {
                    // Don't return the full JWT token yet - require MFA verification first
                    // Generate a temporary MFA token for the verification step
                    var mfaToken = GenerateMfaToken(user, config);
                    
                    return Results.Ok(new
                    {
                        MfaRequired = true,
                        MfaToken = mfaToken,
                        Message = "Multi-factor authentication required. Please enter your 6-digit code from Microsoft Authenticator."
                    });
                }

                // --- CORRECTED LOGIC ---
                // Always generate a new token here to ensure it's created by this method's logic.
                // This removes any ambiguity about where the token comes from.
                user.Token = GenerateJwtToken(user, config);

                return Results.Ok(user);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error authenticating user {Email}", model?.Username);
                return Results.Problem("An error occurred during authentication", statusCode: (int)HttpStatusCode.InternalServerError);
            }
        }

        private static async Task<IResult> Signup(
            [FromBody] SignupViewModel model,
            [FromServices] IUserService userService,
            [FromServices] IConfiguration config,
            [FromServices] ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger("Signup");
            try
            {
                if (model == null)
                {
                    return Results.BadRequest("Signup data is required");
                }

                logger.LogInformation("Signup attempt for email: {Email}", model.Email);

                var user = await userService.SignupAsync(model);
                if (user == null)
                {
                    return Results.BadRequest("User already exists or signup failed");
                }

                // Generate JWT token for immediate login after signup
                var token = GenerateJwtToken(user, config);
                
                // Return user info with token (same as authenticate endpoint)
                return Results.Ok(new
                {
                    user = new
                    {
                        id = user.Id,
                        email = user.Email,
                        username = user.Username,
                        firstname = user.Firstname,
                        lastname = user.Lastname,
                        role = user.Role
                    },
                    token = token
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during signup for email: {Email}", model?.Email);
                return Results.Problem("An error occurred during signup", statusCode: (int)HttpStatusCode.InternalServerError);
            }
        }

        private static string GenerateJwtToken(UserViewModel user, IConfiguration config)
        {
            var secret = config["Jwt:Secret"];
            if (string.IsNullOrEmpty(secret))
            {
                throw new InvalidOperationException("JWT secret is not configured.");
            }
            var key = Encoding.UTF8.GetBytes(secret);
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // Standard claim for user ID
                    new Claim("id", user.Id.ToString()), // Keep for backward compatibility
                    new Claim(ClaimTypes.Name, user.Username ?? string.Empty),
                    new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                    // This is the crucial claim that the authorization policy checks.
                    new Claim(ClaimTypes.Role, user.Role ?? string.Empty)
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        /// <summary>
        /// Generates a temporary MFA token that allows user to complete MFA verification
        /// </summary>
        private static string GenerateMfaToken(UserViewModel user, IConfiguration config)
        {
            var secret = config["Jwt:Secret"];
            if (string.IsNullOrEmpty(secret))
            {
                throw new InvalidOperationException("JWT secret is not configured.");
            }
            var key = Encoding.UTF8.GetBytes(secret);
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim("mfa_pending", "true"), // Special claim indicating MFA is pending
                    new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                    new Claim("username", user.Username ?? string.Empty)
                }),
                Expires = DateTime.UtcNow.AddMinutes(10), // Short expiry for MFA tokens
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
