using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoFinalDSW1.Models
{
    public class Repuesto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(60)]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El precio es obligatorio")]
        [Range(0.01, 100000, ErrorMessage = "Precio inválido")]
        [Column(TypeName = "decimal(10,2)")]
        [DataType(DataType.Currency)]
        public decimal Precio { get; set; }

        [Required]
        [Range(0, 100000, ErrorMessage = "El stock no puede ser negativo")]
        public int Stock { get; set; }

        [Required]
        [Range(0, 100000, ErrorMessage = "El stock mínimo no puede ser negativo")]
        [Display(Name = "Stock mínimo")]
        public int StockMinimo { get; set; }

        public bool Activo { get; set; } = true;
    }
}
