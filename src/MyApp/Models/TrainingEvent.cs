using System.ComponentModel.DataAnnotations;

namespace MyApp.Models
{
    public class TrainingEvent
    {
        public int Id { get; set; }

        // 🔹 Właściciel wydarzenia
        [Required]
        public string UserId { get; set; } = string.Empty;


        // 🔹 Nazwa / typ treningu
        [Required]
        [Display(Name = "Tytuł / typ treningu")]
        public string Title { get; set; } = string.Empty;


        // 🔹 Opis
        [Display(Name = "Opis / notatka")]
        public string? Description { get; set; }


        // 🔹 Start treningu
        [Required]
        [Display(Name = "Data rozpoczęcia")]
        public DateTime Start { get; set; }


        // 🔹 Koniec treningu
        [Required]
        [Display(Name = "Data zakończenia")]
        public DateTime End { get; set; }


        // 🔹 Kategoria (do raportów i statystyk)
        [Display(Name = "Rodzaj treningu")]
        public string? Category { get; set; } // np. Cardio, Siłowy, Grupowy, Stretching


        // 🔸 ⏰ NOWE POLE – czy wysłano przypomnienie dzień przed
        public bool ReminderSent { get; set; } = false;


        // 🔸  kiedy utworzono wydarzenie
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public bool IsRecurring { get; set; } = false;

// "Daily", "Weekly", "Monthly"
public string? RecurrenceInterval { get; set; }

// Dni tygodnia dla Weekly — np. "Mon,Wed,Fri"
public string? RecurrenceDays { get; set; }

// Do kiedy powtarzać cykl
public DateTime? RecurrenceEndDate { get; set; }
 
 public Guid? RecurrenceGroupId { get; set; }
    }
}
