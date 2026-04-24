using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using Catalogo_sis.Librerias;

namespace Catalogo_sis.Models
{
    public class UsuarioModel(IConfiguration configuracion) : Conexion(configuracion)
    {
        public Dictionary<string, object>? Login(string usuario, string contrasenha) {

            using var con = new SqlConnection(cadena_conexion);
            con.Open();

            string query = "SELECT id_usuario, nombre_corto, id_rol, id_estado FROM usuarios WHERE usuario = @usuario AND contrasenha = @contrasenha";

            using var cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@usuario", usuario);
            cmd.Parameters.AddWithValue("@contrasenha", Data.CifrarSHA256(contrasenha));

            using var data = cmd.ExecuteReader();

            if (data.Read()) {

                return new Dictionary<string, object> {
                    ["id_usuario"] = data.GetInt32("id_usuario"),
                    ["usuario"] = data.GetString("nombre_corto"),
                    ["rol"] = data.GetInt32("id_rol"),
                    ["estado"] = data.GetInt32(data.GetOrdinal("id_estado"))
                };
            }
            return null;
        }

        public (int _cod, string _msg)? Agregar(string usuario, string contrasenha, string nombre_corto, string nombre_completo, string correo, int id_rol)
        {
            using var con = new SqlConnection(cadena_conexion);
            con.Open();

            using var cmd = new SqlCommand("sp_insertar_usuario", con);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@usuario", usuario);
            cmd.Parameters.AddWithValue("@contrasenha", Data.CifrarSHA256(contrasenha)); // Ciframos la contraseña
            cmd.Parameters.AddWithValue("@nombre_corto", nombre_corto);
            cmd.Parameters.AddWithValue("@nombre_completo", nombre_completo);
            cmd.Parameters.AddWithValue("@correo", correo);
            cmd.Parameters.AddWithValue("@id_rol", id_rol);

            using var data = cmd.ExecuteReader();

            if (data.Read())
            {
                Console.WriteLine(data.GetString("mensaje"));
                return (data.GetInt32("cod"), data.GetString("msg"));
            }
            return null;

        }

        public (int _cod, string _msg)? Modificar(int id_usuario, string usuario, string nombre_corto, string nombre_completo, string correo, int id_rol, int id_estado)
        {
            using var con = new SqlConnection(cadena_conexion);
            con.Open();

            using var cmd = new SqlCommand("sp_modificar_usuario", con);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@p_id_usuario", id_usuario);
            cmd.Parameters.AddWithValue("@p_usuario", usuario);
            cmd.Parameters.AddWithValue("@p_nombre_corto", nombre_corto);
            cmd.Parameters.AddWithValue("@p_nombre_completo", nombre_completo);
            cmd.Parameters.AddWithValue("@p_correo", correo);
            cmd.Parameters.AddWithValue("@p_id_rol", id_rol);
            cmd.Parameters.AddWithValue("@p_id_estado", id_estado);

            using var data = cmd.ExecuteReader();

            if (data.Read())
            {
                Console.WriteLine(data.GetString("mensaje"));
                return (data.GetInt32("cod"), data.GetString("msg"));
            }
            return null;

        }

        public (int _cod, string _msg)? CrearSolicitud(string nombres, string apellidos, string correo)
        {
            using var con = new SqlConnection(cadena_conexion);
            con.Open();

            using var cmd = new SqlCommand("sp_insertar_solicitud", con);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@nombres", nombres);
            cmd.Parameters.AddWithValue("@apellidos", apellidos);
            cmd.Parameters.AddWithValue("@correo", correo);

            using var data = cmd.ExecuteReader();

            if (data.Read())
            {
                Console.WriteLine(data.GetString("mensaje"));
                return (data.GetInt32("cod"), data.GetString("msg"));
            }
            return null;
        }

