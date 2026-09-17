namespace JobApplication.Application.Interfaces
{
    public interface ICurrentUserService
    {
        int? UserId { get; }
        string? Role { get; }
        int? RecruiterId { get; }
        int? CandidateId { get; }
        bool IsAuthenticated { get; }
    }
}
