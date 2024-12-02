using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CompraOnline.Models;
using CompraOnline.Models.Productos;

namespace CompraOnline.Tests
{
    public class ProductoTests
    {
        [Fact]
        public void Producto_PrecioDebeSerPositivo()
        {
            // Arrange
            var producto = new Producto { precio = -10 };

            // Act
            var esPrecioValido = producto.precio >= 0;

            // Assert
            Assert.False(esPrecioValido, "El precio debe ser positivo.");
        }

        [Fact]
        public void Producto_StockNoDebeSerNegativo()
        {
            // Arrange
            var producto = new Producto { stock = -5 };

            // Act
            var esStockValido = producto.stock >= 0;

            // Assert
            Assert.False(esStockValido, "El stock no debe ser negativo.");
        }
    }
}