        public bool Register(string usuario, string nombre_completo, string correo, string contrasenha)
        {
            using var con = new SqlConnection(cadena_conexion);
            con.Open();

            //Se hace una verificación en la base de datos para saber si el usuario que se está registrando existe o no
            string usuarioExistente = "SELECT COUNT(*) FROM usuarios WHERE usuario = @usuario OR correo = @correo";
            using var checkCmd = new SqlCommand(usuarioExistente, con);
            checkCmd.Parameters.AddWithValue("@usuario", usuario);
            checkCmd.Parameters.AddWithValue("@correo", correo);

            int existe = (int)checkCmd.ExecuteScalar(); //Es un método que sirve para ejecutar una consulta SQL y devolver un único valor
            if (existe > 0)
            {
                return false; //Si regresa un falso, entonces el usuario existe en la base de datos
            }

            string insertQuery = @"
                INSERT INTO usuarios 
                (usuario, contrasenha, nombre_corto, nombre_completo, correo, fecha_registro, id_rol, id_estado) 
                VALUES 
                (@usuario, @contrasenha, @nombre_corto, @nombre_completo, @correo, GETDATE(), 3, 1)";

            using var cmd = new SqlCommand(insertQuery, con);
            cmd.Parameters.AddWithValue("@usuario", usuario);
            cmd.Parameters.AddWithValue("@contrasenha", Data.CifrarSHA256(contrasenha));
            cmd.Parameters.AddWithValue("@nombre_corto", usuario); //Provisional, en el formulario Register no hay espacio para nombre corto, por lo que se usará el nombre de usuario que se ingrese
            cmd.Parameters.AddWithValue("@nombre_completo", nombre_completo);
            cmd.Parameters.AddWithValue("@correo", correo);

            int filasAfectadas = cmd.ExecuteNonQuery();

            return filasAfectadas > 0;
        }

        public bool ValidarCorreo(string correo, int id)
        {
            using var con = new SqlConnection(cadena_conexion);
            con.Open();

            string query = "SELECT correo FROM usuarios WHERE correo = @correo";

            if (id != 0)
            {
                query += " AND id_usuario != @id";
            }

            using var cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@correo", correo);
            if (id != 0)
            {
                cmd.Parameters.AddWithValue("@id", id);
            }
            using var data = cmd.ExecuteReader();

            if (data.Read())
            {
                return true;
            }
            return false;
        }

        public bool ValidarUsuario(string usuario, int id)
        {
            using var con = new SqlConnection(cadena_conexion);
            con.Open();

            string query = "SELECT usuario FROM usuarios WHERE usuario = @usuario";

            if (id != 0)
            {
                query += " AND id_usuario != @id";
            }

            using var cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@usuario", usuario);
            if (id != 0)
            {
                cmd.Parameters.AddWithValue("@id", id);
            }
            using var data = cmd.ExecuteReader();

            if (data.Read())
            {
                return true;
            }
            return false;
        }

        public bool ValidarContrasenha(int id, string contrasenha)
        {
            using var con = new SqlConnection(cadena_conexion);
            con.Open();

            string query = "SELECT contrasenha FROM usuarios WHERE contrasenha = @contrasenha";

            if (id != 0)
            {
                query += " AND id_usuario = @id";
            }

            using var cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@contrasenha", Data.CifrarSHA256(contrasenha));
            if (id != 0)
            {
                cmd.Parameters.AddWithValue("@id", id);
            }
            using var data = cmd.ExecuteReader();

            if (data.Read())
            {
                return true;
            }
            return false;
        }

        public bool ModificarContrasenha(int id, string contrasenha)
        {
            using var con = new SqlConnection(cadena_conexion);
            con.Open();

            string query = "UPDATE usuarios SET contrasenha = @contrasenha WHERE id_usuario = @id";

            using var cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("contrasenha", Data.CifrarSHA256(contrasenha));
            cmd.Parameters.AddWithValue("id", id);
            var data = cmd.ExecuteNonQuery();

            if (data > 0)
            {
                return true;
            }
            return false;
        }

        public bool ActualizarContrasenha(string correo, string contrasenha)
        {
            using var con = new SqlConnection(cadena_conexion);
            con.Open();

            string query = "UPDATE usuarios SET contrasenha = @contrasenha WHERE correo = @correo";

            using var cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@correo", correo);
            cmd.Parameters.AddWithValue("@contrasenha", Data.CifrarSHA256(contrasenha));
            var data = cmd.ExecuteNonQuery();

            if (data > 0)
            {
                return true;
            }
            return false;
        }

