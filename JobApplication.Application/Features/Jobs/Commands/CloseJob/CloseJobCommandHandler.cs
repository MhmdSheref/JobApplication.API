using JobApplication.Application.Interfaces;
using JobApplication.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace JobApplication.Application.Features.Jobs.Commands.CloseJob
{
    public class CloseJobCommandHandler : IRequestHandler<CloseJobCommand>
    {
        private readonly IRepository<Job> _jobRepository;
        private readonly ICurrentUserService _currentUserService;

        public CloseJobCommandHandler(IRepository<Job> jobRepository, ICurrentUserService currentUserService)
        {
            _jobRepository = jobRepository;
            _currentUserService = currentUserService;
        }

        public async Task Handle(CloseJobCommand request, CancellationToken cancellationToken)
        {
            if (!_currentUserService.IsAuthenticated || _currentUserService.Role != "Recruiter" || !_currentUserService.RecruiterId.HasValue)
            {
                throw new UnauthorizedAccessException("Only authenticated recruiters can close jobs.");
            }

            var recruiterId = request.RecruiterId ?? _currentUserService.RecruiterId.Value;

            var job = await _jobRepository.Get().FirstOrDefaultAsync(j => j.Id == request.Id, cancellationToken);
            if (job == null)
            {
                throw new KeyNotFoundException($"Job with id {request.Id} was not found.");
            }

            if (job.RecruiterId != recruiterId)
            {
                throw new UnauthorizedAccessException("Only the recruiter who owns the job can close it.");
            }

            job.Close(recruiterId);

            _jobRepository.Update(job);
            await _jobRepository.SaveChangesAsync();
        }
    }
}
