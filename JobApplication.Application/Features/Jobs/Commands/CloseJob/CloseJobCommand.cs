using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Application.Features.Jobs.Commands.CloseJob
{
    public class CloseJobCommand : IRequest
    {
        public int Id { get; set; }
        public int? RecruiterId { get; set; }

        public int JobId
        {
            get => Id;
            set => Id = value;
        }

        public CloseJobCommand()
        {
        }

        public CloseJobCommand(int id, int? recruiterId = null)
        {
            Id = id;
            RecruiterId = recruiterId;
        }
    }
}
