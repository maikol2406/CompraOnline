﻿using CompraOnline.Data;
using CompraOnline.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CompraOnline.Controllers
{
    public class ProductosController : Controller
    {
        // GET: ProductosController
        [HttpGet]
        [Route("/productosXCategoria")]
        public async Task<ActionResult> ProductosXCategoria()
        {
            BaseDatos db = new BaseDatos();
            List<Pedido> listaPedidos = new List<Pedido>();
            int idUsuario = int.Parse(User.FindFirst("idUsuario")?.Value);
            listaPedidos = db.obtenerPedidos(idUsuario);
            return View();
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