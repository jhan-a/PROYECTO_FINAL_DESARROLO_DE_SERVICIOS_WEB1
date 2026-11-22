using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoFinalDSW1.Models
{
    public class OrdenTrabajo
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Fecha de ingreso")]
        public DateTime FechaIngreso { get; set; } = DateTime.Now;

        [Display(Name = "Fecha de entrega")]
        public DateTime? FechaEntrega { get; set; }

        [Required]
        public EstadoOrden Estado { get; set; } = EstadoOrden.Pendiente;

        [Column(TypeName = "decimal(10,2)")]
        [Range(0, 1000000)]
        [Display(Name = "Costo total")]
        public decimal CostoTotal { get; set; } = 0;

        [StringLength(300, ErrorMessage = "Las observaciones no pueden superar los 300 caracteres")]
        public string? Observaciones { get; set; }

        [Required(ErrorMessage = "El vehículo es obligatorio")]
        [Display(Name = "Vehículo")]
        public int VehiculoId { get; set; }
        public Vehiculo? Vehiculo { get; set; }

        // Se asigna después de crear la orden
        [Display(Name = "Mecánico")]
        public int? MecanicoId { get; set; }
        public Usuario? Mecanico { get; set; }

        public List<DetalleOrden> Detalles { get; set; } = new();

        // Evita que dos personas editen la misma orden a la vez
        [Timestamp]
        public byte[]? RowVersion { get; set; }
    }
}
