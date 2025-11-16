using System.ComponentModel.DataAnnotations;

namespace MyApp.Models
{
    public class TrainingEvent
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;

        [Required]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime Start { get; set; }

        public DateTime End { get; set; }

        public string? Category { get; set; }

        public bool ReminderSent { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsRecurring { get; set; } = false;

        public string? RecurrenceInterval { get; set; }

        public string? RecurrenceDays { get; set; }

        public DateTime? RecurrenceEndDate { get; set; }

        public Guid? RecurrenceGroupId { get; set; }
        public bool IsAdminEvent { get; set; } = false;   
        public bool? IsApproved { get; set; } = null;     


    }
}

