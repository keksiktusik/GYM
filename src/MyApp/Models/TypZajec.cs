using System;
using System.ComponentModel.DataAnnotations;

namespace MyApp.Models
{
    public class TypZajec
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Podaj nazwę typu zajęć.")]
        [MaxLength(100, ErrorMessage = "Nazwa nie może być dłuższa niż 100 znaków.")]
        [Display(Name = "Nazwa zajęć")]
        public string Nazwa { get; set; }

        [Display(Name = "Opis zajęć")]
        [DataType(DataType.MultilineText)]
        public string? Opis { get; set; }

        [Display(Name = "Poziom trudności")]
        [Required(ErrorMessage = "Podaj poziom trudności.")]
        public string? PoziomTrudnosci { get; set; }  // np. "Początkujący", "Średni", "Zaawansowany"

        [Display(Name = "Trener prowadzący")]
        [MaxLength(100)]
        public string? Trener { get; set; }

        [Display(Name = "Czas trwania (minuty)")]
        [Range(10, 300, ErrorMessage = "Czas trwania musi być między 10 a 300 minut.")]
        public int CzasTrwania { get; set; }

        [Display(Name = "Cena za zajęcia (PLN)")]
        [Range(0, 1000, ErrorMessage = "Cena musi być liczbą dodatnią.")]
        [DataType(DataType.Currency)]
        public decimal Cena { get; set; }

        [Display(Name = "Kategoria zajęć")]
        [MaxLength(50)]
        public string? Kategoria { get; set; }  // np. "Fitness", "Siłowe", "Relaksacyjne"

        [Display(Name = "Data utworzenia")]
        public DateTime DataUtworzenia { get; set; } = DateTime.Now;

        [Display(Name = "Aktywne")]
        public bool CzyAktywne { get; set; } = true;
    }
}
