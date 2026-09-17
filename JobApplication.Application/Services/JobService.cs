using JobApplication.Application.DTOs;
using JobApplication.Application.Interfaces;
using JobApplication.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Application.Services
{
    public class JobService
    {
        private readonly IJobRepository _jobRepository;
        private readonly ICurrentUserService _currentUserService;

        public JobService(IJobRepository jobRepository, ICurrentUserService currentUserService)
        {
            _jobRepository = jobRepository;
            _currentUserService = currentUserService;
        }

        public async Task<int> CreateAsync(CreateJobDto createJobDto)
        {   
            var job = new Job()
            {
                Title = createJobDto.Title,
                Description = createJobDto.Description,
                IsActive = true,
                RecruiterId = _currentUserService.RecruiterId
            };
            await _jobRepository.InsertAsync(job);
            await _jobRepository.SaveChangesAsync();

            return job.Id; 
        }

        public async Task Close(int id)
        {
            if (!_currentUserService.IsAuthenticated || _currentUserService.Role != "Recruiter" || !_currentUserService.RecruiterId.HasValue)
            {
                throw new UnauthorizedAccessException("Only authenticated recruiters can close jobs.");
            }

            var job = await _jobRepository.GetByIdAsync(id);
            if (job == null)
            {
                throw new KeyNotFoundException($"Job with id {id} was not found.");
            }

            if (job.RecruiterId != _currentUserService.RecruiterId.Value)
            {
                throw new UnauthorizedAccessException("Only the recruiter who owns the job can close it.");
            }

            job.Close(_currentUserService.RecruiterId.Value);

            _jobRepository.Update(job);
            await _jobRepository.SaveChangesAsync();
        }
    }
}
