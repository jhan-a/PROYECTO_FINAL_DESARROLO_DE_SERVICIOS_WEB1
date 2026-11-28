using System.ComponentModel.DataAnnotations;

namespace ProyectoFinalDSW1.Models
{
    public enum EstadoOrden
    {
        Pendiente = 1,

        [Display(Name = "En reparación")]
        EnReparacion = 2,

        Terminado = 3,
        Entregado = 4
    }
}
