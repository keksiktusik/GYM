using System.ComponentModel.DataAnnotations;

namespace MyApp.Models
{
    public class Person
    {
        [Display(Name = "Wzrost (cm)")]
        [Range(1, 300, ErrorMessage = "Podaj wzrost w cm (1–300).")]
        public double HeightCm { get; set; }

        [Display(Name = "Waga (kg)")]
        [Range(1, 400, ErrorMessage = "Podaj wagę w kg (1–400).")]
        public double WeightKg { get; set; }

        public double? Bmi { get; set; }
        public string? Category { get; set; }

        public void Compute()
        {
            if (HeightCm <= 0 || WeightKg <= 0)
            {
                Bmi = null;
                Category = "Błędne dane";
                return;
            }

            var h = HeightCm / 100.0;
            var bmi = WeightKg / (h * h);
            Bmi = System.Math.Round(bmi, 1);
            Category = bmi switch
            {
                < 18.5 => "Niedowaga",
                < 25.0 => "Waga prawidłowa",
                < 30.0 => "Nadwaga",
                _ => "Otyłość"
            };
        }
    }
}
