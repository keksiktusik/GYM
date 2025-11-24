using Microsoft.AspNetCore.Mvc;
using MyApp.Data;
using MyApp.Models;

namespace MyApp.Controllers
{
    public class GalleryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public GalleryController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // Galeria dla użytkowników
        public IActionResult Index()
        {
            var photos = _context.GalleryPhotos
                .OrderByDescending(p => p.DateAdded)
                .ToList();

            return View(photos);
        }

        // Admin - widok dodawania
        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add(IFormFile photo, string? description)
        {
            if (photo == null)
            {
                ModelState.AddModelError("", "Wybierz zdjęcie.");
                return View();
            }

            string folderPath = Path.Combine(_env.WebRootPath, "uploads/gallery");
            Directory.CreateDirectory(folderPath);

            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(photo.FileName);
            string fullPath = Path.Combine(folderPath, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await photo.CopyToAsync(stream);
            }

            var entry = new GalleryPhoto
            {
                FilePath = "/uploads/gallery/" + fileName,
                Description = description
            };

            _context.GalleryPhotos.Add(entry);
            _context.SaveChanges();

            TempData["msg"] = "Zdjęcie dodano pomyślnie!";
            return RedirectToAction("AdminList");
        }

        // Admin lista zdjęć
        public IActionResult AdminList()
        {
            return View(_context.GalleryPhotos.OrderByDescending(p => p.DateAdded).ToList());
        }

        // Usuwanie zdjęcia
        public IActionResult Delete(int id)
        {
            var img = _context.GalleryPhotos.Find(id);
            if (img == null) return NotFound();

            _context.GalleryPhotos.Remove(img);
            _context.SaveChanges();

            return RedirectToAction("AdminList");
        }
    }
}
