using System.ComponentModel.DataAnnotations;

namespace CompraOnline.Models.CarritoCompras
{
    public class CarritoCompra
    {
        public int idCarrito { get; set; }
        public int idUsuario { get; set; }
        public int idPedido { get; set; }
        public int idProducto { get; set; }
        [Display(Name = "Cantidad")]
        [Range(1,int.MaxValue, ErrorMessage =("La cantidad debe ser mayor que 1"))]
        public int cantidad { get; set; }
        [Display(Name ="Monto total")]
        public float montoTotal { get; set; }
    }
}
