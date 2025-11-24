using System;
using System.ComponentModel.DataAnnotations;

namespace MyApp.Models
{
    public class GalleryPhoto
    {
        public int Id { get; set; }

        [Required]
        public string FilePath { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime DateAdded { get; set; } = DateTime.Now;
    }
}
