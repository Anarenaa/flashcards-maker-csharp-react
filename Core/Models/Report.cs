using System.ComponentModel.DataAnnotations;

namespace Core.Models
{
    public class Report : IHasCreationDate
    {
        public int Id { get; set; }
        public int ReporterId { get; set; }
        public User Reporter { get; set; } = null!;
        public int? ReportedUserId { get; set; }
        public User? ReportedUser { get; set; }
        public int? ReportedSetId { get; set; }
        public Set? ReportedSet { get; set; }
        public ReportReason Reason { get; set; }
        public string? CustomReason { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsResolved { get; set; } = false;
    }
    public enum ReportReason
    {
        [Display(Name = "Спам")]
        Spam,

        [Display(Name = "Неприйнятний контент")]
        InappropriateContent,

        [Display(Name = "Образи або цькування")]
        Harassment,

        [Display(Name = "Порушення авторських прав")]
        CopyrightViolation,

        [Display(Name = "Інше")]
        Other
    }
}
