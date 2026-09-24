using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Domain.Entities
{
    public class Job
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description  { get; set; }
        public bool IsActive { get; set; }
        public int? RecruiterId { get; set; }
        public DateTime? ClosedAt { get; set; }
        public int? ClosedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsClosed => ClosedAt != null;

        public void Close(int recruiterId)
        {
            if (IsClosed)
            {
                throw new InvalidOperationException("The job is already closed.");
            }

            ClosedAt = DateTime.UtcNow;
            ClosedBy = recruiterId;
            IsActive = false;
        }

        public void AutoClose()
        {
            if (IsClosed)
            {
                return;
            }

            ClosedAt = DateTime.UtcNow;
            IsActive = false;
        }
    }
}
