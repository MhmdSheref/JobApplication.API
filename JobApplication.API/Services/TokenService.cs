using JobApplication.Application.Interfaces;
using JobApplication.Domain.Entities;
using Microsoft.Extensions.Configuration;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace JobApplication.API.Services
{
    public class TokenPayload
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int? RecruiterId { get; set; }
        public int? CandidateId { get; set; }
        public long ExpiresAt { get; set; }
    }

    public class TokenService : ITokenService
    {
        private readonly byte[] _key;

        public TokenService(IConfiguration configuration)
        {
            var secret = configuration["Jwt:Key"] ?? "default_job_application_super_secret_key_32_bytes_long!";
            _key = Encoding.UTF8.GetBytes(secret);
        }

        public string GenerateToken(User user)
        {
            var payload = new TokenPayload
            {
                UserId = user.Id,
                Email = user.Email,
                Role = user.Role.ToString(),
                RecruiterId = user.RecruiterId,
                CandidateId = user.CandidateId,
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(7).ToUnixTimeSeconds()
            };

            var payloadBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(payload));
            var payloadBase64 = Convert.ToBase64String(payloadBytes);

            using var hmac = new HMACSHA256(_key);
            var signatureBytes = hmac.ComputeHash(payloadBytes);
            var signatureBase64 = Convert.ToBase64String(signatureBytes);

            return $"{payloadBase64}.{signatureBase64}";
        }

        public TokenPayload? ValidateToken(string token)
        {
            try
            {
                var parts = token.Split('.');
                if (parts.Length != 2) return null;

                var payloadBytes = Convert.FromBase64String(parts[0]);
                var signatureBytes = Convert.FromBase64String(parts[1]);

                using var hmac = new HMACSHA256(_key);
                var expectedSignature = hmac.ComputeHash(payloadBytes);

                if (!CryptographicOperations.FixedTimeEquals(signatureBytes, expectedSignature))
                {
                    return null;
                }

                var payload = JsonSerializer.Deserialize<TokenPayload>(Encoding.UTF8.GetString(payloadBytes));
                if (payload == null || payload.ExpiresAt < DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                {
                    return null;
                }

                return payload;
            }
            catch
            {
                return null;
            }
        }
    }
}
