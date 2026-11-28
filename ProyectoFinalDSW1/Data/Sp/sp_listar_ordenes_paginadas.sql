CREATE OR ALTER PROCEDURE sp_listar_ordenes_paginadas
(
	@pageSize INT = 8,
	@page INT = 1,
	@buscar NVARCHAR(100) = '',
	@estado INT = NULL, /* NULL = todos los estados */
	@mecanicoId INT = NULL /* NULL = todas las órdenes (administrador) */
)
AS
BEGIN
	SET NOCOUNT ON
	DECLARE @inicio INT = (@page - 1) * @pageSize
	SELECT o.Id, o.FechaIngreso, o.FechaEntrega, o.Estado, o.CostoTotal, o.VehiculoId,
		v.Placa, v.Marca, v.Modelo, c.Nombre AS Cliente, o.MecanicoId, u.Nombre AS Mecanico
	FROM OrdenesTrabajo o
	INNER JOIN Vehiculos v ON v.Id = o.VehiculoId
	INNER JOIN Clientes c ON c.Id = v.ClienteId
	LEFT JOIN Usuarios u ON u.Id = o.MecanicoId
	WHERE (v.Placa LIKE '%' + @buscar + '%' OR c.Nombre LIKE '%' + @buscar + '%')
		AND (@estado IS NULL OR o.Estado = @estado)
		AND (@mecanicoId IS NULL OR o.MecanicoId = @mecanicoId)
	ORDER BY o.FechaIngreso DESC, o.Id DESC
	OFFSET @inicio ROWS
	FETCH NEXT @pageSize ROWS ONLY;
	SELECT COUNT(*)
	FROM OrdenesTrabajo o
	INNER JOIN Vehiculos v ON v.Id = o.VehiculoId
	INNER JOIN Clientes c ON c.Id = v.ClienteId
	WHERE (v.Placa LIKE '%' + @buscar + '%' OR c.Nombre LIKE '%' + @buscar + '%')
		AND (@estado IS NULL OR o.Estado = @estado)
		AND (@mecanicoId IS NULL OR o.MecanicoId = @mecanicoId);
END
