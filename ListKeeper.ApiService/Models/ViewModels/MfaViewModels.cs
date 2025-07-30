using System.ComponentModel.DataAnnotations;

namespace ListKeeper.ApiService.Models.ViewModels
{
    /// <summary>
    /// Request model for setting up MFA
    /// </summary>
    public class MfaSetupRequest
    {
        /// <summary>
        /// The user's email address
        /// </summary>
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response model for MFA setup
    /// </summary>
    public class MfaSetupResponse
    {
        /// <summary>
        /// The secret key for manual entry (if QR code doesn't work)
        /// </summary>
        public string SecretKey { get; set; } = string.Empty;

        /// <summary>
        /// QR code as base64 image for Microsoft Authenticator
        /// </summary>
        public string QrCodeImage { get; set; } = string.Empty;

        /// <summary>
        /// Backup codes for recovery
        /// </summary>
        public List<string> BackupCodes { get; set; } = new();

        /// <summary>
        /// Instructions for the user
        /// </summary>
        public string Instructions { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request model for enabling MFA after setup
    /// </summary>
    public class MfaEnableRequest
    {
        /// <summary>
        /// The verification code from Microsoft Authenticator
        /// </summary>
        [Required]
        [StringLength(6, MinimumLength = 6)]
        public string VerificationCode { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request model for MFA verification during login
    /// </summary>
    public class MfaVerificationRequest
    {
        /// <summary>
        /// User's email or username
        /// </summary>
        [Required]
        public string EmailOrUsername { get; set; } = string.Empty;

        /// <summary>
        /// User's password
        /// </summary>
        [Required]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// The 6-digit code from Microsoft Authenticator
        /// </summary>
        [Required]
        [StringLength(6, MinimumLength = 6)]
        public string MfaCode { get; set; } = string.Empty;

        /// <summary>
        /// Whether this is a backup code instead of TOTP code
        /// </summary>
        public bool IsBackupCode { get; set; } = false;
    }

    /// <summary>
    /// Response model for login requiring MFA
    /// </summary>
    public class MfaRequiredResponse
    {
        /// <summary>
        /// Indicates that MFA is required for this user
        /// </summary>
        public bool MfaRequired { get; set; } = true;

        /// <summary>
        /// Temporary token for MFA verification
        /// </summary>
        public string MfaToken { get; set; } = string.Empty;

        /// <summary>
        /// Message for the user
        /// </summary>
        public string Message { get; set; } = "Multi-factor authentication required. Please enter your 6-digit code from Microsoft Authenticator.";
    }

    /// <summary>
    /// Request model for disabling MFA
    /// </summary>
    public class MfaDisableRequest
    {
        /// <summary>
        /// Current password for security verification
        /// </summary>
        [Required]
        public string CurrentPassword { get; set; } = string.Empty;

        /// <summary>
        /// MFA verification code or backup code
        /// </summary>
        [Required]
        [StringLength(8, MinimumLength = 6)]
        public string VerificationCode { get; set; } = string.Empty;

        /// <summary>
        /// Whether this is a backup code
        /// </summary>
        public bool IsBackupCode { get; set; } = false;
    }

    /// <summary>
    /// Response model for MFA status
    /// </summary>
    public class MfaStatusResponse
    {
        /// <summary>
        /// Whether MFA is enabled for the user
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// When MFA was set up
        /// </summary>
        public DateTime? SetupDate { get; set; }

        /// <summary>
        /// Number of remaining backup codes
        /// </summary>
        public int BackupCodesRemaining { get; set; }
    }
}
