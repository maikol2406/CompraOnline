using CompraOnline.Data;
using CompraOnline.Models.Pedidos;
using CompraOnline.Models.Productos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CompraOnline.Controllers
{
    public class ProductosController : Controller
    {
        BaseDatos db = new BaseDatos();

        // GET: ProductosController
        [HttpGet]
        [Route("Productos/pedidos")]
        public async Task<ActionResult> Pedidos()
        {
            List<Pedido> listaPedidos = new List<Pedido>();
            int idUsuario = int.Parse(User.FindFirst("idUsuario")?.Value);
            listaPedidos = await db.obtenerPedidos(idUsuario);
            return View(listaPedidos);
        }

        // GET: ProductosController
        [HttpGet]
        [Route("Productos/productosXCategoria")]
        public async Task<ActionResult> ProductosXCategoria()
        {
            List<Producto> listaProductos = new List<Producto>();
            int idUsuario = int.Parse(User.FindFirst("idUsuario")?.Value);
            listaProductos = await db.obtenerProductos();
            List<Categoria> listaCategorias = await db.obtenerCategorias();
            ViewBag.listaCategorias = listaCategorias;
            return View(listaProductos);
        }

        // GET: ProductosController/Create
        //[Route("Productos/crearProducto")]
        public async Task<ActionResult> CrearProducto()
        {
            ViewBag.Categorias = new SelectList(await db.obtenerCategorias(), "idCategoria", "nombreCategoria");
            return View();
        }

        // POST: ProductosController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CrearProducto(Producto producto)
        {
            try
            {
                db.insertarProducto(producto);
                return RedirectToAction(nameof(ProductosXCategoria));
            }
            catch
            {
                return View(producto);
            }
        }

        // GET: ProductosController
        public ActionResult Index()
        {
            return View();
        }

        // GET: ProductosController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: ProductosController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ProductosController/Create
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

        // GET: ProductosController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: ProductosController/Edit/5
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

        // GET: ProductosController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: ProductosController/Delete/5
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
