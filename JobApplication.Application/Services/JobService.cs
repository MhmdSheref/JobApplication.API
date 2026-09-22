using JobApplication.Application.DTOs;
using JobApplication.Application.Interfaces;
using JobApplication.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobApplication.Application.Services
{
    public class JobService : IJobService
    {
        private readonly IRepository<Job> _jobRepository;
        private readonly ICurrentUserService? _currentUserService;

        public JobService(IRepository<Job> jobRepository, ICurrentUserService? currentUserService = null)
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
                RecruiterId = _currentUserService?.RecruiterId
            };
            await _jobRepository.AddAsync(job);
            await _jobRepository.SaveChangesAsync();

            return job.Id;
        }

        public IEnumerable<Job> GetAll()
        {
            var jobs = _jobRepository.Get().ToList();
            return jobs;
        }

        public Job? GetById(int id)
        {
            var job = _jobRepository.Get().FirstOrDefault(j => j.Id == id);
            return job;
        }

        public async Task Close(int id)
        {
            if (_currentUserService == null || !_currentUserService.IsAuthenticated || _currentUserService.Role != "Recruiter" || !_currentUserService.RecruiterId.HasValue)
            {
                throw new UnauthorizedAccessException("Only authenticated recruiters can close jobs.");
            }

            var job = await _jobRepository.Get().FirstOrDefaultAsync(j => j.Id == id);
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
