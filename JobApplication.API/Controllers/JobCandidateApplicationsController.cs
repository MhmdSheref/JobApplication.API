using JobApplication.Application.DTOs;
using JobApplication.Application.Features.JobCandidateApplications.Commands.CreateJobCandidateApplication;
using JobApplication.Application.Features.JobCandidateApplications.Commands.UpdateJobCandidateApplicationStatus;
using JobApplication.Application.Features.JobCandidateApplications.Queries.GetAllJobCandidateApplications;
using JobApplication.Application.Features.JobCandidateApplications.Queries.GetJobCandidateApplicationById;
using JobApplication.Application.Interfaces;
using JobApplication.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JobApplication.API.Controllers
{
    /// <summary>
    /// Manages candidate applications submitted against job postings.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class JobCandidateApplicationsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public JobCandidateApplicationsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Gets all job candidate applications.
        /// </summary>
        /// <returns>The list of applications.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var applications = await _mediator.Send(new GetAllJobCandidateApplicationsQuery());
            return Ok(new { applications });
        }

        /// <summary>
        /// Gets a single job candidate application by its id.
        /// </summary>
        /// <param name="id">The application id.</param>
        /// <returns>The matching application.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var application = await _mediator.Send(new GetJobCandidateApplicationByIdQuery() { Id = id });
            if (application is null) return NotFound(new
            {
                message = "invalid Id"
            });
            return Ok(new { application });
        }

        /// <summary>
        /// Creates a new job candidate application.
        /// </summary>
        /// <param name="createApplicationDto">The job and candidate ids.</param>
        /// <returns>The id of the newly created application.</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(CreateJobCandidateApplicationDto createApplicationDto)
        {
            var id = await _mediator.Send(new CreateJobCandidateApplicationCommand() { JobId = createApplicationDto.JobId, CandidateId = createApplicationDto.CandidateId });
            return Ok(new { id = id });
        }

        /// <summary>
        /// Updates the status of an existing job candidate application.
        /// </summary>
        /// <param name="id">The application id.</param>
        /// <param name="status">The new application status.</param>
        /// <returns>The id of the updated application.</returns>
        [HttpPatch("{id}/{status}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, JobApplicationStatus status)
        {
            var job = await _mediator.Send(new UpdateJobCandidateApplicationStatusCommand() { Id = id, Status = status });
            if (job == null) return NotFound();
            return Ok(new { id = job.Id });
        }
    }
}
