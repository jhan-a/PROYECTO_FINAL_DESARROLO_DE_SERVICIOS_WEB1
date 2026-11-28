using System.Data;
using Microsoft.Data.SqlClient;
using ProyectoFinalDSW1.Models;

namespace ProyectoFinalDSW1.Data
{
    // Listados paginados con procedimientos almacenados (mismo estilo del proyecto de clonación)
    public class TallerRepository
    {
        private readonly string _connectionString;

        public TallerRepository(IConfiguration configuracion)
        {
            _connectionString = configuracion.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Falta la cadena de conexión DefaultConnection");
        }

        private async Task<(List<T> items, int total)> Listar<T>(
            string procedimiento,
            Action<SqlParameterCollection> parametros,
            Func<SqlDataReader, T> mapear)
        {
            var items = new List<T>();
            await using var conn = new SqlConnection(_connectionString);
            await using var cmd = new SqlCommand(procedimiento, conn);
            cmd.CommandType = CommandType.StoredProcedure;
            parametros(cmd.Parameters);
            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                items.Add(mapear(reader));
            }
            int total = 0;
            if (await reader.NextResultAsync() && await reader.ReadAsync())
            {
                total = reader.GetInt32(0);
            }
            return (items, total);
        }

        public Task<(List<Usuario> items, int total)> ListarUsuarios(string? buscar, int page, int pageSize)
        {
            return Listar("dbo.sp_listar_usuarios_paginados",
                p =>
                {
                    p.AddWithValue("@pageSize", pageSize);
                    p.AddWithValue("@page", page);
                    p.AddWithValue("@buscar", buscar ?? "");
                },
                r => new Usuario
                {
                    Id = r.GetInt32(0),
                    Nombre = r.GetString(1),
                    Correo = r.GetString(2),
                    Rol = (Rol)r.GetInt32(3),
                    Activo = r.GetBoolean(4),
                    IntentosFallidos = r.GetInt32(5),
                    BloqueadoHasta = r.IsDBNull(6) ? null : r.GetDateTime(6),
                    FechaRegistro = r.GetDateTime(7)
                });
        }

        public Task<(List<Cliente> items, int total)> ListarClientes(string? buscar, int page, int pageSize)
        {
            return Listar("dbo.sp_listar_clientes_paginados",
                p =>
                {
                    p.AddWithValue("@pageSize", pageSize);
                    p.AddWithValue("@page", page);
                    p.AddWithValue("@buscar", buscar ?? "");
                },
                r => new Cliente
                {
                    Id = r.GetInt32(0),
                    Nombre = r.GetString(1),
                    Dni = r.GetString(2),
                    Telefono = r.GetString(3),
                    Correo = r.GetString(4),
                    Activo = r.GetBoolean(5)
                });
        }

        public Task<(List<Vehiculo> items, int total)> ListarVehiculos(string? buscar, int page, int pageSize)
        {
            return Listar("dbo.sp_listar_vehiculos_paginados",
                p =>
                {
                    p.AddWithValue("@pageSize", pageSize);
                    p.AddWithValue("@page", page);
                    p.AddWithValue("@buscar", buscar ?? "");
                },
                r => new Vehiculo
                {
                    Id = r.GetInt32(0),
                    Placa = r.GetString(1),
                    Marca = r.GetString(2),
                    Modelo = r.GetString(3),
                    Anio = r.GetInt32(4),
                    Kilometraje = r.GetInt32(5),
                    ClienteId = r.GetInt32(6),
                    Cliente = new Cliente { Id = r.GetInt32(6), Nombre = r.GetString(7) }
                });
        }

        public Task<(List<Servicio> items, int total)> ListarServicios(string? buscar, int page, int pageSize)
        {
            return Listar("dbo.sp_listar_servicios_paginados",
                p =>
                {
                    p.AddWithValue("@pageSize", pageSize);
                    p.AddWithValue("@page", page);
                    p.AddWithValue("@buscar", buscar ?? "");
                },
                r => new Servicio
                {
                    Id = r.GetInt32(0),
                    Nombre = r.GetString(1),
                    Descripcion = r.IsDBNull(2) ? null : r.GetString(2),
                    Precio = r.GetDecimal(3),
                    Activo = r.GetBoolean(4)
                });
        }

        public Task<(List<Repuesto> items, int total)> ListarRepuestos(string? buscar, bool soloAlerta, int page, int pageSize)
        {
            return Listar("dbo.sp_listar_repuestos_paginados",
                p =>
                {
                    p.AddWithValue("@pageSize", pageSize);
                    p.AddWithValue("@page", page);
                    p.AddWithValue("@buscar", buscar ?? "");
                    p.AddWithValue("@soloAlerta", soloAlerta);
                },
                r => new Repuesto
                {
                    Id = r.GetInt32(0),
                    Nombre = r.GetString(1),
                    Precio = r.GetDecimal(2),
                    Stock = r.GetInt32(3),
                    StockMinimo = r.GetInt32(4),
                    Activo = r.GetBoolean(5)
                });
        }

        public Task<(List<OrdenTrabajo> items, int total)> ListarOrdenes(string? buscar, int? estado, int? mecanicoId, int page, int pageSize)
        {
            return Listar("dbo.sp_listar_ordenes_paginadas",
                p =>
                {
                    p.AddWithValue("@pageSize", pageSize);
                    p.AddWithValue("@page", page);
                    p.AddWithValue("@buscar", buscar ?? "");
                    p.AddWithValue("@estado", (object?)estado ?? DBNull.Value);
                    p.AddWithValue("@mecanicoId", (object?)mecanicoId ?? DBNull.Value);
                },
                r => new OrdenTrabajo
                {
                    Id = r.GetInt32(0),
                    FechaIngreso = r.GetDateTime(1),
                    FechaEntrega = r.IsDBNull(2) ? null : r.GetDateTime(2),
                    Estado = (EstadoOrden)r.GetInt32(3),
                    CostoTotal = r.GetDecimal(4),
                    VehiculoId = r.GetInt32(5),
                    Vehiculo = new Vehiculo
                    {
                        Id = r.GetInt32(5),
                        Placa = r.GetString(6),
                        Marca = r.GetString(7),
                        Modelo = r.GetString(8),
                        Cliente = new Cliente { Nombre = r.GetString(9) }
                    },
                    MecanicoId = r.IsDBNull(10) ? null : r.GetInt32(10),
                    Mecanico = r.IsDBNull(10) ? null : new Usuario { Id = r.GetInt32(10), Nombre = r.GetString(11) }
                });
        }
    }
}
