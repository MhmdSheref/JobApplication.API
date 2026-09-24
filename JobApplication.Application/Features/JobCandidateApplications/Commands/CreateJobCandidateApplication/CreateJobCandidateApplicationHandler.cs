using JobApplication.Application.Interfaces;
using JobApplication.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Application.Features.JobCandidateApplications.Commands.CreateJobCandidateApplication
{
    public class CreateJobCandidateApplicationHandler : IRequestHandler<CreateJobCandidateApplicationCommand, int>
    {
        private readonly IRepository<JobCandidateApplication> _jobApplicationRepository;
        private readonly INotificationService _notificationService ;
        private readonly IBackgroundJobScheduler _backgroundJobScheduler;

        public CreateJobCandidateApplicationHandler(IRepository<JobCandidateApplication> jobApplicationRepository, INotificationService notificationService, IBackgroundJobScheduler backgroundJobScheduler)
        {
            _jobApplicationRepository = jobApplicationRepository;
            _notificationService = notificationService;
            _backgroundJobScheduler = backgroundJobScheduler;
        }

        public async Task<int> Handle(CreateJobCandidateApplicationCommand request, CancellationToken cancellationToken)
        {
            var jobApplication = new JobCandidateApplication()
            {
                JobId = request.JobId,
                CandidateId = request.CandidateId,
            };
            await _jobApplicationRepository.AddAsync(jobApplication);
            await _jobApplicationRepository.SaveChangesAsync();

            //_notificationService.NotifyRecruiter(jobApplication.Id); 
            _backgroundJobScheduler.Enqueue<INotificationService>(b=>b.NotifyRecruiter(jobApplication.Id)); 

            return jobApplication.Id;
        }
    }
}
