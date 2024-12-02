using CompraOnline.Models.Productos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompraOnline.Tests
{
    public class PromocionTests
    {
        [Fact]
        public void CalcularPrecioPromocional_AplicaDescuentoCorrecto()
        {
            // Arrange
            var producto = new Producto { precio = 100, precioPromo = 80 };

            // Act
            var descuento = producto.precio - producto.precioPromo;

            // Assert
            Assert.Equal(20, descuento);
        }
    }
}
