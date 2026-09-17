using JobApplication.Domain.Entities;
using System.Threading.Tasks;

namespace JobApplication.Application.Interfaces
{
    public interface IApplicationRepository
    {
        Task<JobCandidateApplication?> GetByIdAsync(int id);
        void Update(JobCandidateApplication application);
        Task SaveChangesAsync();
    }
}
