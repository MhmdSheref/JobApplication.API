using JobApplication.Domain.Enums;

namespace JobApplication.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public int? RecruiterId { get; set; }
        public int? CandidateId { get; set; }
    }
}
