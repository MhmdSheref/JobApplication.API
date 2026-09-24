using Hangfire;
using JobApplication.Application.Interfaces;
using JobApplication.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JobApplication.Application.Services
{
    public class ApplicationService
    {
        private readonly IApplicationRepository _applicationRepository;

        public ApplicationService(IApplicationRepository applicationRepository)
        {
            _applicationRepository = applicationRepository;
        }

        public async Task Cancel(int id)
        {
            var application = await _applicationRepository.GetByIdAsync(id);
            if (application == null)
            {
                throw new KeyNotFoundException($"Application with ID {id} was not found.");
            }

            application.Cancel();

            _applicationRepository.Update(application);
            await _applicationRepository.SaveChangesAsync();

            BackgroundJob.Enqueue<INotificationService>(x => x.NotifyCandidate(id));
        }
    }
}
