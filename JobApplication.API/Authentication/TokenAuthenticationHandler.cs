using JobApplication.API.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace JobApplication.API.Authentication
{
    public class TokenAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly TokenService _tokenService;

        public TokenAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            TokenService tokenService)
            : base(options, logger, encoder)
        {
            _tokenService = tokenService;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue("Authorization", out var authHeaderValues))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var authHeader = authHeaderValues.ToString();
            if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();
            var payload = _tokenService.ValidateToken(token);
            if (payload == null)
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid or expired token."));
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, payload.UserId.ToString()),
                new Claim(ClaimTypes.Email, payload.Email),
                new Claim(ClaimTypes.Role, payload.Role),
                new Claim("RecruiterId", payload.RecruiterId?.ToString() ?? string.Empty),
                new Claim("CandidateId", payload.CandidateId?.ToString() ?? string.Empty)
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
