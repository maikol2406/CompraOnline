using System.Diagnostics;
using CompraOnline.Data;
using CompraOnline.Models;
using CompraOnline.Models.Productos;
using Microsoft.AspNetCore.Mvc;

namespace CompraOnline.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        BaseDatos db = new BaseDatos();

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            List<Producto> listaProductos = await db.obtenerProductosPromo();
            return View(listaProductos);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
