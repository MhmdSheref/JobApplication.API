using JobApplication.Domain.Entities;

namespace JobApplication.Application.Interfaces
{
    public interface ITokenService
    {
        string GenerateToken(User user);
    }
}
