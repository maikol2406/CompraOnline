using CompraOnline.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using CompraOnline.Models.Usuarios;

namespace CompraOnline.Controllers
{
    public class UsuariosController : Controller
    {
        BaseDatos _baseDatos = new BaseDatos();

        // GET: UsuariosController/Create
        public ActionResult Login()
        {
            return View();
        }

        // POST: UsuariosController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(Usuario login)
        {
            try
            {
                foreach (var key in ModelState.Keys.Where(k => k != "username" && k != "password").ToList())
                {
                    ModelState.Remove(key);
                }

                if (!ModelState.IsValid)
                {
                    ModelState.AddModelError(string.Empty, $"El usuario o la contraseña ingresada no son validos.");
                    TempData["ErrorMessage"] = "Los datos ingresados no son válidos.";
                    return View(login);
                }

                bool isValidUser = await _baseDatos.ValidarUsuario(new Usuario
                {
                    username = login.username,
                    password = login.password
                });

                if (!isValidUser)
                {
                    ModelState.AddModelError(string.Empty, "Usuario o contraseña incorrectos.");
                    TempData["ErrorMessage"] = "El usuario o la contraseña son incorrectos.";
                    return View(login);
                }

                Usuario usuario = await _baseDatos.ObtenerUsuario(login);

                try
                {
                    if (usuario.username != null)
                    {
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, usuario.nombreCompleto),
                            new Claim("username", usuario.username.ToString()),
                            new Claim("idUsuario", usuario.idUsuario.ToString())
                        };
                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);
                    }
                }
                catch (Exception e)
                {

                    throw new Exception("Error " + e.Message);
                }

                return RedirectToAction("Index", "Home");

            }
            catch
            {
                return View(login);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            try
            {
                //await HttpContext.SignOutAsync();
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return RedirectToAction("Login", "Usuarios");
            }
            catch (Exception e)
            {

                throw new Exception("Error " + e.Message);
            }
        }

        // GET: UsuariosController/Create
        public ActionResult Registro()
        {
            ViewBag.pass = true;
            return View();
        }

        // POST: UsuariosController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Registro(Usuario usuario)
        {
            try
            {
                await Task.Run(() => _baseDatos.agregarUsuario(usuario));
                return RedirectToAction(nameof(VerUsuarios));
            }
            catch
            {
                return View();
            }
        }

        public async Task<IActionResult> VerUsuarios()
        {
            List<Usuario> usuarios = await _baseDatos.obtenerUsuarios();
            return View(usuarios);
        }

        // GET: UsuariosController/Create
        public async Task<ActionResult> VerUsuario(int idusuario)
        {
            Usuario usuario = await _baseDatos.obtenerUsuario(idusuario);
            return View(usuario);
        }

        [HttpGet]
        public ActionResult CambiarContrasena(string idUsuario)
        {
            CambioPassword cambioPassword = new CambioPassword();
            cambioPassword.idUsuario = int.Parse(idUsuario);
            return (View(cambioPassword));
        }

        [HttpPost]
        public async Task<IActionResult> CambiarContrasena(CambioPassword cambio)
        {
            bool resultado = await _baseDatos.CambioContrasena(cambio.idUsuario, cambio.ContrasenaActual, cambio.NuevaContrasena);

            if (!resultado)
            {
                ViewData["ErrorMessage"] = "La contrasena actual es incorrecta.";
                return View(cambio);
            }
            Logout();
            return RedirectToAction(nameof(Login));
        }

        // GET: UsuariosController/Create
        public async Task<ActionResult> EditarUsuario(int idUsuario)
        {
            Usuario usaurio = await _baseDatos.obtenerUsuario(idUsuario);
            return View(usaurio);
        }

        // POST: UsuariosController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditarUsuario(Usuario usuario)
        {
            try
            {
                await Task.Run(() => _baseDatos.actualizarUsuario(usuario));
                return RedirectToAction(nameof(VerUsuarios));
            }
            catch
            {
                return View();
            }
        }

        // GET: UsuariosController/Create
        public async Task<ActionResult> EliminarUsuario(int idUsuario)
        {
            Usuario usuario = await _baseDatos.obtenerUsuario(idUsuario);
            try
            {
                _baseDatos.eliminarUsuario(usuario);
                TempData["Success"] = "Usuario eliminado exitosamente.";
                return RedirectToAction("VerUsuarios", "Usuarios");

            }
            catch (Exception e)
            {
                TempData["Error"] = "Error al eliminar el usuario. Por favor, inténtelo de nuevo.";
                return RedirectToAction("VerUsuarios", "Usuarios");
            }
        }
    }
}
