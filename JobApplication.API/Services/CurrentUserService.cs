using JobApplication.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace JobApplication.API.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

        public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

        public int? UserId => int.TryParse(User?.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : null;

        public string? Role => User?.FindFirst(ClaimTypes.Role)?.Value;

        public int? RecruiterId => int.TryParse(User?.FindFirst("RecruiterId")?.Value, out var id) ? id : null;

        public int? CandidateId => int.TryParse(User?.FindFirst("CandidateId")?.Value, out var id) ? id : null;
    }
}
