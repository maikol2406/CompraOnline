using CompraOnline.Data;
using CompraOnline.Models.CarritoCompras;
using CompraOnline.Models.Pedidos;
using CompraOnline.Models.Productos;
using CompraOnline.Models.Usuarios;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NuGet.Common;
using System.Diagnostics;
using System.Text;
using Token = CompraOnline.Data.Token;

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
            List<Usuario> listaUsuarios = await db.obtenerUsuarios();
            int idUsuario = int.Parse(User.FindFirst("idUsuario")?.Value);
            listaPedidos = await db.obtenerPedidos(idUsuario);
            ViewBag.listaUsuarios = listaUsuarios;
            ViewBag.fecha = DateTime.Now.AddMinutes(-120);
            return View(listaPedidos);
        }

        // GET: PedidosController/Create
        public ActionResult CrearPedidos()
        {
            Pedido pedido = new Pedido();
            pedido.idUsuario = int.Parse(User.FindFirst("idUsuario")?.Value);
            pedido.precioTotal = 0;
            pedido.estadoPedido = false;
            return View(pedido);
        }

        // POST: PedidosController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CrearPedidos(Pedido pedido)
        {
            try
            {
                await db.insertarPedido(pedido.idUsuario, pedido.precioTotal, pedido.estadoPedido);
                return RedirectToAction(nameof(MisPedidos));
            }
            catch
            {
                return View();
            }
        }

        // GET: PedidosController/Create
        public async Task<ActionResult> PagarPedido(int idPedido)
        {
            var factura = "0070000701";
            int idCliente = int.Parse(User.FindFirst("idUsuario")?.Value);
            var pedido = await db.obtenerPedido(idCliente);

            foreach (var item in await db.obtenerCarritosCompras(pedido.idPedido))
            {
                pedido.precioTotal = pedido.precioTotal + item.montoTotal;
            }

            Pago pago = new Pago();
            
            List<Pago> listaPagos = await db.ObtenerPagos(idCliente);

            int nro = 0;
            if (listaPagos.Count() == 0)
            {
                nro++;
                pago.pk_tsal001 = factura + nro.ToString().PadLeft(10, '0');
            }
            else
            {
                string ultimoPago = listaPagos.LastOrDefault().pk_tsal001;
                pago.pk_tsal001 = factura + (long.Parse(ultimoPago.Substring(ultimoPago.Length - 10)) + 1).ToString().PadLeft(10, '0');
            }
            pago.idPedido = idPedido;
            pago.terminalId = "EMVSBAT1";
            pago.transactionType = "SALE";
            pago.invoice = pago.pk_tsal001.Substring(pago.pk_tsal001.Length - 10);
            pago.totalAmount = pedido.precioTotal.ToString();
            double costo = pedido.precioTotal;
            double impuesto = costo * 0.13;
            pago.taxAmount = impuesto.ToString();
            pago.tipAmount = "0";
            //pago.clientEmail = pedido;
            pago.idCliente = idCliente;

            return View(pago);
        }

        // POST: PedidosController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> PagarPedido(Pago pago)
        {
            try
            {
                //string URL_SB2B = "http://localhost:62278/api/";
                string URL_SB2B = "https://sb2b.superbaterias.com:44324/api/";
                string token = "";
                using (HttpClient client = new HttpClient())
                {
                    //string url = "http://localhost:62278/generaToken";
                    string url = "https://sb2b.superbaterias.com:44324/generaToken";
                    Token objetoToken = new Token();

                    string jsonToken = JsonConvert.SerializeObject(objetoToken);
                    StringContent contentToken = new StringContent(jsonToken, Encoding.UTF8, "application/json");
                    HttpResponseMessage responsePago = await client.PostAsync(url, contentToken);
                    if (responsePago.IsSuccessStatusCode)
                    {
                        string respuestatoken = await responsePago.Content.ReadAsStringAsync();
                        token = JsonConvert.DeserializeObject<string>(respuestatoken);
                    }
                }
                using (HttpClient cliente = new HttpClient())
                {
                    cliente.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                    string link = URL_SB2B + "/bac/open-script-page";

                    string jsonPago = JsonConvert.SerializeObject(pago);
                    StringContent contenido = new StringContent(jsonPago, Encoding.UTF8, "application/json");

                    HttpResponseMessage responsePago = await cliente.PostAsync(link, contenido);
                    if (responsePago.IsSuccessStatusCode)
                    {
                        string respuestaPago = await responsePago.Content.ReadAsStringAsync();
                        UrlPago respuestaLinkPago = JsonConvert.DeserializeObject<UrlPago>(respuestaPago);

                        string scriptPath = @"C:\EqualRP\IntegracionBAC\run-sdk.ps1";
                        string parametro = respuestaLinkPago.URL.ToString();

                        ProcessStartInfo psi = new ProcessStartInfo
                        {
                            FileName = "powershell.exe",
                            Arguments = $"-ExecutionPolicy Bypass -File \"{scriptPath}\" \"{parametro}\"",
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        };

                        using (Process process = new Process { StartInfo = psi })
                        {
                            process.Start();

                            // Captura la salida del proceso (opcional)
                            string output = process.StandardOutput.ReadToEnd();
                            string error = process.StandardError.ReadToEnd();

                            process.WaitForExit();

                            // Muestra la salida o error si lo necesitas
                            Console.WriteLine("Output:");
                            Console.WriteLine(output);
                            Console.WriteLine("Error:");
                            Console.WriteLine(error);
                        }

                        using (HttpClient clientePago = new HttpClient())
                        {
                            clientePago.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                            string linkPago = URL_SB2B + "ConsultaEstado";
                            ConsultaPago consultaPago = new ConsultaPago();
                            consultaPago.pk_tsal001 = pago.pk_tsal001;
                            string jsonConsultaPago = JsonConvert.SerializeObject(consultaPago);
                            StringContent contenidoPago = new StringContent(jsonConsultaPago, Encoding.UTF8, "application/json");
                            //HttpResponseMessage responseConsultaPago = await clientePago.PostAsync(linkPago, contenidoPago);
                            bool seguir = true;
                            while (seguir)
                            {
                                HttpResponseMessage responseConsultaPago = await clientePago.PostAsync(linkPago, contenidoPago);
                                //await Task.Delay(3000);
                                if (responseConsultaPago.IsSuccessStatusCode)
                                {
                                    string respuestaConsulta = await responseConsultaPago.Content.ReadAsStringAsync();
                                    ConsultaPago consulta = JsonConvert.DeserializeObject<ConsultaPago>(respuestaConsulta);
                                    if (consulta.codigo == 1)
                                    {
                                        await db.insertarPago(pago);
                                        Pedido pedido = await db.obtenerPedido(pago.idPedido);
                                        pedido.estadoPedido = true;
                                        seguir = false;
                                        await db.actualizarPedido(pago.idPedido);
                                        foreach (var item in await db.obtenerCantidades(pago.idPedido))
                                        {
                                            foreach (var item2 in await db.obtenerProductos())
                                            {
                                                if (item.idProducto == item2.idProducto)
                                                {
                                                    item2.stock = item2.stock - item.cantidad;
                                                    db.actualizarCantidadProducto(item.idProducto, item2.stock);
                                                }
                                            }
                                        }
                                        return RedirectToAction("MisPedidos", "Pedidos");
                                    }
                                }
                            }
                        }
                    }
                }
                return RedirectToAction("VerCarrito", "CarritoCompras");
            }
            catch(Exception e)
            {
                throw new Exception("Error " + e.Message);
                return RedirectToAction("PagarPedido", "Pedidos", new { idPedido = pago.idPedido });
            }
        }

        // GET: PedidosController/Details/5
        public async Task<ActionResult> DetallesPedido(int idPedido)
        {
            Pedido pedido = new Pedido();
            List<Usuario> listaUsuarios = await db.obtenerUsuarios();
            int idUsuario = int.Parse(User.FindFirst("idUsuario")?.Value);
            pedido = await db.obtenerPedidoXId(idPedido, idUsuario);
            ViewBag.listaUsuarios = listaUsuarios;
            List<CarritoCompra> listaCarrito = new List<CarritoCompra>();
            listaCarrito = await db.obtenerCarritosCompras(idPedido);
            ViewBag.listaCarrito = listaCarrito;
            List<Producto> listaProductos = new List<Producto>();
            listaProductos = await db.obtenerProductos();
            ViewBag.listaProductos = listaProductos;
            return View(pedido);
        }

        // GET: PedidosController/Delete/5
        public async Task<ActionResult> EliminarPedido(int idPedido)
        {
            Pedido pedido = new Pedido();
            int idUsuario = int.Parse(User.FindFirst("idUsuario")?.Value);
            pedido = await db.obtenerPedidoXId(idPedido, idUsuario);
            int i = await db.eliminarPedido(pedido);
            return RedirectToAction(nameof(MisPedidos));
        }
    }
}
