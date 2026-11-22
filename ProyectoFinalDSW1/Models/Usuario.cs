using System.ComponentModel.DataAnnotations;

namespace ProyectoFinalDSW1.Models
{
    public class Usuario
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(50, ErrorMessage = "El nombre no puede superar los 50 caracteres")]
        [Display(Name = "Nombre completo")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es obligatorio")]
        [StringLength(50, ErrorMessage = "El correo no puede superar los 50 caracteres")]
        [EmailAddress(ErrorMessage = "Correo electrónico inválido")]
        [Display(Name = "Correo electrónico")]
        public string Correo { get; set; } = string.Empty;

     
        [Required]
        [StringLength(200)]
        public string ClaveHash { get; set; } = string.Empty;

        [Required(ErrorMessage = "El rol es obligatorio")]
        public Rol Rol { get; set; }

        public bool Activo { get; set; } = true;

        [Range(0, 10)]
        public int IntentosFallidos { get; set; } = 0;

        public DateTime? BloqueadoHasta { get; set; }

        [Required]
        [Display(Name = "Fecha de registro")]
        public DateTime FechaRegistro { get; set; } = DateTime.Now;
    }
}
