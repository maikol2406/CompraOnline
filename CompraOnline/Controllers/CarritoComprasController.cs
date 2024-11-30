using CompraOnline.Data;
using CompraOnline.Models.CarritoCompras;
using CompraOnline.Models.Pedidos;
using CompraOnline.Models.Productos;
using CompraOnline.Models.Usuarios;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CompraOnline.Controllers
{
    public class CarritoComprasController : Controller
    {
        BaseDatos db = new BaseDatos();

        // GET: CarritoComprasController
        public async Task<ActionResult> VerCarrito()
        {
            int idUsuario = int.Parse(User.FindFirst("idUsuario")?.Value);
            Pedido pedido = await db.obtenerPedido(idUsuario);
            List<CarritoCompra> listaCarrito = new List<CarritoCompra>();
            foreach (var item in await db.obtenerCarritoCompras(pedido.idPedido))
            {
                listaCarrito.Add(item);
            }
            if (pedido.idPedido == 0)
            {
                await db.insertarPedido(idUsuario, 0, false);
            }
            List<Producto> listaProductos = await db.obtenerProductos();
            List<Usuario> listaUsuarios = await db.obtenerUsuarios();
            ViewBag.listaProductos = listaProductos;
            ViewBag.listaUsuarios = listaUsuarios;
            return View(listaCarrito);
        }

        // GET: CarritoComprasController/Create
        public async Task<ActionResult> ProductoCarrito(int idProducto)
        {
            Pedido pedido = new Pedido();
            CarritoCompra carrito = new CarritoCompra();
            int idUsuario = int.Parse(User.FindFirst("idUsuario")?.Value);
            pedido = await db.obtenerPedido(idUsuario);
            if (pedido.estadoPedido || pedido.idPedido == 0)
            {
                int proceso = await db.insertarPedido(idUsuario, 0, false);
                pedido = await db.obtenerPedido(idUsuario);
                carrito.idPedido = pedido.idPedido;
            }
            else
            {
                carrito.idPedido = pedido.idPedido;
            }
            
            carrito.idUsuario = idUsuario;
            
            carrito.idProducto = idProducto;
            return View(carrito);
        }

        // POST: CarritoComprasController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ProductoCarrito(CarritoCompra carrito)
        {
            try
            {
                float precioTotal = 0;
                int proceso = await db.insertarCarritoCompras(carrito);
                foreach (var item in await db.obtenerPedidos(carrito.idUsuario))
                {
                    if (item.idPedido == carrito.idPedido)
                    {
                        //CREAR UN METODO EN BASE DE DATOS PARA TRAER EL CARRITO Y LEER EL COSTO DE CADA ARTICULO CON EL ID DE PEDIDO Y PASARLO A PRECIOTOTAL.
                        foreach (var item2 in await db.obtenerCarritoCompras(item.idPedido))
                        {
                            precioTotal = precioTotal + item2.montoTotal;
                        }
                        //precioTotal = item.precioTotal + precioTotal;
                    }
                }
                int proceso2 = await db.actualizarCostoPedido(carrito.idPedido, precioTotal);
                return RedirectToAction("ProductosXCategoria", "Productos");
            }
            catch
            {
                return View();
            }
        }

        // GET: CarritoComprasController
        public ActionResult Index()
        {
            return View();
        }

        // GET: CarritoComprasController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: CarritoComprasController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: CarritoComprasController/Create
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

        // GET: CarritoComprasController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: CarritoComprasController/Edit/5
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

        // GET: CarritoComprasController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: CarritoComprasController/Delete/5
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
