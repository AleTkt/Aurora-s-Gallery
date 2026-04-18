using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using Catalogo_sis.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Catalogo_sis.Librerias
{
    public class Data(DataModel dataModel)
    {
        private static readonly MailMessage Correo = new();
        private static readonly SmtpClient Cliente = new();

        private readonly DataModel _dataModel = dataModel;

        public List <SelectListItem> LlenarSelect (string tabla, string order, string id_campo, string campo)
        {
            var datos = _dataModel.Llenar(tabla, order);

            return datos
                .Where(x => x.ContainsKey(id_campo) && x.ContainsKey(campo))
                .Select (x => new SelectListItem { 
                    Value = x[id_campo]?.ToString(),
                    Text = x[campo]?.ToString()
                }
                ).ToList ();
        }

        public static bool  EnviarCorreo(string correo, string asunto, string cuerpo)
        {
            try
            {
                using var mensaje = new MailMessage();
                mensaje.From = new MailAddress("ga234100212@uma.edu.sv", "Aurora's Gallery");
                mensaje.To.Add(new MailAddress(correo));
                mensaje.Subject = asunto;
                mensaje.Body = cuerpo;
                mensaje.IsBodyHtml = true;

                using var cliente = new SmtpClient("smtp.gmail.com", 587)
                {
                    Credentials = new NetworkCredential("ga234100212@uma.edu.sv", "jdpq cpup uucf rsyz"),
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    TargetName = "smtp.gmail.com"
                };

                cliente.Send(mensaje);
                return true;
            }
            catch (Exception)
            {
                return false;
            }


        }

        public static string CifrarSHA256(string texto)
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(texto);
            byte[] hashBytes = SHA256.HashData(inputBytes);

            StringBuilder sb = new();
            foreach (byte b in hashBytes)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }



    }
}
