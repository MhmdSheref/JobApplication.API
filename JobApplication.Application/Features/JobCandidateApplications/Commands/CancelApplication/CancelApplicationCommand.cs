using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Application.Features.JobCandidateApplications.Commands.CancelApplication
{
    public class CancelApplicationCommand : IRequest
    {
        public int ApplicationId { get; set; }
        public int? RequesterId { get; set; }

        public CancelApplicationCommand()
        {
        }

        public CancelApplicationCommand(int applicationId, int? requesterId = null)
        {
            ApplicationId = applicationId;
            RequesterId = requesterId;
        }
    }
}
