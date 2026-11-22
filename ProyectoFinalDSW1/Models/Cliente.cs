using System.ComponentModel.DataAnnotations;

namespace ProyectoFinalDSW1.Models
{
    public class Cliente
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(80, ErrorMessage = "El nombre no puede superar los 80 caracteres")]
        [Display(Name = "Nombre completo")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El DNI es obligatorio")]
        [RegularExpression(@"^\d{8}$", ErrorMessage = "El DNI debe tener 8 dígitos")]
        public string Dni { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es obligatorio")]
        [RegularExpression(@"^9\d{8}$", ErrorMessage = "El teléfono debe tener 9 dígitos y empezar con 9")]
        [Display(Name = "Teléfono")]
        public string Telefono { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es obligatorio")]
        [StringLength(50)]
        [EmailAddress(ErrorMessage = "Correo electrónico inválido")]
        public string Correo { get; set; } = string.Empty;

        public bool Activo { get; set; } = true;

        public List<Vehiculo> Vehiculos { get; set; } = new();
    }
}
