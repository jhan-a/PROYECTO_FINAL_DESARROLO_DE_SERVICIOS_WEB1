using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoFinalDSW1.Models
{
    public class DetalleOrden : IValidatableObject
    {
        public int Id { get; set; }

        [Required]
        public int OrdenTrabajoId { get; set; }
        public OrdenTrabajo? OrdenTrabajo { get; set; }

        public int? ServicioId { get; set; }
        public Servicio? Servicio { get; set; }

        public int? RepuestoId { get; set; }
        public Repuesto? Repuesto { get; set; }

        [Required]
        [Range(1, 1000, ErrorMessage = "La cantidad debe ser entre 1 y 1000")]
        public int Cantidad { get; set; } = 1;

        [Column(TypeName = "decimal(10,2)")]
        [Range(0, 1000000)]
        public decimal Subtotal { get; set; }

        
        public IEnumerable<ValidationResult> Validate(ValidationContext context)
        {
            if (ServicioId == null && RepuestoId == null)
                yield return new ValidationResult("Debe elegir un servicio o un repuesto");

            if (ServicioId != null && RepuestoId != null)
                yield return new ValidationResult("Elija solo un servicio o un repuesto, no ambos");
        }
    }
}
