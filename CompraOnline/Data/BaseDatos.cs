using CompraOnline.Models.CarritoCompras;
using CompraOnline.Models.Pedidos;
using CompraOnline.Models.Productos;
using CompraOnline.Models.Usuarios;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Configuration;
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

        public async Task<List<Producto>> obtenerNombresProductos()
        {
            SqlConnectionStringBuilder builder = conexion();
            List<Producto> listaProductos = new List<Producto>();
            using (SqlConnection conn = new SqlConnection(builder.ConnectionString))
            {
                //string query = "SELECT DISTINCT(nombreProducto), idProducto FROM Productos";
                string query = "SELECT nombreProducto, MIN(idProducto) AS idProducto FROM Productos GROUP BY nombreProducto;";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    await conn.OpenAsync();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Producto producto = new Producto();
                            producto.nombreProducto = Convert.ToString(reader["nombreProducto"].ToString());
                            producto.idProducto = Convert.ToInt32(reader["idProducto"]);

                            listaProductos.Add(producto);
                        }
                    }
                    await conn.CloseAsync();
                }
            }
            return listaProductos;
        }

        public async Task<List<Producto>> obtenerProductosFiltrados(string nombreProducto)
        {
            SqlConnectionStringBuilder builder = conexion();
            List<Producto> listaProductos = new List<Producto>();
            using (SqlConnection conn = new SqlConnection(builder.ConnectionString))
            {
                string query = "SELECT * FROM Productos WHERE nombreProducto = @NOMBREPRODUCTO";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@NOMBREPRODUCTO", nombreProducto);
                    await conn.OpenAsync();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (await reader.ReadAsync())
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

        public async Task<Producto> obtenerProducto(int idProducto)
        {
            SqlConnectionStringBuilder builder = conexion();
            Producto producto = new Producto();
            using (SqlConnection conn = new SqlConnection(builder.ConnectionString))
            {
                string query = "SELECT * FROM Productos WHERE idProducto = @IDPRODUCTO";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IDPRODUCTO", idProducto);
                    await conn.OpenAsync();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            producto.idProducto = Convert.ToInt32(reader["idProducto"].ToString());
                            producto.nombreProducto = Convert.ToString(reader["nombreProducto"].ToString());
                            producto.descripcionProducto = Convert.ToString(reader["descripcionProducto"].ToString());
                            producto.precio = float.Parse(reader["precio"].ToString());
                            producto.precioPromo = float.Parse(reader["precioPromo"].ToString());
                            producto.stock = Convert.ToInt32(reader["stock"].ToString());
                            producto.idCategoria = Convert.ToInt32(reader["idCategoria"].ToString());
                            producto.promocion = Convert.ToBoolean(reader["promocion"].ToString());
                        }
                    }
                    await conn.CloseAsync();
                }
            }
            return producto;
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

        public void actualizarProducto(Producto producto)
        {
            SqlConnectionStringBuilder builder = conexion();
            List<Categoria> listaCategorias = new List<Categoria>();
            using (SqlConnection conn = new SqlConnection(builder.ConnectionString))
            {
                string query = "UPDATE Productos SET nombreProducto = @NOMBREPRODUCTO, descripcionProducto = @DESCRIPCION, precio = @PRECIO, " +
                    "precioPromo = @PRECIOPROMO, stock = @STOCK, idCategoria = @IDCATEGORIA, promocion = @PROMOCION WHERE idProducto = @IDPRODUCTO";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    //cmd.CommandType = CommandType.StoredProcedure;

                    //cmd.Parameters.AddWithValue("@IDPRODUCTO", producto.idProducto);
                    cmd.Parameters.AddWithValue("@NOMBREPRODUCTO", producto.nombreProducto);
                    cmd.Parameters.AddWithValue("@DESCRIPCION", producto.descripcionProducto);
                    cmd.Parameters.AddWithValue("@PRECIO", producto.precio);
                    cmd.Parameters.AddWithValue("@PRECIOPROMO", producto.precioPromo);
                    cmd.Parameters.AddWithValue("@STOCK", producto.stock);
                    cmd.Parameters.AddWithValue("@IDCATEGORIA", producto.idCategoria);
                    cmd.Parameters.AddWithValue("@PROMOCION", producto.promocion);
                    cmd.Parameters.AddWithValue("@IDPRODUCTO", producto.idProducto);

                    conn.OpenAsync();
                    try
                    {
                        cmd.ExecuteNonQuery();
                        conn.Close();
                    }
                    catch (SqlException sqlEx)
                    {
                        conn.Close();
                        throw new Exception("Error al actualizar los datos en la tabla Productos. " + sqlEx.Message);
                    }
                    catch (Exception otherEx)
                    {
                        conn.Close();
                        throw new Exception("Se produjo un error al ejecutar el método UPDATE Productos." + otherEx.Message);
                    }
                    conn.CloseAsync();
                }
            }
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
                            //pedido.cantidad = Convert.ToInt32(reader["cantidad"].ToString());
                            pedido.precioTotal = float.Parse(reader["precioTotal"].ToString());
                            pedido.estadoPedido = Convert.ToBoolean(reader["estadoPedido"].ToString());
                            pedido.fechaCreacion = Convert.ToDateTime(reader["fechaCreacion"]);

                            listaPedidos.Add(pedido);
                        }
                    }
                    await conn.CloseAsync();
                }
            }
            return listaPedidos;
        }

        public async Task<Pedido> obtenerPedido(int idUsuario)
        {
            SqlConnectionStringBuilder builder = conexion();
            Pedido pedido = new Pedido();
            using (SqlConnection conn = new SqlConnection(builder.ConnectionString))
            {
                string query = "SELECT TOP 1 * FROM Pedidos WHERE idUsuario = @IDUSUARIO AND estadoPedido = 0 ORDER BY idPedido DESC";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IDUSUARIO", idUsuario);
                    await conn.OpenAsync();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            pedido = new Pedido();
                            pedido.idPedido = Convert.ToInt32(reader["idPedido"].ToString());
                            pedido.idUsuario = Convert.ToInt32(reader["idUsuario"].ToString());
                            //pedido.cantidad = Convert.ToInt32(reader["cantidad"].ToString());
                            pedido.precioTotal = float.Parse(reader["precioTotal"].ToString());
                            pedido.estadoPedido = Convert.ToBoolean(reader["estadoPedido"].ToString());
                        }
                    }
                    await conn.CloseAsync();
                }
            }
            return pedido;
        }

        public async Task<Pedido> obtenerPedidoXId(int idPedido, int idUsuario)
        {
            SqlConnectionStringBuilder builder = conexion();
            Pedido pedido = new Pedido();
            using (SqlConnection conn = new SqlConnection(builder.ConnectionString))
            {
                string query = "SELECT TOP 1 * FROM Pedidos WHERE idUsuario = @IDUSUARIO AND idPedido = @IDPEDIDO AND estadoPedido = 1 ORDER BY idPedido DESC";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IDUSUARIO", idUsuario);
                    cmd.Parameters.AddWithValue("@IDPEDIDO", idPedido);
                    await conn.OpenAsync();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            pedido = new Pedido();
                            pedido.idPedido = Convert.ToInt32(reader["idPedido"].ToString());
                            pedido.idUsuario = Convert.ToInt32(reader["idUsuario"].ToString());
                            //pedido.cantidad = Convert.ToInt32(reader["cantidad"].ToString());
                            pedido.precioTotal = float.Parse(reader["precioTotal"].ToString());
                            pedido.estadoPedido = Convert.ToBoolean(reader["estadoPedido"].ToString());
                            pedido.fechaCreacion = Convert.ToDateTime(reader["fechaCreacion"]);
                        }
                    }
                    await conn.CloseAsync();
                }
            }
            return pedido;
        }

        //ELIMINAR_PEDIDO
        public async Task<int> eliminarPedido(Pedido pedido)
        {
            SqlConnectionStringBuilder builder = conexion();
            List<Categoria> listaCategorias = new List<Categoria>();
            using (SqlConnection conn = new SqlConnection(builder.ConnectionString))
            {
                string query = "ELIMINAR_PEDIDO";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@IDPEDIDO", pedido.idPedido);
                    cmd.Parameters.AddWithValue("@IDUSUARIO", pedido.idUsuario);

                    await conn.OpenAsync();
                    try
                    {
                        cmd.ExecuteNonQuery();
                        conn.Close();
                    }
                    catch (SqlException sqlEx)
                    {
                        conn.Close();
                        throw new Exception("Error al registrar los datos en la tabla CarritoCompras. " + sqlEx.Message);
                    }
                    catch (Exception otherEx)
                    {
                        conn.Close();
                        throw new Exception("Se produjo un error al ejecutar el método INSERTAR_CARRITOCOMPRAS." + otherEx.Message);
                    }
                    await conn.CloseAsync();
                }
            }
            return 1;
        }

        public async Task<int> insertarPedido(int idUsuario, float precioTotal, bool estadoPedido)
        {
            SqlConnectionStringBuilder builder = conexion();
            List<Categoria> listaCategorias = new List<Categoria>();
            using (SqlConnection conn = new SqlConnection(builder.ConnectionString))
            {
                string query = "INSERTAR_PEDIDO";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@IDUSUARIO", idUsuario);
                    cmd.Parameters.AddWithValue("@PRECIOTOTAL", precioTotal);
                    cmd.Parameters.AddWithValue("@ESTADOPEDIDO", estadoPedido);

                    await conn.OpenAsync();
                    try
                    {
                        cmd.ExecuteNonQuery();
                        conn.Close();
                    }
                    catch (SqlException sqlEx)
                    {
                        conn.Close();
                        throw new Exception("Error al registrar los datos en la tabla Pedidos. " + sqlEx.Message);
                    }
                    catch (Exception otherEx)
                    {
                        conn.Close();
                        throw new Exception("Se produjo un error al ejecutar el método INSERTAR_PEDIDO." + otherEx.Message);
                    }
                    await conn.CloseAsync();
                }
            }
            return 1;
        }

        public async Task<int> actualizarCostoPedido(int idPedido, float precioTotal)
        {
            SqlConnectionStringBuilder builder = conexion();
            List<Categoria> listaCategorias = new List<Categoria>();
            using (SqlConnection conn = new SqlConnection(builder.ConnectionString))
            {
                string query = "UPDATE Pedidos SET precioTotal = @PRECIOTOTAL WHERE idPedido = @IDPEDIDO";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    //cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@IDPEDIDO", idPedido);
                    cmd.Parameters.AddWithValue("@PRECIOTOTAL", precioTotal);


                    await conn.OpenAsync();
                    try
                    {
                        cmd.ExecuteNonQuery();
                        conn.Close();
                    }
                    catch (SqlException sqlEx)
                    {
                        conn.Close();
                        throw new Exception("Error al actualizar los datos en la tabla Pedidos. " + sqlEx.Message);
                    }
                    catch (Exception otherEx)
                    {
                        conn.Close();
                        throw new Exception("Se produjo un error al ejecutar el método UPDATE Pedido." + otherEx.Message);
                    }
                    await conn.CloseAsync();
                }
            }
            return 1;
        }

        //INSERTAR_CARRITOCOMPRAS
        public async Task<int> insertarCarritoCompras(CarritoCompra carrito)
        {
            SqlConnectionStringBuilder builder = conexion();
            List<Categoria> listaCategorias = new List<Categoria>();
            using (SqlConnection conn = new SqlConnection(builder.ConnectionString))
            {
                string query = "INSERTAR_CARRITOCOMPRAS";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    //cmd.Parameters.AddWithValue("@IDPRODUCTO", producto.idProducto);
                    cmd.Parameters.AddWithValue("@IDUSUARIO", carrito.idUsuario);
                    cmd.Parameters.AddWithValue("@IDPEDIDO", carrito.idPedido);
                    cmd.Parameters.AddWithValue("@IDPRODUCTO", carrito.idProducto);
                    cmd.Parameters.AddWithValue("@CANTIDAD", carrito.cantidad);

                    await conn.OpenAsync();
                    try
                    {
                        cmd.ExecuteNonQuery();
                        conn.Close();
                    }
                    catch (SqlException sqlEx)
                    {
                        conn.Close();
                        throw new Exception("Error al registrar los datos en la tabla CarritoCompras. " + sqlEx.Message);
                    }
                    catch (Exception otherEx)
                    {
                        conn.Close();
                        throw new Exception("Se produjo un error al ejecutar el método INSERTAR_CARRITOCOMPRAS." + otherEx.Message);
                    }
                    await conn.CloseAsync();
                }
            }
            return 1;
        }

        public async Task<List<CarritoCompra>> obtenerCarritosCompras(int idPedido)
        {
            SqlConnectionStringBuilder builder = conexion();
            CarritoCompra carritoCompra = new CarritoCompra();
            List<CarritoCompra> listaCarritos = new List<CarritoCompra>();
            using (SqlConnection conn = new SqlConnection(builder.ConnectionString))
            {
                string query = "SELECT idCarrito, idUsuario, idPedido, idProducto, cantidad, montoTotal FROM CarritoCompras WHERE idPedido = @IDPEDIDO";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    //cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@IDPEDIDO", idPedido);

                    await conn.OpenAsync();
                    try
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                carritoCompra = new CarritoCompra();
                                carritoCompra.idCarrito = Convert.ToInt32(reader["idCarrito"].ToString());
                                carritoCompra.idUsuario = Convert.ToInt32(reader["idUsuario"].ToString());
                                carritoCompra.idPedido = Convert.ToInt32(reader["idPedido"].ToString());
                                carritoCompra.idProducto = Convert.ToInt32(reader["idProducto"].ToString());
                                carritoCompra.cantidad = Convert.ToInt32(reader["cantidad"].ToString());
                                carritoCompra.montoTotal = float.Parse(reader["montoTotal"].ToString());

                                listaCarritos.Add(carritoCompra);
                            }
                        }
                        await conn.CloseAsync();
                    }
                    catch (SqlException sqlEx)
                    {
                        conn.Close();
                        throw new Exception("Error al obtener los datos de la tabla CarritoCompras. " + sqlEx.Message);
                    }
                }
            }
            return listaCarritos;
        }

        public async Task<CarritoCompra> obtenerProductoCarrito(int idCarrito)
        {
            SqlConnectionStringBuilder builder = conexion();
            CarritoCompra carritoCompra = new CarritoCompra();
            using (SqlConnection conn = new SqlConnection(builder.ConnectionString))
            {
                string query = "SELECT idCarrito, idUsuario, idPedido, idProducto, cantidad, montoTotal FROM CarritoCompras WHERE idCarrito = @IDCARRITO";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    //cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@IDCARRITO", idCarrito);

                    await conn.OpenAsync();
                    try
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                carritoCompra = new CarritoCompra();
                                carritoCompra.idCarrito = Convert.ToInt32(reader["idCarrito"].ToString());
                                carritoCompra.idUsuario = Convert.ToInt32(reader["idUsuario"].ToString());
                                carritoCompra.idPedido = Convert.ToInt32(reader["idPedido"].ToString());
                                carritoCompra.idProducto = Convert.ToInt32(reader["idProducto"].ToString());
                                carritoCompra.cantidad = Convert.ToInt32(reader["cantidad"].ToString());
                                carritoCompra.montoTotal = float.Parse(reader["montoTotal"].ToString());
                            }
                        }
                        await conn.CloseAsync();
                    }
                    catch (SqlException sqlEx)
                    {
                        conn.Close();
                        throw new Exception("Error al obtener los datos de la tabla CarritoCompras. " + sqlEx.Message);
                    }
                }
            }
            return carritoCompra;
        }

        //ACTUALIZAR_PRODUCTO_CARRITO
        public async Task<int> actualizarProductoCarrito(CarritoCompra carrito)
        {
            SqlConnectionStringBuilder builder = conexion();
            List<Categoria> listaCategorias = new List<Categoria>();
            using (SqlConnection conn = new SqlConnection(builder.ConnectionString))
            {
                string query = "ACTUALIZAR_PRODUCTO_CARRITO";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@CANTIDAD", carrito.cantidad);
                    cmd.Parameters.AddWithValue("@IDPRODUCTO", carrito.idProducto);
                    cmd.Parameters.AddWithValue("@IDCARRITO", carrito.idCarrito);
                    cmd.Parameters.AddWithValue("@IDPEDIDO", carrito.idPedido);
                    cmd.Parameters.AddWithValue("@IDUSUARIO", carrito.idUsuario);

                    await conn.OpenAsync();
                    try
                    {
                        await cmd.ExecuteNonQueryAsync();
                        conn.Close();
                    }
                    catch (SqlException sqlEx)
                    {
                        conn.Close();
                        throw new Exception("Error al actualizar los datos en la tabla CarritoCompras. " + sqlEx.Message);
                    }
                    catch (Exception otherEx)
                    {
                        conn.Close();
                        throw new Exception("Se produjo un error al ejecutar el método UPDATE ACTUALIZAR_PRODUCTO_CARRITO. " + otherEx.Message);
                    }
                    await conn.CloseAsync();
                }
            }
            return 1;
        }

        //ELIMINAR_PRODUCTO_CARRITO
        public async Task<int> eliminarProductoCarrito(CarritoCompra carrito)
        {
            SqlConnectionStringBuilder builder = conexion();
            List<Categoria> listaCategorias = new List<Categoria>();
            using (SqlConnection conn = new SqlConnection(builder.ConnectionString))
            {
                string query = "ELIMINAR_PRODUCTO_CARRITO";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@IDCARRITO", carrito.idCarrito);
                    cmd.Parameters.AddWithValue("@IDPEDIDO", carrito.idPedido);
                    cmd.Parameters.AddWithValue("@IDUSUARIO", carrito.idUsuario);

                    await conn.OpenAsync();
                    try
                    {
                        await cmd.ExecuteNonQueryAsync();
                        conn.Close();
                    }
                    catch (SqlException sqlEx)
                    {
                        conn.Close();
                        throw new Exception("Error al actualizar los datos en la tabla CarritoCompras. " + sqlEx.Message);
                    }
                    catch (Exception otherEx)
                    {
                        conn.Close();
                        throw new Exception("Se produjo un error al ejecutar el método UPDATE ELIMINAR_PRODUCTO_CARRITO. " + otherEx.Message);
                    }
                    await conn.CloseAsync();
                }
            }
            return 1;
        }

        public async Task<List<Pago>> ObtenerPagos(int idCliente)
        {
            SqlConnectionStringBuilder builder = conexion();
            List<Pago> listaPagos = new List<Pago>();
            Pago pago;
            try
            {
                using (SqlConnection conn = new SqlConnection(builder.ToString()))
                {
                    string query = "SELECT * FROM Pagos WHERE idCliente = @IDCLIENTE";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@IDCLIENTE", idCliente);
                        await conn.OpenAsync();
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (reader.Read())
                            {
                                pago = new Pago();
                                pago.pk_tsal001 = reader["pk_tsal001"] as string ?? "";
                                pago.idPedido = reader["idPedido"] as int? ?? 0;
                                pago.terminalId = reader["terminalId"] as string ?? "";
                                pago.transactionType = reader["transactionType"] as string ?? "";
                                pago.invoice = reader["invoice"] as string ?? "";
                                pago.totalAmount = reader["totalAmount"] as string ?? "";
                                pago.taxAmount = reader["taxAmount"] as string ?? "";
                                pago.tipAmount = reader["tipAmount"] as string ?? "";
                                pago.clientEmail = reader["clientEmail"] as string ?? "";
                                pago.idCliente = reader["idCliente"] as int? ?? 0;

                                listaPagos.Add(pago);
                            }
                        }
                        await conn.CloseAsync();
                    }
                }
                return listaPagos;
            }
            catch (Exception e)
            {
                return listaPagos = new List<Pago>();
                throw new Exception("Error cargando los pagos" + e.Message);
            }
        }

        public async Task<int> insertarPago(Pago pago)
        {
            SqlConnectionStringBuilder builder = conexion();
            List<Categoria> listaCategorias = new List<Categoria>();
            using (SqlConnection conn = new SqlConnection(builder.ConnectionString))
            {
                string query = "INSERTAR_PAGO";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@PK_TSAL001", pago.pk_tsal001);
                    cmd.Parameters.AddWithValue("@IDPEDIDO", pago.idPedido);
                    cmd.Parameters.AddWithValue("@TERMINALID", pago.terminalId);
                    cmd.Parameters.AddWithValue("@TRANSACTIONTYPE", pago.transactionType);
                    cmd.Parameters.AddWithValue("@INVOICE", pago.invoice);
                    cmd.Parameters.AddWithValue("@TOTALAMOUNT", pago.totalAmount);
                    cmd.Parameters.AddWithValue("@TAXAMOUNT", pago.taxAmount);
                    cmd.Parameters.AddWithValue("@TIPAMOUNT", pago.tipAmount);
                    cmd.Parameters.AddWithValue("@CLIENTEMAIL", pago.clientEmail);
                    cmd.Parameters.AddWithValue("@IDCLIENTE", pago.idCliente);

                    await conn.OpenAsync();
                    try
                    {
                        cmd.ExecuteNonQuery();
                        conn.Close();
                    }
                    catch (SqlException sqlEx)
                    {
                        conn.Close();
                        throw new Exception("Error al registrar los datos en la tabla de Pagos. " + sqlEx.Message);
                    }
                    catch (Exception otherEx)
                    {
                        conn.Close();
                        throw new Exception("Se produjo un error al ejecutar el método INSERTAR_PAGO." + otherEx.Message);
                    }
                    await conn.CloseAsync();
                }
            }
            return 1;
        }

        public async Task actualizarPedido(int idPedido)
        {
            SqlConnectionStringBuilder builder = conexion();
            using (SqlConnection conn = new SqlConnection(builder.ConnectionString))
            {
                try
                {
                    await conn.OpenAsync();
                    var query = "UPDATE Pedidos SET estadoPedido = @ESTADO, fechaCreacion = (SELECT GETDATE()) WHERE idPedido = @IDPEDIDO;";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ESTADO", 1);
                        cmd.Parameters.AddWithValue("@IDPEDIDO", idPedido);

                        await cmd.ExecuteNonQueryAsync();
                    }
                }
                catch (Exception e)
                {

                    throw new Exception("Error al actualizar el estado del pedido. " + e.Message);
                }
            }
        }

        public async Task<List<CantidadesProducto>> obtenerCantidades(int idPedido)
        {
            SqlConnectionStringBuilder builder = conexion();
            List<CantidadesProducto> listaCantidades = new List<CantidadesProducto>();
            CantidadesProducto cantidad;
            try
            {
                using (SqlConnection conn = new SqlConnection(builder.ToString()))
                {
                    string query = "SELECT C.idProducto, C.cantidad FROM Pedidos P" +
                        "INNER JOIN CarritoCompras C ON P.idPedido = C.idPedido" +
                        "WHERE P.idPedido = @IDPEDIDO AND P.estadoPedido = 1";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@IDPEDIDO", idPedido);
                        await conn.OpenAsync();
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (reader.Read())
                            {
                                cantidad = new CantidadesProducto();
                                cantidad.idProducto = Convert.ToInt32(reader["idProducto"]);
                                cantidad.cantidad = Convert.ToInt32(reader["cantidad"]);


                                listaCantidades.Add(cantidad);
                            }
                        }
                    }
                }
                return listaCantidades;
            }
            catch (Exception e)
            {
                return listaCantidades = new List<CantidadesProducto>();
                throw new Exception("Error cargando las cantidades de productos" + e.Message);
            }
        }

    }
}
