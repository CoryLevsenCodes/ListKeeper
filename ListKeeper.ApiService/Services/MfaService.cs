using OtpNet;
using QRCoder;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ListKeeper.ApiService.Services
{
    /// <summary>
    /// Service for handling Multi-Factor Authentication (MFA) operations
    /// </summary>
    public interface IMfaService
    {
        /// <summary>
        /// Generates a new MFA secret key for a user
        /// </summary>
        string GenerateMfaSecret();

        /// <summary>
        /// Generates a QR code URL for Microsoft Authenticator setup
        /// </summary>
        string GenerateQrCodeUrl(string userEmail, string secret, string issuer = "ListKeeper");

        /// <summary>
        /// Generates a QR code image as base64 string
        /// </summary>
        string GenerateQrCodeImage(string qrCodeUrl);

        /// <summary>
        /// Verifies a TOTP code against the user's secret
        /// </summary>
        bool VerifyTotpCode(string secret, string userCode, int windowSize = 1);

        /// <summary>
        /// Generates backup codes for MFA recovery
        /// </summary>
        List<string> GenerateBackupCodes(int count = 10);

        /// <summary>
        /// Encrypts a secret for database storage
        /// </summary>
        string EncryptSecret(string secret);

        /// <summary>
        /// Decrypts a secret from database storage
        /// </summary>
        string DecryptSecret(string encryptedSecret);

        /// <summary>
        /// Hashes backup codes for secure storage
        /// </summary>
        string HashBackupCodes(List<string> backupCodes);

        /// <summary>
        /// Verifies a backup code against stored hashed codes
        /// </summary>
        bool VerifyBackupCode(string backupCode, string hashedBackupCodes);
    }

    public class MfaService : IMfaService
    {
        private readonly IConfiguration _configuration;
        private readonly string _encryptionKey;

        public MfaService(IConfiguration configuration)
        {
            _configuration = configuration;
            // In production, this should come from a secure key management system
            _encryptionKey = _configuration["MfaSettings:EncryptionKey"] ?? "ListKeeperMfaKey2025!SecureDefault32B";
        }

        public string GenerateMfaSecret()
        {
            var key = KeyGeneration.GenerateRandomKey(20); // 160-bit key
            return Base32Encoding.ToString(key);
        }

        public string GenerateQrCodeUrl(string userEmail, string secret, string issuer = "ListKeeper")
        {
            var totpSetup = new TotpSetup
            {
                Issuer = issuer,
                Account = userEmail,
                Key = secret,
                QrCodeSetupImageUrl = $"otpauth://totp/{Uri.EscapeDataString(issuer)}:{Uri.EscapeDataString(userEmail)}?secret={secret}&issuer={Uri.EscapeDataString(issuer)}"
            };

            return totpSetup.QrCodeSetupImageUrl;
        }

        public string GenerateQrCodeImage(string qrCodeUrl)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(qrCodeUrl, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrCodeData);
            
            var qrCodeImage = qrCode.GetGraphic(20);
            return Convert.ToBase64String(qrCodeImage);
        }

        public bool VerifyTotpCode(string secret, string userCode, int windowSize = 1)
        {
            try
            {
                var secretBytes = Base32Encoding.ToBytes(secret);
                var totp = new Totp(secretBytes);
                
                // Verify with a window to account for time drift
                var currentWindow = DateTime.UtcNow;
                for (int i = -windowSize; i <= windowSize; i++)
                {
                    var windowTime = currentWindow.AddSeconds(i * 30); // TOTP period is 30 seconds
                    var expectedCode = totp.ComputeTotp(windowTime);
                    
                    if (expectedCode == userCode)
                        return true;
                }
                
                return false;
            }
            catch
            {
                return false;
            }
        }

        public List<string> GenerateBackupCodes(int count = 10)
        {
            var backupCodes = new List<string>();
            using var rng = RandomNumberGenerator.Create();
            
            for (int i = 0; i < count; i++)
            {
                var bytes = new byte[4];
                rng.GetBytes(bytes);
                var code = BitConverter.ToUInt32(bytes, 0).ToString("D8");
                backupCodes.Add(code);
            }
            
            return backupCodes;
        }

        public string EncryptSecret(string secret)
        {
            try
            {
                var key = Encoding.UTF8.GetBytes(_encryptionKey.PadRight(32).Substring(0, 32));
                using var aes = Aes.Create();
                aes.Key = key;
                aes.GenerateIV();
                
                using var encryptor = aes.CreateEncryptor();
                var secretBytes = Encoding.UTF8.GetBytes(secret);
                var encryptedBytes = encryptor.TransformFinalBlock(secretBytes, 0, secretBytes.Length);
                
                // Combine IV and encrypted data
                var result = new byte[aes.IV.Length + encryptedBytes.Length];
                Array.Copy(aes.IV, 0, result, 0, aes.IV.Length);
                Array.Copy(encryptedBytes, 0, result, aes.IV.Length, encryptedBytes.Length);
                
                return Convert.ToBase64String(result);
            }
            catch
            {
                throw new InvalidOperationException("Failed to encrypt MFA secret");
            }
        }

        public string DecryptSecret(string encryptedSecret)
        {
            try
            {
                var key = Encoding.UTF8.GetBytes(_encryptionKey.PadRight(32).Substring(0, 32));
                var encryptedData = Convert.FromBase64String(encryptedSecret);
                
                using var aes = Aes.Create();
                aes.Key = key;
                
                // Extract IV and encrypted data
                var iv = new byte[aes.IV.Length];
                var encrypted = new byte[encryptedData.Length - iv.Length];
                Array.Copy(encryptedData, 0, iv, 0, iv.Length);
                Array.Copy(encryptedData, iv.Length, encrypted, 0, encrypted.Length);
                
                aes.IV = iv;
                using var decryptor = aes.CreateDecryptor();
                var decryptedBytes = decryptor.TransformFinalBlock(encrypted, 0, encrypted.Length);
                
                return Encoding.UTF8.GetString(decryptedBytes);
            }
            catch
            {
                throw new InvalidOperationException("Failed to decrypt MFA secret");
            }
        }

        public string HashBackupCodes(List<string> backupCodes)
        {
            var hashedCodes = backupCodes.Select(code => 
                Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(code + _encryptionKey)))
            ).ToList();
            
            return JsonSerializer.Serialize(hashedCodes);
        }

        public bool VerifyBackupCode(string backupCode, string hashedBackupCodes)
        {
            try
            {
                var hashedCodes = JsonSerializer.Deserialize<List<string>>(hashedBackupCodes);
                if (hashedCodes == null) return false;
                
                var inputHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(backupCode + _encryptionKey)));
                return hashedCodes.Contains(inputHash);
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Helper class for TOTP setup
    /// </summary>
    public class TotpSetup
    {
        public string Issuer { get; set; } = string.Empty;
        public string Account { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public string QrCodeSetupImageUrl { get; set; } = string.Empty;
    }
}
