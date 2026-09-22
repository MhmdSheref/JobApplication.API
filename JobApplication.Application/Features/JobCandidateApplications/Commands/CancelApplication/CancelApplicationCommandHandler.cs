using JobApplication.Application.Interfaces;
using JobApplication.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace JobApplication.Application.Features.JobCandidateApplications.Commands.CancelApplication
{
    public class CancelApplicationCommandHandler : IRequestHandler<CancelApplicationCommand>
    {
        private readonly IRepository<JobCandidateApplication>? _repository;
        private readonly IApplicationRepository? _applicationRepository;

        public CancelApplicationCommandHandler(IRepository<JobCandidateApplication> repository)
        {
            _repository = repository;
        }

        public CancelApplicationCommandHandler(IApplicationRepository applicationRepository)
        {
            _applicationRepository = applicationRepository;
        }

        public async Task Handle(CancelApplicationCommand request, CancellationToken cancellationToken)
        {
            JobCandidateApplication? application = null;

            if (_repository != null)
            {
                application = await _repository.Get().FirstOrDefaultAsync(a => a.Id == request.ApplicationId, cancellationToken);
            }
            else if (_applicationRepository != null)
            {
                application = await _applicationRepository.GetByIdAsync(request.ApplicationId);
            }

            if (application == null)
            {
                throw new KeyNotFoundException($"Application with ID {request.ApplicationId} was not found.");
            }

            application.Cancel();

            if (_repository != null)
            {
                _repository.Update(application);
                await _repository.SaveChangesAsync();
            }
            else if (_applicationRepository != null)
            {
                _applicationRepository.Update(application);
                await _applicationRepository.SaveChangesAsync();
            }
        }
    }
}
