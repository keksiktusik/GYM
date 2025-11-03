using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MyApp.Controllers
{
    [Authorize(Roles = "Klient")]
    public class ClientController : Controller
    {
        public IActionResult Index() => View();
    }
}
