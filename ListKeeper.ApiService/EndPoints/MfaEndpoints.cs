using ListKeeper.ApiService.Services;
using ListKeeper.ApiService.Models.ViewModels;
using ListKeeper.ApiService.Helpers;
using ListKeeperWebApi.WebApi.Models;
using ListKeeperWebApi.WebApi.Services;
using ListKeeperWebApi.WebApi.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ListKeeper.ApiService.EndPoints
{
    /// <summary>
    /// API endpoints for Multi-Factor Authentication (MFA) management
    /// </summary>
    public static class MfaEndpoints
    {
        public static void MapMfaEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/api/mfa")
                .WithTags("MFA")
                .WithOpenApi();

            // Setup MFA - generates QR code and backup codes
            group.MapPost("/setup", SetupMfa)
                .RequireAuthorization()
                .WithName("SetupMfa")
                .WithSummary("Initialize MFA setup for a user")
                .WithDescription("Generates a secret key, QR code, and backup codes for MFA setup");

            // Enable MFA - verify setup and activate MFA
            group.MapPost("/enable", EnableMfa)
                .RequireAuthorization()
                .WithName("EnableMfa")
                .WithSummary("Enable MFA after verifying setup")
                .WithDescription("Verifies the setup code and enables MFA for the user");

            // Verify MFA during login
            group.MapPost("/verify", VerifyMfa)
                .AllowAnonymous()
                .WithName("VerifyMfa")
                .WithSummary("Verify MFA code during login")
                .WithDescription("Verifies MFA code and completes the login process");

            // Disable MFA
            group.MapPost("/disable", DisableMfa)
                .RequireAuthorization()
                .WithName("DisableMfa")
                .WithSummary("Disable MFA for a user")
                .WithDescription("Disables MFA after password and MFA code verification");

            // Get MFA status
            group.MapGet("/status", GetMfaStatus)
                .RequireAuthorization()
                .WithName("GetMfaStatus")
                .WithSummary("Get current MFA status")
                .WithDescription("Returns whether MFA is enabled and related information");

            // Generate new backup codes
            group.MapPost("/backup-codes/regenerate", RegenerateBackupCodes)
                .RequireAuthorization()
                .WithName("RegenerateBackupCodes")
                .WithSummary("Generate new backup codes")
                .WithDescription("Generates new backup codes and invalidates old ones");
        }

        private static async Task<IResult> SetupMfa(
            [FromBody] MfaSetupRequest request,
            [FromServices] IMfaService mfaService,
            [FromServices] DatabaseContext dbContext,
            [FromServices] ICurrentUserHelper currentUserHelper)
        {
            try
            {
                var currentUserId = currentUserHelper.GetCurrentUserId();
                if (currentUserId == null)
                {
                    return Results.Unauthorized();
                }

                var user = await dbContext.Users.FindAsync(currentUserId);
                if (user == null)
                {
                    return Results.NotFound("User not found");
                }

                // Generate MFA secret and backup codes
                var secret = mfaService.GenerateMfaSecret();
                var qrCodeUrl = mfaService.GenerateQrCodeUrl(user.Email, secret);
                var qrCodeImage = mfaService.GenerateQrCodeImage(qrCodeUrl);
                var backupCodes = mfaService.GenerateBackupCodes();

                // Store encrypted secret temporarily (not enabled until verified)
                user.MfaSecretKey = mfaService.EncryptSecret(secret);
                user.MfaBackupCodes = mfaService.HashBackupCodes(backupCodes);
                
                await dbContext.SaveChangesAsync();

                var response = new MfaSetupResponse
                {
                    SecretKey = secret,
                    QrCodeImage = qrCodeImage,
                    BackupCodes = backupCodes,
                    Instructions = "1. Open Microsoft Authenticator app\n2. Tap '+' to add account\n3. Scan QR code or enter key manually\n4. Enter the 6-digit code to verify setup"
                };

                return Results.Ok(response);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Failed to setup MFA: {ex.Message}");
            }
        }

        private static async Task<IResult> EnableMfa(
            [FromBody] MfaEnableRequest request,
            [FromServices] IMfaService mfaService,
            [FromServices] DatabaseContext dbContext,
            [FromServices] ICurrentUserHelper currentUserHelper)
        {
            try
            {
                var currentUserId = currentUserHelper.GetCurrentUserId();
                if (currentUserId == null)
                {
                    return Results.Unauthorized();
                }

                var user = await dbContext.Users.FindAsync(currentUserId);
                if (user == null)
                {
                    return Results.NotFound("User not found");
                }

                if (string.IsNullOrEmpty(user.MfaSecretKey))
                {
                    return Results.BadRequest("MFA setup not found. Please run setup first.");
                }

                // Decrypt and verify the code
                var secret = mfaService.DecryptSecret(user.MfaSecretKey);
                if (!mfaService.VerifyTotpCode(secret, request.VerificationCode))
                {
                    return Results.BadRequest("Invalid verification code");
                }

                // Enable MFA
                user.IsMfaEnabled = true;
                user.MfaSetupDate = DateTime.UtcNow;
                
                await dbContext.SaveChangesAsync();

                return Results.Ok(new { Message = "MFA enabled successfully", MfaEnabled = true });
            }
            catch (Exception ex)
            {
                return Results.Problem($"Failed to enable MFA: {ex.Message}");
            }
        }

        private static async Task<IResult> VerifyMfa(
            [FromBody] MfaVerificationRequest request,
            [FromServices] IMfaService mfaService,
            [FromServices] DatabaseContext dbContext,
            [FromServices] IUserService userService)
        {
            try
            {
                // First verify password
                var user = await dbContext.Users
                    .FirstOrDefaultAsync(u => u.Email == request.EmailOrUsername || u.Username == request.EmailOrUsername);

                if (user == null || !userService.VerifyPassword(request.Password, user.Password))
                {
                    return Results.BadRequest("Invalid credentials");
                }

                if (!user.IsMfaEnabled)
                {
                    return Results.BadRequest("MFA is not enabled for this user");
                }

                bool isCodeValid = false;

                if (request.IsBackupCode)
                {
                    // Verify backup code
                    if (!string.IsNullOrEmpty(user.MfaBackupCodes))
                    {
                        isCodeValid = mfaService.VerifyBackupCode(request.MfaCode, user.MfaBackupCodes);
                        
                        if (isCodeValid)
                        {
                            // Remove used backup code
                            var hashedCodes = JsonSerializer.Deserialize<List<string>>(user.MfaBackupCodes) ?? new List<string>();
                            var codeHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(request.MfaCode + "ListKeeperMfaKey2025!SecureDefault32B")));
                            hashedCodes.Remove(codeHash);
                            user.MfaBackupCodes = JsonSerializer.Serialize(hashedCodes);
                            await dbContext.SaveChangesAsync();
                        }
                    }
                }
                else
                {
                    // Verify TOTP code
                    if (!string.IsNullOrEmpty(user.MfaSecretKey))
                    {
                        var secret = mfaService.DecryptSecret(user.MfaSecretKey);
                        isCodeValid = mfaService.VerifyTotpCode(secret, request.MfaCode);
                    }
                }

                if (!isCodeValid)
                {
                    return Results.BadRequest("Invalid MFA code");
                }

                // Generate JWT token for successful login
                var loginResponse = await userService.LoginAsync(request.EmailOrUsername, request.Password);
                return Results.Ok(loginResponse);
            }
            catch (Exception ex)
            {
                return Results.Problem($"MFA verification failed: {ex.Message}");
            }
        }

        private static async Task<IResult> DisableMfa(
            [FromBody] MfaDisableRequest request,
            [FromServices] IMfaService mfaService,
            [FromServices] DatabaseContext dbContext,
            [FromServices] ICurrentUserHelper currentUserHelper,
            [FromServices] IUserService userService)
        {
            try
            {
                var currentUserId = currentUserHelper.GetCurrentUserId();
                if (currentUserId == null)
                {
                    return Results.Unauthorized();
                }

                var user = await dbContext.Users.FindAsync(currentUserId);
                if (user == null)
                {
                    return Results.NotFound("User not found");
                }

                // Verify password
                if (!userService.VerifyPassword(request.CurrentPassword, user.Password))
                {
                    return Results.BadRequest("Invalid password");
                }

                // Verify MFA code
                bool isCodeValid = false;
                
                if (request.IsBackupCode && !string.IsNullOrEmpty(user.MfaBackupCodes))
                {
                    isCodeValid = mfaService.VerifyBackupCode(request.VerificationCode, user.MfaBackupCodes);
                }
                else if (!string.IsNullOrEmpty(user.MfaSecretKey))
                {
                    var secret = mfaService.DecryptSecret(user.MfaSecretKey);
                    isCodeValid = mfaService.VerifyTotpCode(secret, request.VerificationCode);
                }

                if (!isCodeValid)
                {
                    return Results.BadRequest("Invalid verification code");
                }

                // Disable MFA
                user.IsMfaEnabled = false;
                user.MfaSecretKey = null;
                user.MfaBackupCodes = null;
                user.MfaSetupDate = null;
                
                await dbContext.SaveChangesAsync();

                return Results.Ok(new { Message = "MFA disabled successfully", MfaEnabled = false });
            }
            catch (Exception ex)
            {
                return Results.Problem($"Failed to disable MFA: {ex.Message}");
            }
        }

        private static async Task<IResult> GetMfaStatus(
            [FromServices] DatabaseContext dbContext,
            [FromServices] ICurrentUserHelper currentUserHelper)
        {
            try
            {
                var currentUserId = currentUserHelper.GetCurrentUserId();
                if (currentUserId == null)
                {
                    return Results.Unauthorized();
                }

                var user = await dbContext.Users.FindAsync(currentUserId);
                if (user == null)
                {
                    return Results.NotFound("User not found");
                }

                int backupCodesCount = 0;
                if (!string.IsNullOrEmpty(user.MfaBackupCodes))
                {
                    var hashedCodes = JsonSerializer.Deserialize<List<string>>(user.MfaBackupCodes);
                    backupCodesCount = hashedCodes?.Count ?? 0;
                }

                var response = new MfaStatusResponse
                {
                    IsEnabled = user.IsMfaEnabled,
                    SetupDate = user.MfaSetupDate,
                    BackupCodesRemaining = backupCodesCount
                };

                return Results.Ok(response);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Failed to get MFA status: {ex.Message}");
            }
        }

        private static async Task<IResult> RegenerateBackupCodes(
            [FromServices] IMfaService mfaService,
            [FromServices] DatabaseContext dbContext,
            [FromServices] ICurrentUserHelper currentUserHelper)
        {
            try
            {
                var currentUserId = currentUserHelper.GetCurrentUserId();
                if (currentUserId == null)
                {
                    return Results.Unauthorized();
                }

                var user = await dbContext.Users.FindAsync(currentUserId);
                if (user == null)
                {
                    return Results.NotFound("User not found");
                }

                if (!user.IsMfaEnabled)
                {
                    return Results.BadRequest("MFA is not enabled");
                }

                // Generate new backup codes
                var newBackupCodes = mfaService.GenerateBackupCodes();
                user.MfaBackupCodes = mfaService.HashBackupCodes(newBackupCodes);
                
                await dbContext.SaveChangesAsync();

                return Results.Ok(new { BackupCodes = newBackupCodes, Message = "New backup codes generated. Store them securely!" });
            }
            catch (Exception ex)
            {
                return Results.Problem($"Failed to regenerate backup codes: {ex.Message}");
            }
        }
    }
}
