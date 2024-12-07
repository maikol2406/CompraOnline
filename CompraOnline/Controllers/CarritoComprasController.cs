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
                var listaProductos = await db.obtenerProductos();
                var listaCarrito = await db.obtenerCarritosCompras(carrito.idPedido);
                float precioTotal1 = 0;

                var producto = listaProductos.FirstOrDefault(p => p.idProducto == carrito.idProducto);
                if (producto == null)
                {
                    ModelState.AddModelError("", "El producto no existe.");
                    return View(carrito);
                }

                if (carrito.cantidad > producto.stock)
                {
                    ModelState.AddModelError("cantidad", $"La cantidad solicitada ({carrito.cantidad}) excede el stock disponible ({producto.stock}).");
                    ViewBag.productoPrecio = producto.precio;
                    ViewBag.producto = producto.nombreProducto + " - " + producto.descripcionProducto;
                    ViewBag.imagen = producto.imagen;
                    return View(carrito);
                }

                var itemEnCarrito = listaCarrito.FirstOrDefault(c => c.idProducto == carrito.idProducto);
                if (itemEnCarrito != null && (itemEnCarrito.cantidad + carrito.cantidad) > producto.stock)
                {
                    ModelState.AddModelError("cantidad", $"La cantidad total ({itemEnCarrito.cantidad + carrito.cantidad}) excede el stock disponible ({producto.stock}).");
                    ViewBag.productoPrecio = producto.precio;
                    ViewBag.producto = producto.nombreProducto + " - " + producto.descripcionProducto;
                    ViewBag.imagen = producto.imagen;
                    return View(carrito);
                }

                if (itemEnCarrito != null)
                {

                    int proceso = await db.insertarCarritoCompras(carrito);
                    foreach (var item in await db.obtenerPedidos(carrito.idUsuario))
                    {
                        if (item.idPedido == carrito.idPedido)
                        {
                            foreach (var item2 in listaCarrito)
                            {
                                precioTotal1 = precioTotal1 + item2.montoTotal;
                            }
                        }
                    }
                    int proceso2 = await db.actualizarCostoPedido(carrito.idPedido, precioTotal1);
                }
                else
                {
                    carrito.montoTotal = carrito.cantidad * producto.precio;
                    foreach (var item in await db.obtenerPedidosPendientes(carrito.idUsuario))
                    {
                        if (item.idPedido == carrito.idPedido)
                        {
                            foreach (var item2 in listaCarrito)
                            {
                                precioTotal1 = precioTotal1 + item2.montoTotal;
                            }
                            precioTotal1 += carrito.montoTotal;
                        }
                    }
                    int proceso2 = await db.actualizarCostoPedido(carrito.idPedido, precioTotal1);
                    await db.insertarCarritoCompras(carrito);
                }

                //producto.stock -= carrito.cantidad;
                //db.actualizarCantidadProducto(carrito.idProducto, producto.stock);


                return RedirectToAction("VerCarrito", "CarritoCompras");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Ocurrió un error al procesar la solicitud. Inténtelo nuevamente.");
                return View(carrito);
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
            try
            {
                CarritoCompra carrito = await db.obtenerProductoCarrito(idCarrito);
                if (carrito == null)
                {
                    return NotFound();
                }

                int resultado = await db.eliminarProductoCarrito(carrito);

                //if (resultado > 0)
                //{
                //    var producto = await db.obtenerProducto(carrito.idProducto);
                //    if (producto != null)
                //    {
                //        producto.stock += carrito.cantidad;
                //        db.actualizarCantidadProducto(carrito.idProducto, producto.stock);
                //    }
                //}

                return RedirectToAction(nameof(VerCarrito));
            }
            catch (Exception ex)
            {
                // Manejo de errores
                ModelState.AddModelError("", "Ocurrió un error al eliminar el producto del carrito. Inténtelo nuevamente.");
                return RedirectToAction(nameof(VerCarrito));
            }
        }

    }
}
