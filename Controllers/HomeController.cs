using Microsoft.AspNetCore.Mvc;
using RetroagirNfEntrada.Models;
using System.Diagnostics;

namespace RetroagirNfEntrada.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View(new BuscaNotaFiscalViewModel());
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
