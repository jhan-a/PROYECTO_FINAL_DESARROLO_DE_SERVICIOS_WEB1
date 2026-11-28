using System.ComponentModel.DataAnnotations;

namespace ProyectoFinalDSW1.Models
{
    public static class ReglasClave
    {
        public const string Patron = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\w\s]).+$";
        public const string Mensaje = "La contraseña debe tener mayúscula, minúscula, número y símbolo";
    }

    public class LoginViewModel
    {
        [Required(ErrorMessage = "El correo es obligatorio")]
        [StringLength(50, ErrorMessage = "El correo no puede superar los 50 caracteres")]
        [EmailAddress(ErrorMessage = "Correo electrónico inválido")]
        [Display(Name = "Correo electrónico")]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [StringLength(50, ErrorMessage = "La contraseña no puede superar los 50 caracteres")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Clave { get; set; } = string.Empty;

        public string? ReturnUrl { get; set; }
    }

    public class CambiarClaveViewModel
    {
        [Required(ErrorMessage = "La contraseña actual es obligatoria")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña actual")]
        public string ClaveActual { get; set; } = string.Empty;

        [Required(ErrorMessage = "La nueva contraseña es obligatoria")]
        [StringLength(50, MinimumLength = 8, ErrorMessage = "La contraseña debe tener entre 8 y 50 caracteres")]
        [RegularExpression(ReglasClave.Patron, ErrorMessage = ReglasClave.Mensaje)]
        [DataType(DataType.Password)]
        [Display(Name = "Nueva contraseña")]
        public string ClaveNueva { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirme la nueva contraseña")]
        [Compare(nameof(ClaveNueva), ErrorMessage = "Las contraseñas no coinciden")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar contraseña")]
        public string ConfirmarClave { get; set; } = string.Empty;
    }

    public class UsuarioCrearViewModel
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(50, ErrorMessage = "El nombre no puede superar los 50 caracteres")]
        [Display(Name = "Nombre completo")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es obligatorio")]
        [StringLength(50, ErrorMessage = "El correo no puede superar los 50 caracteres")]
        [EmailAddress(ErrorMessage = "Correo electrónico inválido")]
        [Display(Name = "Correo electrónico")]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El rol es obligatorio")]
        [EnumDataType(typeof(Rol), ErrorMessage = "Rol inválido")]
        public Rol? Rol { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [StringLength(50, MinimumLength = 8, ErrorMessage = "La contraseña debe tener entre 8 y 50 caracteres")]
        [RegularExpression(ReglasClave.Patron, ErrorMessage = ReglasClave.Mensaje)]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Clave { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirme la contraseña")]
        [Compare(nameof(Clave), ErrorMessage = "Las contraseñas no coinciden")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar contraseña")]
        public string ConfirmarClave { get; set; } = string.Empty;
    }

    public class UsuarioEditarViewModel
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

        [Required(ErrorMessage = "El rol es obligatorio")]
        [EnumDataType(typeof(Rol), ErrorMessage = "Rol inválido")]
        public Rol? Rol { get; set; }

        public bool Activo { get; set; } = true;

        // Opcional: solo se llena si el administrador quiere restablecer la contraseña
        [StringLength(50, MinimumLength = 8, ErrorMessage = "La contraseña debe tener entre 8 y 50 caracteres")]
        [RegularExpression(ReglasClave.Patron, ErrorMessage = ReglasClave.Mensaje)]
        [DataType(DataType.Password)]
        [Display(Name = "Nueva contraseña (opcional)")]
        public string? NuevaClave { get; set; }
    }

    public class PaginacionViewModel
    {
        public int Pagina { get; set; } = 1;
        public int TotalPaginas { get; set; } = 1;
        public string? Buscar { get; set; }
        public int? Estado { get; set; }
        public bool Alerta { get; set; }
    }

    public class ListaPaginada<T> : PaginacionViewModel
    {
        public List<T> Items { get; set; } = new();

        public static ListaPaginada<T> Crear(List<T> items, int total, int pagina, int tamano, string? buscar, int? estado = null)
        {
            return new ListaPaginada<T>
            {
                Items = items,
                Pagina = pagina,
                TotalPaginas = Math.Max(1, (int)Math.Ceiling((double)total / tamano)),
                Buscar = buscar,
                Estado = estado
            };
        }
    }

    public class DashboardViewModel
    {
        public int Pendientes { get; set; }
        public int EnReparacion { get; set; }
        public int Terminadas { get; set; }
        public int TotalClientes { get; set; }
        public int TotalVehiculos { get; set; }
        public List<Repuesto> RepuestosEnAlerta { get; set; } = new();
    }
}
