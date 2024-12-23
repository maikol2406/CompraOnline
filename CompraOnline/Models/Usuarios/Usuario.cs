﻿using System.ComponentModel.DataAnnotations;

namespace CompraOnline.Models.Usuarios
{
    public class Usuario
    {
        [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
        public int idUsuario { get; set; }
        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [Display(Name = "Nombre de usuario")]
        public string username { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string password { get; set; }
        [Required]
        [Display(Name = "Confirmar contraseña")]
        [DataType(DataType.Password)]
        [Compare("password", ErrorMessage = "Las contraseñas no coinciden")]
        public string confirmaPassword { get; set; }
        [Required]
        [Display(Name = "Nombre completo")]
        public string nombreCompleto { get; set; }
    }
}