        public List<Dictionary<string, object>>? ListarUsuarios()
        {
            var usuarios = new List<Dictionary<string, object>>();

            using var con = new SqlConnection(cadena_conexion);
            con.Open();

            string query = "SELECT * FROM usuarios_view";

            using var cmd = new SqlCommand(query, con);

            using var data = cmd.ExecuteReader();

            //Mientras se lee empieza a crear registros

            while (data.Read())
            {

                usuarios.Add(new Dictionary<string, object>
                {
                    ["id"] = data.GetInt32("id_usuario"),
                    ["usuario"] = data.GetString("usuario"),
                    ["nombre_corto"] = data.GetString("nombre_corto"),
                    ["nombre_completo"] = data.GetString("nombre_completo"),
                    ["correo"] = data.GetString("correo"),
                    ["fecha_registro"] = data.GetDateTime("fecha_registro"),
                    ["rol"] = data.GetString("rol"),
                    ["estado"] = data.GetString("estado")
                });
            }
            return usuarios;
        }

        public List<Dictionary<string, object>>? ListarSolicitudes()
        {
            var solicitudes = new List<Dictionary<string, object>>();

            using var con = new SqlConnection(cadena_conexion);
            con.Open();

            // Ordenamos por fecha descendente para ver las nuevas solicitudes primero
            string query = "SELECT id_solicitud, nombres, apellidos, correo, fecha_solicitud, estado FROM solicitudes ORDER BY fecha_solicitud DESC";

            using var cmd = new SqlCommand(query, con);
            using var data = cmd.ExecuteReader();

            while (data.Read())
            {
                solicitudes.Add(new Dictionary<string, object>
                {
                    ["id_solicitud"] = Convert.ToInt32(data["id_solicitud"]),
                    ["nombres"] = data["nombres"].ToString() ?? "",
                    ["apellidos"] = data["apellidos"].ToString() ?? "",
                    ["correo"] = data["correo"].ToString() ?? "",
                    ["fecha_solicitud"] = Convert.ToDateTime(data["fecha_solicitud"]),
                    ["estado"] = data["estado"].ToString() ?? ""
                });
            }
            return solicitudes;
        }

        public bool Reasignar(int id, string contrasenha)
        {
            using var con = new SqlConnection(cadena_conexion);
            con.Open();

            string query = "UPDATE usuarios SET contrasenha = @contrasenha WHERE id_usuario = @id";

            using var cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@contrasenha", Data.CifrarSHA256(contrasenha));

            if (cmd.ExecuteNonQuery() > 0)
            {
                return true;
            }
            return false;
        }

        public bool Eliminar(int id)
        {
            using var con = new SqlConnection(cadena_conexion);
            con.Open();

            string query = "UPDATE usuarios SET id_estado = 99 WHERE id_usuario = @id";

            using var cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@id", id);

            if (cmd.ExecuteNonQuery() > 0)
            {
                return true;
            }
            return false;
        }

        public bool EliminarSolicitud(int id)
        {
            using var con = new SqlConnection(cadena_conexion);
            con.Open();

            // Borrado físico de la base de datos
            string query = "DELETE FROM solicitudes WHERE id_solicitud = @id";

            using var cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@id", id);

            if (cmd.ExecuteNonQuery() > 0)
            {
                return true;
            }
            return false;
        }

        public Dictionary<string, object>? Buscar(int id)
        {
            using var con = new SqlConnection(cadena_conexion);
            con.Open();

            string query = "SELECT * FROM usuarios_view WHERE id_usuario = @id";

            using var cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@id", id);

            using var data = cmd.ExecuteReader();

            if (data.Read())
            {
                return new Dictionary<string, object>
                {
                    ["id"] = data.GetInt32("id_usuario"),
                    ["usuario"] = data.GetString("usuario"),
                    ["nombre_corto"] = data.GetString("nombre_corto"),
                    ["nombre_completo"] = data.GetString("nombre_completo"),
                    ["correo"] = data.GetString("correo"),
                    ["id_rol"] = data.GetInt32("id_rol"),
                    ["id_estado"] = data.GetInt32("id_estado")
                };
            }
            return null;
        }
    }
}
