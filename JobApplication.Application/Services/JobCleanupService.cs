using JobApplication.Application.Interfaces;
using JobApplication.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JobApplication.Application.Services
{
    public class JobCleanupService : IJobCleanupService
    {
        private readonly IRepository<Job> _jobRepository;
        private readonly ILogger<JobCleanupService> _logger;
        private const int ExpirationDays = 30;

        public JobCleanupService(IRepository<Job> jobRepository, ILogger<JobCleanupService> logger)
        {
            _jobRepository = jobRepository;
            _logger = logger;
        }

        public async Task AutoCloseExpiredJobsAsync(CancellationToken cancellationToken = default)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-ExpirationDays);

            _logger.LogInformation("Running recurring job: checking for active jobs created before {CutoffDate}", cutoffDate);

            var expiredJobs = await _jobRepository.Get()
                .Where(j => j.IsActive && j.CreatedAt <= cutoffDate)
                .ToListAsync(cancellationToken);

            if (expiredJobs.Count == 0)
            {
                _logger.LogInformation("No expired jobs found to auto-close.");
                return;
            }

            foreach (var job in expiredJobs)
            {
                job.AutoClose();
                _jobRepository.Update(job);
                _logger.LogInformation("Auto-closed job {JobId} ('{Title}') open since {CreatedAt}", job.Id, job.Title, job.CreatedAt);
            }

            await _jobRepository.SaveChangesAsync();
            _logger.LogInformation("Successfully auto-closed {Count} expired jobs.", expiredJobs.Count);
        }
    }
}
