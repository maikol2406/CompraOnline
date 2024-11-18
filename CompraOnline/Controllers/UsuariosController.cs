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

                if (string.IsNullOrWhiteSpace(login.username))
                {
                    ModelState.AddModelError("username", "El campo 'username' es obligatorio.");
                }

                if (string.IsNullOrWhiteSpace(login.password))
                {
                    ModelState.AddModelError("password", "El campo 'password' es obligatorio.");
                }

                //if (!ModelState.IsValid)
                //{
                //    return View(login);
                //}

                bool isValidUser = await _baseDatos.ValidarUsuario(new Usuario
                {
                    username = login.username,
                    password = login.password
                });

                if (!isValidUser)
                {
                    ModelState.AddModelError(string.Empty, "Cédula o contraseña incorrectos.");
                    return View(login);
                }

                Usuario usuario = await _baseDatos.ObtenerUsuario(login);

                try
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
                catch (Exception e)
                {

                    throw new Exception("Error " + e.Message);
                }

               

                return RedirectToAction("Index", "Home");

            }
            catch
            {
                return View();
            }
        }

        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.SignOutAsync();
            return RedirectToAction("Login", "Usuarios");
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

            return RedirectToAction(nameof(Login));
        }
    }
}
