﻿using System.ComponentModel.DataAnnotations;

namespace CompraOnline.Models.Productos
{
    public class Categoria
    {
        [Required]
        public int idCategoria { get; set; }
        [Required]
        [Display(Name = "Nombre categoría")]
        public string nombreCategoria { get; set; }
    }
}
