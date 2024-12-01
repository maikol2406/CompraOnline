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
            float totalCarrito = 0;
            List<Producto> listaProductos = await db.obtenerProductos();
            List<Usuario> listaUsuarios = await db.obtenerUsuarios();
            int idUsuario = int.Parse(User.FindFirst("idUsuario")?.Value);
            Pedido pedido = await db.obtenerPedido(idUsuario);
            List<CarritoCompra> listaCarrito = new List<CarritoCompra>();
            foreach (var item in await db.obtenerCarritosCompras(pedido.idPedido))
            {
                listaCarrito.Add(item);
                foreach (var item2 in listaProductos)
                {
                    if (item.idProducto == item2.idProducto)
                    {
                        totalCarrito = (float)(totalCarrito + (item2.precioPromo * item.cantidad));
                    }
                }
            }
            if (pedido.idPedido == 0)
            {
                await db.insertarPedido(idUsuario, 0, false);
            }
            
            ViewBag.listaProductos = listaProductos;
            ViewBag.listaUsuarios = listaUsuarios;
            ViewBag.totalCarrito = totalCarrito;
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
            Producto producto = await db.obtenerProducto(carrito.idProducto);
            ViewBag.producto = producto.nombreProducto + " - " + producto.descripcionProducto;
            ViewBag.productoPrecio = producto.precio;
            ViewBag.imagen = producto.imagen;
            return View(carrito);
        }

        // POST: CarritoComprasController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ProductoCarrito(CarritoCompra carrito)
        {
            try
            {
                foreach (var item in await db.obtenerProductos())
                {
                    if (item.idProducto == carrito.idProducto && carrito.cantidad > item.stock)
                    {
                        ModelState.AddModelError("cantidad", $"La cantidad solicitada ({carrito.cantidad}) excede el stock disponible ({item.stock}).");
                        ViewBag.productoPrecio = item.precio;
                        return View(carrito);
                    }
                    else
                    {
                        if (item.idProducto == carrito.idPedido)
                        {
                            ViewBag.stock = item.stock;
                        }
                    }
                }
                
                float precioTotal = 0;
                int proceso = await db.insertarCarritoCompras(carrito);
                foreach (var item in await db.obtenerPedidos(carrito.idUsuario))
                {
                    if (item.idPedido == carrito.idPedido)
                    {
                        //CREAR UN METODO EN BASE DE DATOS PARA TRAER EL CARRITO Y LEER EL COSTO DE CADA ARTICULO CON EL ID DE PEDIDO Y PASARLO A PRECIOTOTAL.
                        foreach (var item2 in await db.obtenerCarritosCompras(item.idPedido))
                        {
                            precioTotal = precioTotal + item2.montoTotal;
                        }
                        //precioTotal = item.precioTotal + precioTotal;
                    }
                }
                int proceso2 = await db.actualizarCostoPedido(carrito.idPedido, precioTotal);
                return RedirectToAction("VerCarrito", "CarritoCompras");
            }
            catch
            {
                return View();
            }
        }

        public async Task<ActionResult> EditarProductoCarrito(int idCarrito)
        {
            CarritoCompra productoCarrito = new CarritoCompra();
            List<Producto> listaProductos = await db.obtenerProductos();
            productoCarrito = await db.obtenerProductoCarrito(idCarrito);
            foreach (var item in listaProductos)
            {
                if (productoCarrito.idProducto == item.idProducto)
                {
                    ViewBag.producto = item;
                    break;
                }
            }
            return View(productoCarrito);
        }

        // POST: CarritoComprasController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditarProductoCarrito(CarritoCompra carrito)
        {
            try
            {
                await db.actualizarProductoCarrito(carrito);
                return RedirectToAction(nameof(VerCarrito));
            }
            catch
            {
                return View();
            }
        }

        // GET: CarritoComprasController/Delete/5
        public async Task<ActionResult> EliminarProductoCarrito(int idCarrito)
        {
            CarritoCompra carrito = await db.obtenerProductoCarrito(idCarrito);
            int i = await db.eliminarProductoCarrito(carrito);
            return RedirectToAction(nameof(VerCarrito));
        }

    }
}
