namespace ProyectoFinalDSW1.Models
{
    public static class EstadoOrdenExtensions
    {
        public static string Texto(this EstadoOrden estado) => estado switch
        {
            EstadoOrden.Pendiente => "Pendiente",
            EstadoOrden.EnReparacion => "En reparación",
            EstadoOrden.Terminado => "Terminado",
            EstadoOrden.Entregado => "Entregado",
            _ => "Desconocido"
        };

        public static string Clase(this EstadoOrden estado) => estado switch
        {
            EstadoOrden.Pendiente => "bg-secondary",
            EstadoOrden.EnReparacion => "bg-warning text-dark",
            EstadoOrden.Terminado => "bg-info text-dark",
            EstadoOrden.Entregado => "bg-success",
            _ => "bg-dark"
        };
    }
}
