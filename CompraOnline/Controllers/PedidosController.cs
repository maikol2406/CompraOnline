using CompraOnline.Data;
using CompraOnline.Models.Pedidos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CompraOnline.Controllers
{
    public class PedidosController : Controller
    {
        BaseDatos db = new BaseDatos();

        // GET: PedidosController
        [Route("Pedidos/misPedidos")]
        public async Task<ActionResult> MisPedidos()
        {
            List<Pedido> listaPedidos = new List<Pedido>();
            int idUsuario = int.Parse(User.FindFirst("idUsuario")?.Value);
            listaPedidos = await db.obtenerPedidos(idUsuario);
            return View(listaPedidos);
        }

        // GET: PedidosController/Create
        public ActionResult CrearPedidos()
        {
            Pedido pedido = new Pedido();
            pedido.idUsuario = int.Parse(User.FindFirst("idUsuario")?.Value);
            return View();
        }

        // POST: PedidosController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CrearPedidos(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: PedidosController
        public ActionResult Index()
        {
            return View();
        }

        // GET: PedidosController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: PedidosController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: PedidosController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: PedidosController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: PedidosController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: PedidosController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: PedidosController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
