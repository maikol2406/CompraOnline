using System.ComponentModel.DataAnnotations;

namespace CompraOnline.Models.Productos
{
    public class Producto
    {
        //[Required]
        public int idProducto { get; set; }
        [Required]
        [Display(Name = "Nombre del producto")]
        public string nombreProducto { get; set; }
        [Required]
        [Display(Name = "Descripción")]
        public string descripcionProducto { get; set; }
        [Required]
        [Display(Name = "Precio")]
        public float precio { get; set; }
        [Required]
        [Display(Name = "Precio promoción")]
        public float? precioPromo { get; set; }
        [Required]
        [Display(Name = "Cantidas en stock")]
        public int stock { get; set; }
        [Required]
        public int idCategoria { get; set; }
        [Display(Name = "Promoción")]
        public bool promocion { get; set; }
        public string? imagen { get; set; }
    }
}
