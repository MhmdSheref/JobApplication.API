using JobApplication.Application.Interfaces;
using JobApplication.Domain.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Infrastructure.Services
{
    public class EmailNotificationService : INotificationService
    {
        private readonly IRepository<JobCandidateApplication> _jobCandidateApplicationRepository;
        private readonly ILogger<EmailNotificationService> _logger;

        public EmailNotificationService(IRepository<JobCandidateApplication> jobCandidateApplicationRepository, ILogger<EmailNotificationService> logger)
        {
            _jobCandidateApplicationRepository = jobCandidateApplicationRepository;
            _logger = logger;
        }

        public void NotifyRecruiter(int applicationId)
        {
            var application = _jobCandidateApplicationRepository.Get().FirstOrDefault(a=>a.Id == applicationId); 

            if(application is null)
            {
                _logger.LogWarning("application {applicationId}is not found ", applicationId); 
                return; 
            }
            _logger.LogInformation("Send Email :  cadidate {CandidateId} has applied to {JobId} and applicationId is {applicationId}", 
                application.CandidateId, application.JobId,applicationId);
        }

        public void NotifyCandidate(int applicationId)
        {
            var application = _jobCandidateApplicationRepository.Get().FirstOrDefault(a => a.Id == applicationId);

            if (application is null)
            {
                _logger.LogWarning("application {applicationId} is not found", applicationId);
                return;
            }

            _logger.LogInformation("Send Email : Candidate {CandidateId}'s application {applicationId} for job {JobId} has been cancelled.",
                application.CandidateId, applicationId, application.JobId);
        }
    }
}
