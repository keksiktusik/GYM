using Microsoft.AspNetCore.Mvc;
using MyApp.Models;
using System.Collections.Generic;
using System.Linq;

namespace MyApp.Controllers
{
    public class TypZajecController : Controller
    {
        // tymczasowa lista w pamięci – zamiast bazy danych
        private static List<TypZajec> _lista = new List<TypZajec>();

        // GET: TypZajec
        public IActionResult Index()
        {
            return View(_lista);
        }

        // GET: TypZajec/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: TypZajec/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(TypZajec model)
        {
            if (ModelState.IsValid)
            {
                model.Id = _lista.Count + 1;
                _lista.Add(model);
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: TypZajec/Edit/5
        public IActionResult Edit(int id)
        {
            var item = _lista.FirstOrDefault(t => t.Id == id);
            if (item == null) return NotFound();
            return View(item);
        }

        // POST: TypZajec/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(TypZajec model)
        {
            var item = _lista.FirstOrDefault(t => t.Id == model.Id);
            if (item == null) return NotFound();

            if (ModelState.IsValid)
            {
                item.Nazwa = model.Nazwa;
                item.Opis = model.Opis;
                item.PoziomTrudnosci = model.PoziomTrudnosci;
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: TypZajec/Delete/5
        public IActionResult Delete(int id)
        {
            var item = _lista.FirstOrDefault(t => t.Id == id);
            if (item == null) return NotFound();
            return View(item);
        }

        // POST: TypZajec/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var item = _lista.FirstOrDefault(t => t.Id == id);
            if (item != null)
                _lista.Remove(item);
            return RedirectToAction(nameof(Index));
        }
    }
}
