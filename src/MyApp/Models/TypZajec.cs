using System.ComponentModel.DataAnnotations;

namespace MyApp.Models
{
    public class TypZajec
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Podaj nazwę typu zajęć.")]
        [Display(Name = "Nazwa zajęć")]
        public string Nazwa { get; set; }

        [Display(Name = "Opis zajęć")]
        public string? Opis { get; set; }

        [Display(Name = "Poziom trudności")]
        public string? PoziomTrudnosci { get; set; }
    }
}
