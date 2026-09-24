using JobApplication.Application.Interfaces;
using JobApplication.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Application.Features.JobCandidateApplications.Commands.UpdateJobCandidateApplicationStatus
{
    public class UpdateJobCandidateApplicationStatusHandler : IRequestHandler<UpdateJobCandidateApplicationStatusCommand, JobCandidateApplication?>
    {
        private readonly IRepository<JobCandidateApplication> _jobApplicationRepository;
        private readonly IBackgroundJobScheduler _backgroundJobScheduler;

        public UpdateJobCandidateApplicationStatusHandler(IRepository<JobCandidateApplication> jobApplicationRepository, IBackgroundJobScheduler backgroundJobScheduler)
        {
            _jobApplicationRepository = jobApplicationRepository;
            _backgroundJobScheduler = backgroundJobScheduler;
        }

        public async Task<JobCandidateApplication?> Handle(UpdateJobCandidateApplicationStatusCommand request, CancellationToken cancellationToken)
        {
            var jobApplication = await _jobApplicationRepository.Get().FirstOrDefaultAsync(a => a.Id == request.Id);
            if (jobApplication == null)
            {
                return null;
            }
            jobApplication.UpdateStatus(request.Status);
            await _jobApplicationRepository.SaveChangesAsync();

            _backgroundJobScheduler.Schedule<INotificationService>(s=>s.NotifyRecruiter(jobApplication.Id) , TimeSpan.FromMinutes(2)); 
            return jobApplication;
        }
    }
}
