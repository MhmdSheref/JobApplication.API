using JobApplication.Application.Features.Jobs.Commands.CloseJob;
using JobApplication.Application.Interfaces;
using JobApplication.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace JobApplication.Tests
{
    public class JobClosingTests
    {
        [Fact]
        public async Task Recruiter_Closes_Own_Job_Succeeds()
        {
            var job = new Job
            {
                Id = 1,
                Title = "Software Engineer",
                Description = "C# / .NET Developer",
                IsActive = true,
                RecruiterId = 10
            };

            var fakeRepo = new FakeJobRepository(job);
            var fakeUser = new FakeCurrentUserService
            {
                IsAuthenticated = true,
                Role = "Recruiter",
                RecruiterId = 10
            };

            var handler = new CloseJobCommandHandler(fakeRepo, fakeUser);

            var before = DateTime.UtcNow;
            await handler.Handle(new CloseJobCommand(1), default);
            var after = DateTime.UtcNow;

            Assert.False(job.IsActive);
            Assert.True(job.IsClosed);
            Assert.NotNull(job.ClosedAt);
            Assert.InRange(job.ClosedAt.Value, before, after);
            Assert.Equal(10, job.ClosedBy);
            Assert.True(fakeRepo.SaveChangesCalled);
        }

        [Fact]
        public async Task Unauthenticated_User_Throws_UnauthorizedAccessException()
        {
            var job = new Job { Id = 1, IsActive = true, RecruiterId = 10 };
            var fakeRepo = new FakeJobRepository(job);
            var fakeUser = new FakeCurrentUserService { IsAuthenticated = false };

            var handler = new CloseJobCommandHandler(fakeRepo, fakeUser);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                handler.Handle(new CloseJobCommand(1), default));
        }

        [Fact]
        public async Task NonRecruiter_Role_Throws_UnauthorizedAccessException()
        {
            var job = new Job { Id = 1, IsActive = true, RecruiterId = 10 };
            var fakeRepo = new FakeJobRepository(job);
            var fakeUser = new FakeCurrentUserService
            {
                IsAuthenticated = true,
                Role = "Candidate",
                RecruiterId = 10
            };

            var handler = new CloseJobCommandHandler(fakeRepo, fakeUser);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                handler.Handle(new CloseJobCommand(1), default));
        }

        [Fact]
        public async Task Recruiter_Closing_Other_Recruiters_Job_Throws_UnauthorizedAccessException()
        {
            var job = new Job { Id = 1, IsActive = true, RecruiterId = 10 };
            var fakeRepo = new FakeJobRepository(job);
            var fakeUser = new FakeCurrentUserService
            {
                IsAuthenticated = true,
                Role = "Recruiter",
                RecruiterId = 99 // different recruiter
            };

            var handler = new CloseJobCommandHandler(fakeRepo, fakeUser);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                handler.Handle(new CloseJobCommand(1), default));
        }

        [Fact]
        public async Task NonExistent_Job_Throws_KeyNotFoundException()
        {
            var fakeRepo = new FakeJobRepository();
            var fakeUser = new FakeCurrentUserService
            {
                IsAuthenticated = true,
                Role = "Recruiter",
                RecruiterId = 10
            };

            var handler = new CloseJobCommandHandler(fakeRepo, fakeUser);

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                handler.Handle(new CloseJobCommand(999), default));
        }

        [Fact]
        public async Task Closing_Already_Closed_Job_Throws_InvalidOperationException()
        {
            var job = new Job
            {
                Id = 1,
                IsActive = false,
                RecruiterId = 10,
                ClosedAt = DateTime.UtcNow.AddHours(-1),
                ClosedBy = 10
            };

            var fakeRepo = new FakeJobRepository(job);
            var fakeUser = new FakeCurrentUserService
            {
                IsAuthenticated = true,
                Role = "Recruiter",
                RecruiterId = 10
            };

            var handler = new CloseJobCommandHandler(fakeRepo, fakeUser);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                handler.Handle(new CloseJobCommand(1), default));
        }

        private class FakeJobRepository : IRepository<Job>
        {
            private readonly List<Job> _items;
            public bool SaveChangesCalled { get; private set; }

            public FakeJobRepository(params Job[] jobs)
            {
                _items = jobs.ToList();
            }

            public Task AddAsync(Job entity)
            {
                _items.Add(entity);
                return Task.CompletedTask;
            }

            public IQueryable<Job> Get()
            {
                return _items.AsQueryable();
            }

            public void Remove(Job entity)
            {
                _items.Remove(entity);
            }

            public void Update(Job entity)
            {
                var idx = _items.FindIndex(j => j.Id == entity.Id);
                if (idx >= 0) _items[idx] = entity;
            }

            public Task SaveChangesAsync()
            {
                SaveChangesCalled = true;
                return Task.CompletedTask;
            }
        }

        private class FakeCurrentUserService : ICurrentUserService
        {
            public bool IsAuthenticated { get; set; } = true;
            public int? UserId { get; set; } = 1;
            public string? Role { get; set; } = "Recruiter";
            public int? RecruiterId { get; set; } = 1;
            public int? CandidateId { get; set; } = null;
        }
    }
}
