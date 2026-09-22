using JobApplication.Application.DTOs;
using JobApplication.Application.Features.Jobs.Commands.CreateJob;
using JobApplication.Application.Features.Jobs.Queries.GetAllJobs;
using JobApplication.Application.Features.Jobs.Queries.GetJobById;
using JobApplication.Application.Interfaces;
using JobApplication.Application.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JobApplication.API.Controllers
{
    /// <summary>
    /// Manages job postings.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class JobsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly JobService? _jobService;

        public JobsController(IMediator mediator, JobService? jobService = null)
        {
            _mediator = mediator;
            _jobService = jobService;
        }

        /// <summary>
        /// Gets all job postings.
        /// </summary>
        /// <returns>The list of jobs.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var jobs = await _mediator.Send(new GetAllJobsQuery());
            return Ok(new { jobs });
        }

        /// <summary>
        /// Gets a single job posting by its id.
        /// </summary>
        /// <param name="id">The job id.</param>
        /// <returns>The matching job.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var job = await _mediator.Send(new GetJobByIdQuery() { Id = id });
            if (job is null) return NotFound(new
            {
                message = "invalid Id"
            });
            return Ok(new { job });
        }

        /// <summary>
        /// Creates a new job posting.
        /// </summary>
        /// <param name="createJobDto">The job title and description.</param>
        /// <returns>The id of the newly created job.</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(CreateJobDto createJobDto)
        {
            var id = await _mediator.Send(new CreateJobCommand() { Title = createJobDto.Title, Description = createJobDto.Description });

            return Ok(new
            {
                id = id
            });
        }

        [Authorize]
        [HttpPut("{id}/close")]
        public async Task<IActionResult> Close(int id)
        {
            if (_jobService == null)
            {
                throw new InvalidOperationException("JobService is not available.");
            }

            try
            {
                await _jobService.Close(id);
                return Ok(new { message = "Job closed successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
