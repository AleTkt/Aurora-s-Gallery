using Microsoft.AspNetCore.Mvc;

namespace Catalogo_sis.Controllers
{
    public class ArtistasController : Controller
    {
        public IActionResult Artistas()
        {
            return View();
        }
    }
}
