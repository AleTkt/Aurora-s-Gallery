using Microsoft.AspNetCore.Mvc;

namespace Catalogo_sis.Controllers
{
    public class GaleriaController : Controller
    {
        public IActionResult Galeria()
        {
            return View();
        }
    }
}
