using Catalogo_sis.Models;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using Catalogo_sis.Librerias;
using Catalogo_sis.Helpers;

namespace Catalogo_sis.Controllers
{
    public class UsuarioController(UsuarioModel usuarioModel) : Controller
    {

        private readonly UsuarioModel _usuarioModel = usuarioModel;
        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Recuperar()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        public IActionResult Solicitudes()
        {
            // Verificamos si hay alguna alerta pendiente (ej. cuando eliminemos una solicitud)
            if (TempData.Get<Alerta>("Alerta") != null)
            {
                ViewBag.msg = TempData.Get<Alerta>("Alerta");
            }

            // Traemos los datos desde el modelo
            var solicitudes = _usuarioModel.ListarSolicitudes();

            // Se los pasamos a la vista
            return View(solicitudes);
        }

        public IActionResult Usuarios()
        {
            if (TempData.Get<Alerta>("Alerta") != null)
            {
                ViewBag.msg = TempData.Get<Alerta>("Alerta");
            }
            var usuarios = _usuarioModel.ListarUsuarios();
            return View(usuarios);
        }

        [HttpPost]
        public JsonResult ValidarUsuario([Required] string usuario, int id)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false });
            }

            var resp = _usuarioModel.ValidarUsuario(usuario, id);

            if (resp == true)
            {
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        [HttpPost]
        public JsonResult ValidarCorreo([Required][EmailAddress] string correo, int id)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false });
            }

            var resp = _usuarioModel.ValidarCorreo(correo, id);

            if (resp == true)
            {
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult IniciarSesion(string usuario, string contrasenha)
        {

            if (string.IsNullOrEmpty(usuario) || string.IsNullOrEmpty(contrasenha))
            {
                return Json(new { success = false, message = "Por favor ingrese sus credenciales" });
            }

            var resp = _usuarioModel.Login(usuario, contrasenha);

            if (resp != null) //Se hace un if nuevo para validar el estado del usuario, esto para evitar el paso si el usuario está vetado de la página
            {
                int estadoUsuario = Convert.ToInt32(resp["estado"]);

                if (estadoUsuario == 2)
                {
                    return Json(new { success = false, message = "Tu cuenta ha sido suspendida o vetada." });
                }
                else if (estadoUsuario == 1)
                {
                    return Json(new { success = true, message = "Inicio de sesión exitoso" });
                }
                else
                {
                    return Json(new { success = false, message = "Estado de cuenta desconocido." });
                }
            }
            else
            {
                return Json(new { success = false, message = "Credenciales incorrectas" });
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult RecuperarContrasenha([Required][EmailAddress] string correo)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, msg = "Ingrese un correo válido" });
            }

            var resp_ = _usuarioModel.ValidarCorreo(correo, 0);

            if (resp_ == true)
            {
                var contrasenha = Guid.NewGuid().ToString("N")[..8];
                var resp = _usuarioModel.ActualizarContrasenha(correo, contrasenha);

                if (resp == true)
                {
                    var asunto = "Recuperación de contraseña - Aurora's Gallery";
                    var cuerpo = "<p> Se te ha asignado una nueva contraseña </p>" +
                        "<p> Contraseña nueva:<strong>" + contrasenha + "</strong> </p>";

                    if (Data.EnviarCorreo(correo, asunto, cuerpo))
                    {
                        return Json(new { success = true, msg = "Se ha enviado un correo electrónico con la contraseña nueva." });
                    }
                    return Json(new { success = false, msg = "Error al enviar correo." });
                }
                return Json(new { success = false, msg = "Error al recuperar la contraseña, vuelva a intentarlo" });
            }
            return Json(new { success = false, msg = "El correo ingresado no está registrado en el sistema." });
        }

        [HttpPost]
        public JsonResult Registrarse(
            [Required] string usuario,
            [Required] string contrasenha,
            [Required] string nombre_completo,
            [Required][EmailAddress] string correo)
        {
            //Se valida que los campos no vayan nulos
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, msg = "Por favor, complete todos los campos obligatorios." });
            }

            //Se intenta hacer el registro en la base de datos
            bool registrado = _usuarioModel.Register(usuario, nombre_completo, correo, contrasenha);

            if (registrado)
            {
                return Json(new { success = true, message = "Registro exitoso. ¡Bienvenido al lienzo!" });
            }
            else
            {
                return Json(new { success = false, message = "El nombre de usuario o correo ya se encuentran en uso." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult AgregarUsuario(
            [Required] string usuario,
            [Required] string contrasenha,
            [Required] string nombre_corto,
            [Required] string nombre_completo,
            [Required][EmailAddress] string correo,
            [Required] int id_rol
            )
        {

            if (!ModelState.IsValid)
            {
                return Json(new { success = false, msg = "Por favor, complete todos los campos obligatorios." });
            }

            //Pasa los datos al modelo
            var resultado = _usuarioModel.Agregar(usuario, contrasenha, nombre_corto, nombre_completo, correo, id_rol);

            if (resultado != null)
            {
                return Json(new { success = true, cod = resultado.Value._cod, msg = resultado.Value._msg });
            }
            return Json(new { success = false, msg = "Error al agregar el usuario" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult ModificarUsuario(
            [Required] int id_usuario,
            [Required] string usuario,
            [Required] string nombre_corto,
            [Required] string nombre_completo,
            [Required][EmailAddress] string correo,
            [Required] int rol,
            [Required] int estado
            )
        {

            if (!ModelState.IsValid)
            {
                return Json(new { success = false, msg = "Por favor, complete todos los campos obligatorios." });
            }

            if (id_usuario == 1001)
            {
                return Json(new { success = false, msg = "El administrador no puede ser modificado." });
            }
            //Pasa los datos al modelo
            var resultado = _usuarioModel.Modificar(id_usuario, usuario, nombre_corto, nombre_completo, correo, rol, estado);

            if (resultado != null)
            {
                return Json(new { success = true, cod = resultado.Value._cod, msg = resultado.Value._msg });
            }
            return Json(new { success = false, msg = "Error al agregar el usuario" });
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult EnviarSolicitud(
            [Required] string nombres,
            [Required] string apellidos,
            [Required][EmailAddress] string correo
            )
        {

            if (!ModelState.IsValid)
            {
                return Json(new { success = false, msg = "Por favor, complete todos los campos obligatorios." });
            }

            var resultado = _usuarioModel.CrearSolicitud(nombres, apellidos, correo);

            if (resultado != null)
            {
                return Json(new { success = true, cod = resultado.Value._cod, msg = resultado.Value._msg });
            }
            return Json(new { success = false, msg = "Error al enviar la solicitud." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult AceptarSolicitud(int id_solicitud, string nombres, string apellidos, string correo)
        {
            try
            {

                string contrasenha = Guid.NewGuid().ToString("N")[..8];

                string primerNombre = nombres.Trim().Split(' ')[0];
                string primerApellido = apellidos.Trim().Split(' ')[0];

                string baseUsuario = $"{primerNombre.ToLower()}{primerApellido.ToLower()}";

                // Si la unión tiene más de 12 letras, la cortamos para dejar espacio a los 3 números
                if (baseUsuario.Length > 12)
                {
                    baseUsuario = baseUsuario.Substring(0, 12);
                }

                string usuario = $"{baseUsuario}{new Random().Next(100, 999)}";

                string nombre_corto = $"{primerNombre} {primerApellido[0]}.";
                string nombre_completo = $"{nombres} {apellidos}";
                int id_rol = 3;

                var resultado = _usuarioModel.Agregar(usuario, contrasenha, nombre_corto, nombre_completo, correo, id_rol);

                if (resultado != null && resultado.Value._cod == 1)
                {
                    var asunto = "¡Bienvenido a Aurora's Gallery! - Credenciales de acceso";
                    var cuerpo = $"<h3>¡Hola {primerNombre}!</h3>" +
                                 $"<p>Nos complace informarte que tu solicitud de ingreso ha sido aprobada. A partir de ahora formas parte de nuestro ecosistema de artistas.</p>" +
                                 $"<p>Aquí tienes tus credenciales autogeneradas para acceder al lienzo:</p>" +
                                 $"<div style='background-color:#f4f4f4; padding: 15px; border-radius: 5px; margin: 10px 0;'>" +
                                 $"  <p><strong>Usuario:</strong> {usuario}</p>" +
                                 $"  <p><strong>Contraseña temporal:</strong> {contrasenha}</p>" +
                                 $"</div>" +
                                 $"<p><em>Te recomendamos iniciar sesión y cambiar tu contraseña lo antes posible por motivos de seguridad.</em></p>";

                    Data.EnviarCorreo(correo, asunto, cuerpo);

                    _usuarioModel.EliminarSolicitud(id_solicitud);

                    return Json(new { success = true, msg = "Solicitud aprobada con éxito. Se ha enviado el correo con las credenciales." });
                }
                else if (resultado != null)
                {
                    return Json(new { success = false, msg = resultado.Value._msg });
                }

                return Json(new { success = false, msg = "Error al intentar registrar al usuario." });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error C# Aceptar Solicitud: " + ex.Message);
                return Json(new { success = false, msg = "Ocurrió un error inesperado al procesar la solicitud." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RechazarSolicitud(int? id_solicitud)
        {
            if (id_solicitud == null)
            {
                TempData.Set("Alerta", new Alerta(0, "Hubo un problema al identificar la solicitud."));
                return RedirectToAction("Solicitudes", "Usuario");
            }

            var resp = _usuarioModel.EliminarSolicitud(id_solicitud.Value);

            if (resp == true)
            {
                TempData.Set("Alerta", new Alerta(1, "La solicitud ha sido rechazada."));
                return RedirectToAction("Solicitudes", "Usuario");
            }

            TempData.Set("Alerta", new Alerta(99, "Hubo un problema al intentar eliminar la solicitud."));
            return RedirectToAction("Solicitudes", "Usuario");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ReasignarContrasenha(
            [Required] int id_usuario,
            [Required][EmailAddress] string correo,
            [Required] string contrasenha
            )
        {
            if (!ModelState.IsValid)
            {
                TempData.Set("Alerta", new Alerta(0, "Ingrese correctamente todos los datos"));
                return RedirectToAction("Usuarios", "Usuario");
            }

            if (id_usuario == 1001)
            {
                TempData.Set("Alerta", new Alerta(0, "El usuario Administrador no puede ser modificado."));
                return RedirectToAction("Usuarios", "Usuario");
            }

            var resp = _usuarioModel.Reasignar(id_usuario, contrasenha);

            if (resp)
            {
                var asunto = "Reasignación de contraseña - Aurora's Gallery";
                var cuerpo = "<p>Se te ha asignado una nueva contraseña.</p>" +
                             "<p>Contraseña nueva: <strong>" + contrasenha + "</strong></p>";

                if (Data.EnviarCorreo(correo, asunto, cuerpo))
                {
                    TempData.Set("Alerta", new Alerta(1, "Se ha asignado la nueva contraseña y ha sido enviada al correo del usuario"));
                }
                else
                {
                    TempData.Set("Alerta", new Alerta(0, "Error al enviar el correo con la contraseña"));
                }

                return RedirectToAction("Usuarios", "Usuario");
            }

            TempData.Set("Alerta", new Alerta(99, "Error al reasignar la contraseña."));
            return RedirectToAction("Usuarios", "Usuario");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EliminarUsuario(int? id_usuario)
        {
            if (id_usuario == null)
            {
                TempData.Set("Alerta", new Alerta (0, "Hubo un problema al recuperar el usuario."));
                return RedirectToAction("Usuarios", "Usuario");
            }

            if (id_usuario == 1001)
            {
                TempData.Set("Alerta", new Alerta(0, "El usuario Administrador no puede ser eliminado."));
                return RedirectToAction("Usuarios", "Usuario");
            }

            var resp = _usuarioModel.Eliminar(id_usuario.Value);

            if (resp == true)
            {
                TempData.Set("Alerta", new Alerta(1, "El usuario ha sido eliminado."));
                return RedirectToAction("Usuarios", "Usuario");
            }
            TempData.Set("Alerta", new Alerta(99, "Hubo un problema al recuperar el usuario"));
            return RedirectToAction("Usuarios", "Usuario");
        }

        [HttpPost]
        public JsonResult BuscarUsuario(int id)
        {
            var resp = _usuarioModel.Buscar(id);
            return Json(resp);
        }
    } // Fin
} // Fin x2
