using JobApplication.Application.Features.JobCandidateApplications.Commands.CancelApplication;
using JobApplication.Application.Services;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JobApplication.API.Controllers
{
    /// <summary>
    /// Manages candidate applications.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ApplicationsController : ControllerBase
    {
        private readonly IMediator? _mediator;
        private readonly ApplicationService? _applicationService;

        public ApplicationsController(IMediator mediator, ApplicationService? applicationService = null)
        {
            _mediator = mediator;
            _applicationService = applicationService;
        }

        public ApplicationsController(ApplicationService applicationService)
        {
            _applicationService = applicationService;
        }

        /// <summary>
        /// Cancels a job candidate application.
        /// </summary>
        /// <param name="id">The id of the application to cancel.</param>
        /// <returns>No content on success.</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                if (_mediator != null)
                {
                    await _mediator.Send(new CancelApplicationCommand(id));
                }
                else if (_applicationService != null)
                {
                    await _applicationService.Cancel(id);
                }
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
