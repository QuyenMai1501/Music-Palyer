using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using MusicPlayer.Models;

namespace MusicPlayer.Services
{
    public enum PasswordCheckResult
    {
        Failed,
        Success,
        SuccessRehash
    }

    public class PasswordService
    {
        private readonly PasswordHasher<User> _hasher = new();
        private static readonly Regex LegacySha256Pattern = new("^[0-9a-f]{64}$", RegexOptions.Compiled);

        public string HashPassword(string password)
        {
            return _hasher.HashPassword(null!, password);
        }

        public PasswordCheckResult CheckPassword(string password, string storedHash)
        {
            if (!string.IsNullOrEmpty(storedHash) && LegacySha256Pattern.IsMatch(storedHash))
            {
                string sha256Hex = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(password))).ToLowerInvariant();
                return sha256Hex == storedHash ? PasswordCheckResult.SuccessRehash : PasswordCheckResult.Failed;
            }

            return _hasher.VerifyHashedPassword(null!, storedHash, password) switch
            {
                PasswordVerificationResult.Success => PasswordCheckResult.Success,
                PasswordVerificationResult.SuccessRehashNeeded => PasswordCheckResult.SuccessRehash,
                _ => PasswordCheckResult.Failed
            };
        }
    }
}
