using System.ComponentModel.DataAnnotations;

namespace MyApp.Models
{
    public class TrainingEvent
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Tytuł / typ treningu")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "Opis / notatka")]
        public string? Description { get; set; }

        [Required]
        [Display(Name = "Data rozpoczęcia")]
        public DateTime Start { get; set; }

        [Required]
        [Display(Name = "Data zakończenia")]
        public DateTime End { get; set; }

        [Display(Name = "Rodzaj treningu")]
        public string? Category { get; set; } // np. Cardio, Siłowy, Grupowy, Stretching
    }
}
