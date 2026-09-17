using JobApplication.Domain.Entities;
using System.Threading.Tasks;

namespace JobApplication.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task InsertAsync(User user);
        Task SaveChangesAsync();
    }
}
