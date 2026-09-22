using JobApplication.API.Controllers;
using JobApplication.Application.Features.JobCandidateApplications.Commands.CancelApplication;
using JobApplication.Application.Interfaces;
using JobApplication.Application.Services;
using JobApplication.Domain.Entities;
using JobApplication.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace JobApplication.Tests
{
    public class ApplicationCancellationTests
    {
        // 1. Domain Tests: Applied -> Cancelled succeeds and CancelledAt is populated
        [Fact]
        public void Applied_To_Cancelled_Succeeds_And_Sets_CancelledAt()
        {
            var application = new JobCandidateApplication
            {
                Id = 1,
                JobApplicationStatus = JobApplicationStatus.Applied,
                AppliedAt = DateTime.UtcNow.AddDays(-1)
            };

            var before = DateTime.UtcNow;
            application.Cancel();
            var after = DateTime.UtcNow;

            Assert.Equal(JobApplicationStatus.Cancelled, application.JobApplicationStatus);
            Assert.NotNull(application.CancelledAt);
            Assert.InRange(application.CancelledAt.Value, before, after);
            Assert.InRange(application.StatusUpdatedAt, before, after);
        }

        // 2. Domain Tests: UnderReview -> Cancelled succeeds
        [Fact]
        public void UnderReview_To_Cancelled_Succeeds()
        {
            var application = new JobCandidateApplication
            {
                Id = 1,
                JobApplicationStatus = JobApplicationStatus.UnderReview,
                AppliedAt = DateTime.UtcNow.AddDays(-2)
            };

            application.Cancel();

            Assert.Equal(JobApplicationStatus.Cancelled, application.JobApplicationStatus);
            Assert.NotNull(application.CancelledAt);
        }

        // 3. Domain Tests: Other statuses -> cancellation is rejected
        [Theory]
        [InlineData(JobApplicationStatus.InterView)]
        [InlineData(JobApplicationStatus.Accepted)]
        [InlineData(JobApplicationStatus.Rejected)]
        [InlineData(JobApplicationStatus.Cancelled)]
        public void OtherStatuses_Cancellation_IsRejected(JobApplicationStatus status)
        {
            var application = new JobCandidateApplication
            {
                Id = 1,
                JobApplicationStatus = status
            };

            var ex = Assert.Throws<InvalidOperationException>(() => application.Cancel());
            Assert.Contains(status.ToString(), ex.Message);
        }

        // 4. Application Service: Non-existent application throws KeyNotFoundException
        [Fact]
        public async Task ApplicationService_NonExistentApplication_ThrowsKeyNotFoundException()
        {
            var fakeRepo = new FakeApplicationRepository();
            var service = new ApplicationService(fakeRepo);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.Cancel(999));
        }

        // 5. API Layer: Non-existent application returns 404
        [Fact]
        public async Task ApplicationsController_NonExistentApplication_Returns404NotFound()
        {
            var fakeRepo = new FakeApplicationRepository();
            var service = new ApplicationService(fakeRepo);
            var controller = new ApplicationsController(service);

            var result = await controller.Cancel(999);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        // 6. API Layer: Successful DELETE returns 204
        [Fact]
        public async Task ApplicationsController_SuccessfulCancel_Returns204NoContent()
        {
            var app = new JobCandidateApplication
            {
                Id = 1,
                JobApplicationStatus = JobApplicationStatus.Applied
            };
            var fakeRepo = new FakeApplicationRepository(app);
            var service = new ApplicationService(fakeRepo);
            var controller = new ApplicationsController(service);

            var result = await controller.Cancel(1);

            Assert.IsType<NoContentResult>(result);
            Assert.Equal(JobApplicationStatus.Cancelled, app.JobApplicationStatus);
            Assert.NotNull(app.CancelledAt);
            Assert.True(fakeRepo.SaveChangesCalled);
        }

        // 7. API Layer: Disallowed status returns 400 BadRequest
        [Fact]
        public async Task ApplicationsController_DisallowedStatus_Returns400BadRequest()
        {
            var app = new JobCandidateApplication
            {
                Id = 1,
                JobApplicationStatus = JobApplicationStatus.Accepted
            };
            var fakeRepo = new FakeApplicationRepository(app);
            var service = new ApplicationService(fakeRepo);
            var controller = new ApplicationsController(service);

            var result = await controller.Cancel(1);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }

        // 8. CQRS Handler: Non-existent application throws KeyNotFoundException
        [Fact]
        public async Task CancelApplicationCommandHandler_NonExistentApplication_ThrowsKeyNotFoundException()
        {
            var fakeRepo = new FakeApplicationRepository();
            var handler = new CancelApplicationCommandHandler(fakeRepo);

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                handler.Handle(new CancelApplicationCommand(999), default));
        }

        // 9. CQRS Handler: Applied application cancels successfully and sets timestamp
        [Fact]
        public async Task CancelApplicationCommandHandler_AppliedApplication_CancelsSuccessfully()
        {
            var app = new JobCandidateApplication
            {
                Id = 1,
                JobApplicationStatus = JobApplicationStatus.Applied
            };
            var fakeRepo = new FakeApplicationRepository(app);
            var handler = new CancelApplicationCommandHandler(fakeRepo);

            var before = DateTime.UtcNow;
            await handler.Handle(new CancelApplicationCommand(1), default);
            var after = DateTime.UtcNow;

            Assert.Equal(JobApplicationStatus.Cancelled, app.JobApplicationStatus);
            Assert.NotNull(app.CancelledAt);
            Assert.InRange(app.CancelledAt.Value, before, after);
            Assert.True(fakeRepo.SaveChangesCalled);
        }

        // 10. CQRS Handler: UnderReview application cancels successfully
        [Fact]
        public async Task CancelApplicationCommandHandler_UnderReviewApplication_CancelsSuccessfully()
        {
            var app = new JobCandidateApplication
            {
                Id = 1,
                JobApplicationStatus = JobApplicationStatus.UnderReview
            };
            var fakeRepo = new FakeApplicationRepository(app);
            var handler = new CancelApplicationCommandHandler(fakeRepo);

            await handler.Handle(new CancelApplicationCommand(1), default);

            Assert.Equal(JobApplicationStatus.Cancelled, app.JobApplicationStatus);
            Assert.NotNull(app.CancelledAt);
            Assert.True(fakeRepo.SaveChangesCalled);
        }

        // 11. CQRS Handler: Disallowed status throws InvalidOperationException
        [Fact]
        public async Task CancelApplicationCommandHandler_DisallowedStatus_ThrowsInvalidOperationException()
        {
            var app = new JobCandidateApplication
            {
                Id = 1,
                JobApplicationStatus = JobApplicationStatus.Accepted
            };
            var fakeRepo = new FakeApplicationRepository(app);
            var handler = new CancelApplicationCommandHandler(fakeRepo);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                handler.Handle(new CancelApplicationCommand(1), default));
        }

        // Minimal fake repository for test isolation without external mocking packages
        private class FakeApplicationRepository : IApplicationRepository
        {
            private readonly Dictionary<int, JobCandidateApplication> _items = new();
            public bool SaveChangesCalled { get; private set; }

            public FakeApplicationRepository(params JobCandidateApplication[] applications)
            {
                foreach (var app in applications)
                {
                    _items[app.Id] = app;
                }
            }

            public Task<JobCandidateApplication?> GetByIdAsync(int id)
            {
                _items.TryGetValue(id, out var app);
                return Task.FromResult(app);
            }

            public void Update(JobCandidateApplication application)
            {
                _items[application.Id] = application;
            }

            public Task SaveChangesAsync()
            {
                SaveChangesCalled = true;
                return Task.CompletedTask;
            }
        }
    }
}
