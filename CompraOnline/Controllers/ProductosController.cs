﻿using CompraOnline.Data;
using CompraOnline.Models.CarritoCompras;
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

        [HttpGet]
        public async Task<ActionResult> BuscarProducto()
        {
            List<Producto> listaProductos = new List<Producto>();
            listaProductos = await db.obtenerNombresProductos();
            ViewBag.nombreProductos = listaProductos.Select(o => new SelectListItem { Value = o.nombreProducto, Text = o.nombreProducto }).ToList();
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> ObtenerProductosFiltro(string nombreProducto)
        {
            var productos = await db.obtenerProductosFiltrados(nombreProducto);
            return PartialView("_ProductosFiltrados", productos);
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
                if (producto.precio != producto.precioPromo)
                {
                    producto.promocion = true;
                }
                else
                {
                    producto.promocion = false;
                }
                db.insertarProducto(producto);
                return RedirectToAction(nameof(ProductosXCategoria));
            }
            catch
            {
                return View(producto);
            }
        }

        // GET: ProductosController/Edit/5
        public async Task<ActionResult> EditarProducto(int idProducto)
        {
            Producto producto = await db.obtenerProducto(idProducto);
            ViewBag.Categorias = new SelectList(await db.obtenerCategorias(), "idCategoria", "nombreCategoria");
            return View(producto);
        }

        // POST: ProductosController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditarProducto(Producto producto)
        {
            try
            {
                if (producto.precio != producto.precioPromo)
                {
                    producto.promocion = true;
                }
                else
                {
                    producto.promocion = false;
                }
                db.actualizarProducto(producto);
                return RedirectToAction(nameof(ProductosXCategoria));
            }
            catch
            {
                ViewBag.Categorias = new SelectList(await db.obtenerCategorias(), "idCategoria", "nombreCategoria");
                return View(producto);
            }
        }

        // GET: CarritoComprasController/Detail/5
        public async Task<ActionResult> DetallesProducto(int idProducto)
        {
            try
            {
                Producto producto = new Producto();
                producto = await db.obtenerProducto(idProducto);
                if (producto == null)
                {
                    return NotFound();
                }
                return View(producto);
            }
            catch
            {
                return RedirectToAction(nameof(ProductosXCategoria));
            }
        }

    }
}
