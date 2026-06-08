using Core.Models;

namespace Core.DTOs
{
    public class CreateReportDTO
    {
        public int? ReportedUserId { get; set; }
        public int? ReportedSetId { get; set; }

        public ReportReason Reason { get; set; }
        public string? CustomReason { get; set; }
    }
}
