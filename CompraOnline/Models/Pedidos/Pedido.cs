using System.ComponentModel.DataAnnotations;

namespace CompraOnline.Models.Pedidos
{
    public class ListaPedidos
    {
        public List<Pedido> listaPedidos { get; set; }
    }

    public class Pedido
    {
        [Required]
        [Display(Name = "# de pedido")]
        public int idPedido { get; set; }
        [Required]
        public int idUsuario { get; set; }
        [Display(Name = "Cantidad")]
        public int cantidad { get; set; }
        [Display(Name = "Total pedido")]
        public float precioTotal { get; set; }
        [Required]
        [Display(Name = "Estado del pedido")]
        public bool estadoPedido { get; set; }
    }
}
