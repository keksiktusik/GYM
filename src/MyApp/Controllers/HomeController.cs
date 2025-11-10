using Microsoft.AspNetCore.Mvc;
using MyApp.Models;

namespace MyApp.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View(new Person());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(Person model)
        {
            if (!ModelState.IsValid)
                return View(model);

            model.Compute();
            return View("Result", model); // <- to musi być View("Result", model)
        }

        public IActionResult Result(Person model)
        {
            return View(model);
        }
    }
}
