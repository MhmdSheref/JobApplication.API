using JobApplication.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace JobApplication.Domain.Entities
{
    public  class JobCandidateApplication
    {
        public int Id { get; set; }
        public int CandidateId { get; set; }
        [ForeignKey(nameof(CandidateId))]
        public Candidate Candidate { get; set; }
        public int JobId { get; set; }
        [ForeignKey(nameof(JobId))]
        public Job Job { get; set; }
        public JobApplicationStatus JobApplicationStatus { get; set; }
        public DateTime AppliedAt { get; set; }
        public DateTime StatusUpdatedAt { get; set; }
        public DateTime? CancelledAt { get; set; }

        [NotMapped]
        public JobApplicationStatus Status
        {
            get => JobApplicationStatus;
            set => JobApplicationStatus = value;
        }

        public void Cancel()
        {
            if (JobApplicationStatus != JobApplicationStatus.Applied && JobApplicationStatus != JobApplicationStatus.UnderReview)
            {
                throw new InvalidOperationException($"Cannot cancel application with status '{JobApplicationStatus}'. Only applications in Applied or UnderReview status can be cancelled.");
            }

            JobApplicationStatus = JobApplicationStatus.Cancelled;
            var now = DateTime.UtcNow;
            CancelledAt = now;
            StatusUpdatedAt = now;
        }
    }
}
