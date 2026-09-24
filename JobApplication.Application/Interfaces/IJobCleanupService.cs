using System.Threading;
using System.Threading.Tasks;

namespace JobApplication.Application.Interfaces
{
    public interface IJobCleanupService
    {
        Task AutoCloseExpiredJobsAsync(CancellationToken cancellationToken = default);
    }
}
