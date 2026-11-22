using System.ComponentModel.DataAnnotations;

namespace ProyectoFinalDSW1.Models
{
    public class Vehiculo
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "La placa es obligatoria")]
        [RegularExpression(@"^[A-Z0-9]{6}$", ErrorMessage = "La placa debe tener 6 letras o números en mayúscula, sin guión")]
        public string Placa { get; set; } = string.Empty;

        [Required(ErrorMessage = "La marca es obligatoria")]
        [StringLength(30)]
        public string Marca { get; set; } = string.Empty;

        [Required(ErrorMessage = "El modelo es obligatorio")]
        [StringLength(30)]
        public string Modelo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El año es obligatorio")]
        [Range(1980, 2100, ErrorMessage = "Año inválido")]
        [Display(Name = "Año")]
        public int Anio { get; set; }

        [Range(0, 2000000, ErrorMessage = "Kilometraje inválido")]
        public int Kilometraje { get; set; }

        [Required(ErrorMessage = "El cliente es obligatorio")]
        [Display(Name = "Cliente")]
        public int ClienteId { get; set; }
        public Cliente? Cliente { get; set; }

        public List<OrdenTrabajo> Ordenes { get; set; } = new();
    }
}
