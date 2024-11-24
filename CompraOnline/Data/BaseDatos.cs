using CompraOnline.Models.Pedidos;
using CompraOnline.Models.Productos;
using CompraOnline.Models.Usuarios;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Security.Cryptography;
using System.Text;

namespace CompraOnline.Data
{
    public class BaseDatos
    {
        public SqlConnectionStringBuilder conexion()
        {
            Param prm = new Param();
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            builder.DataSource = prm.parametrosDB().DataSource;
            builder.UserID = prm.parametrosDB().UserID;
            builder.Password = prm.parametrosDB().Password;
            builder.InitialCatalog = prm.parametrosDB().InitialCatalog;
            builder.TrustServerCertificate = true;
            return builder;
        }

        public static string EncriptaContrasena(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(bytes).Replace("-", "").ToLower();
            }
        }

        public virtual async Task<List<Usuario>> obtenerUsuarios()
        {
            var listaUsuarios = new List<Usuario>();

            SqlConnectionStringBuilder builder = conexion();
            using (SqlConnection conn = new SqlConnection(builder.ConnectionString))
            {
                await conn.OpenAsync();
                var query = "SELECT * FROM usuarios";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (await reader.ReadAsync())
                        {
                            var usuario = new Usuario
                            {
                                idUsuario = reader.GetInt32("idUsuario"),
                                username = reader.GetString("username"),
                                password = reader.GetString("password"),
                                nombreCompleto = reader.GetString("nombreCompleto")
                            };
                            listaUsuarios.Add(usuario);
                        }
                    }
                }
            }
            return listaUsuarios;
        }

        public virtual async void agregarUsuario(Usuario usuario)
        {
            try
            {
                SqlConnectionStringBuilder builder = conexion();
                using (SqlConnection conn = new SqlConnection(builder.ConnectionString))
                {
                    await conn.OpenAsync();
                    var query = "INSERT INTO Usuarios (username, password, nombreCompleto) VALUES (@username, @password, @nombreCompleto)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        //cmd.Parameters.AddWithValue("@idUsuario", usuario.idUsuario);
                        cmd.Parameters.AddWithValue("@username", usuario.username);
                        cmd.Parameters.AddWithValue("@password", EncriptaContrasena(usuario.password));
                        cmd.Parameters.AddWithValue("@nombreCompleto", usuario.nombreCompleto);

                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception e)
            {

                throw new Exception("Error al guardar los datos del usuario. " + e.Message);
            }
        }

        public virtual async Task<bool> CambioContrasena(int idUsuario, string passwordActual, string passwordNuevo)
        {
            SqlConnectionStringBuilder builder = conexion();
            using (SqlConnection conn = new SqlConnection(builder.ConnectionString))
            {
                await conn.OpenAsync();

                string passIngresado = EncriptaContrasena(passwordActual);
                string passNuevoEncriptado = EncriptaContrasena(passwordNuevo);

                string queryConsulta = "SELECT password FROM usuarios WHERE idUsuario = @idUsuario";
                using (SqlCommand cmd = new SqlCommand(queryConsulta, conn))
                {
                    cmd.Parameters.AddWithValue("@idUsuario", idUsuario);
                    
                    string passAlmacenado = (string)await cmd.ExecuteScalarAsync();

                    if (!passAlmacenado.Equals(passIngresado))
                    {
                        return false;
                    }
                }

                var queryInsertar = "UPDATE usuarios SET password = @nuevaContrasena WHERE idUsuario = @idUsuario";
                using (SqlCommand cmdInsert = new SqlCommand(queryInsertar,conn))
                {
                    cmdInsert.Parameters.AddWithValue("@idUsuario", idUsuario);
                    cmdInsert.Parameters.AddWithValue("@nuevaContrasena", EncriptaContrasena(passwordNuevo));
                    await cmdInsert.ExecuteNonQueryAsync();
                }
            }

            return true;
        }

        public async Task<bool> ValidarUsuario(Usuario login)
        {
            SqlConnectionStringBuilder builder = conexion();
            using (SqlConnection conn = new SqlConnection(builder.ConnectionString))
            {
                await conn.OpenAsync();

                var passwordEncriptada = EncriptaContrasena(login.password);

                var query = "SELECT COUNT(1) FROM dbo.Usuarios WHERE username = @username AND password = @password";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.Add(new SqlParameter("@username", SqlDbType.NVarChar) { Value = login.username });
                    cmd.Parameters.Add(new SqlParameter("@password", SqlDbType.NVarChar) { Value = passwordEncriptada });

                    try
                    {
                        var resultado = await cmd.ExecuteScalarAsync();
                        return Convert.ToInt32(resultado) > 0;
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Error " + e.Message);
                    }
                    return false;
                }
            }
        }

        public async Task<Usuario> ObtenerUsuario(Usuario login)
        {
            SqlConnectionStringBuilder builder = conexion();
            using (SqlConnection conn = new SqlConnection(builder.ConnectionString))
            {
                conn.Open();

                var passwordEncriptada = EncriptaContrasena(login.password);

                var query = "SELECT idUsuario, username, password, nombreCompleto FROM usuarios WHERE username = @username AND password = @password";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@username", login.username);
                    cmd.Parameters.AddWithValue("@password", passwordEncriptada);

                    try
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                Usuario usuario = new Usuario
                                {
                                    idUsuario = reader.GetInt32("idUsuario"),
                                    username = reader.GetString("username"),
                                    password = reader.GetString("password"),
                                    nombreCompleto = reader.GetString("nombreCompleto")
                                };
                                return usuario;
                            }
                        }
                    }
                    catch (Exception e)
                    {

                        throw new Exception("Error " + e.Message);
                    }
                }
            }
            return null;
        }

        public async Task<List<Pedido>> obtenerPedidos(int idUsuario)
        {
            SqlConnectionStringBuilder builder = conexion();
            List<Pedido> listaPedidos = new List<Pedido>();
            using (SqlConnection conn = new SqlConnection(builder.ConnectionString))
            {
                string query = "SELECT * FROM Pedidos WHERE idUsuario = @IDUSUARIO";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IDUSUARIO", idUsuario);
                    await conn.OpenAsync();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            Pedido pedido = new Pedido();
                            pedido.idPedido = Convert.ToInt32(reader["idPedido"].ToString());
                            pedido.idUsuario = Convert.ToInt32(reader["idUsuario"].ToString());
                            pedido.cantidad = Convert.ToInt32(reader["cantidad"].ToString());
                            pedido.precioTotal = float.Parse(reader["precioTotal"].ToString());
                            pedido.estadoPedido = Convert.ToBoolean(reader["estadoPedido"].ToString());

                            listaPedidos.Add(pedido);
                        }
                    }
                    await conn.CloseAsync();
                }
            }
            return listaPedidos;
        }

        public async Task<List<Producto>> obtenerProductos()
        {
            SqlConnectionStringBuilder builder = conexion();
            List<Producto> listaProductos = new List<Producto>();
            using (SqlConnection conn = new SqlConnection(builder.ConnectionString))
            {
                string query = "SELECT * FROM Productos";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    await conn.OpenAsync();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Producto producto = new Producto();
                            producto.idProducto = Convert.ToInt32(reader["idProducto"].ToString());
                            producto.nombreProducto = Convert.ToString(reader["nombreProducto"].ToString());
                            producto.descripcionProducto = Convert.ToString(reader["descripcionProducto"].ToString());
                            producto.precio = float.Parse(reader["precio"].ToString());
                            producto.precioPromo = float.Parse(reader["precioPromo"].ToString());
                            producto.stock = Convert.ToInt32(reader["stock"].ToString());
                            producto.idCategoria = Convert.ToInt32(reader["idCategoria"].ToString());
                            producto.promocion = Convert.ToBoolean(reader["promocion"].ToString());

                            listaProductos.Add(producto);
                        }
                    }
                    await conn.CloseAsync();
                }
            }
            return listaProductos;
        }

        public async Task<List<Categoria>> obtenerCategorias()
        {
            SqlConnectionStringBuilder builder = conexion();
            List<Categoria> listaCategorias = new List<Categoria>();
            using (SqlConnection conn = new SqlConnection(builder.ConnectionString))
            {
                string query = "SELECT * FROM Categorias";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    await conn.OpenAsync();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Categoria categoria = new Categoria();
                            categoria.idCategoria = Convert.ToInt32(reader["idCategoria"].ToString());
                            categoria.nombreCategoria = Convert.ToString(reader["nombreCategoria"].ToString());
                            

                            listaCategorias.Add(categoria);
                        }
                    }
                    await conn.CloseAsync();
                }
            }
            return listaCategorias;
        }

        public void insertarProducto(Producto producto)
        {
            SqlConnectionStringBuilder builder = conexion();
            List<Categoria> listaCategorias = new List<Categoria>();
            using (SqlConnection conn = new SqlConnection(builder.ConnectionString))
            {
                string query = "INSERTAR_ARTICULO";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    //cmd.Parameters.AddWithValue("@IDPRODUCTO", producto.idProducto);
                    cmd.Parameters.AddWithValue("@NOMBREPRODUCTO", producto.nombreProducto);
                    cmd.Parameters.AddWithValue("@DESCRIPCION", producto.descripcionProducto);
                    cmd.Parameters.AddWithValue("@PRECIO", producto.precio);
                    cmd.Parameters.AddWithValue("@PRECIOPROMO", producto.precioPromo);
                    cmd.Parameters.AddWithValue("@STOCK", producto.stock);
                    cmd.Parameters.AddWithValue("@IDCATEGORIA", producto.idCategoria);
                    cmd.Parameters.AddWithValue("@PROMOCION", producto.promocion);

                    conn.OpenAsync();
                    try
                    {
                        cmd.ExecuteNonQuery();
                        conn.Close();
                    }
                    catch (SqlException sqlEx)
                    {
                        conn.Close();
                        throw new Exception("Error al registrar los datos en la tabla Productos. " + sqlEx.Message);
                    }
                    catch (Exception otherEx)
                    {
                        conn.Close();
                        throw new Exception("Se produjo un error al ejecutar el método INSERTAR_ARTICULO." + otherEx.Message);
                    }
                    conn.CloseAsync();
                }
            }
        }

    }
}
