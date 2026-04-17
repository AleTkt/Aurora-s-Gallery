using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using Catalogo_sis.Librerias;

namespace Catalogo_sis.Models
{
    public class DataModel(IConfiguration configuracion) : Conexion(configuracion)
    {
        public List<Dictionary<string, object>>? Llenar(string tabla, string order)
        {
            var datos = new List<Dictionary<string, object>>();

            using var con = new SqlConnection(cadena_conexion);
            con.Open();

            string query = $"SELECT * FROM {tabla} ORDER BY {order} ASC";

            using var cmd = new SqlCommand(query, con);
            using var data = cmd.ExecuteReader();

            while (data.Read())
            {
                var registro = new Dictionary<string, object>();

                for (int i = 0; i < data.FieldCount; i++) {

                    string campo = data.GetName(i);
                    object valor = data.GetValue(i);
                    registro[campo] = valor;
                }
                datos.Add(registro);
            }
            return datos;
        }
    }
}