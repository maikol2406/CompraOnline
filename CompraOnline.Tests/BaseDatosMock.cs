using CompraOnline.Models.Pedidos;
using CompraOnline.Models.Productos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompraOnline.Tests
{
    public class BaseDatosMock
    {
        public Task<Producto> obtenerProducto(int idProducto)
        {
            var producto = idProducto == 1
                ? new Producto { idProducto = 1, nombreProducto = "Producto Prueba", precio = 100 }
                : null;

            return Task.FromResult(producto);
        }

        public Task<List<Pedido>> obtenerPedidos(int idUsuario)
        {
            var pedidos = new List<Pedido>
        {
            new Pedido { idPedido = 1, idUsuario = idUsuario, estadoPedido = true }
        };
            return Task.FromResult(pedidos);
        }

        public Task<List<Producto>> obtenerProductos()
        {
            var productos = new List<Producto>
        {
            new Producto { idProducto = 1, nombreProducto = "Producto 1", precio = 100 },
            new Producto { idProducto = 2, nombreProducto = "Producto 2", precio = 200 }
        };
            return Task.FromResult(productos);
        }
    }
}
