CREATE OR ALTER PROCEDURE sp_listar_usuarios_paginados
(
	@pageSize INT = 8, /* Cantidad de elementos por página */
	@page INT = 1, /* Índice de la página */
	@buscar NVARCHAR(100) = ''
)
AS
BEGIN
	SET NOCOUNT ON
	DECLARE @inicio INT = (@page - 1) * @pageSize
	SELECT Id, Nombre, Correo, Rol, Activo, IntentosFallidos, BloqueadoHasta, FechaRegistro
	FROM Usuarios
	WHERE Nombre LIKE '%' + @buscar + '%' OR Correo LIKE '%' + @buscar + '%'
	ORDER BY Nombre
	OFFSET @inicio ROWS
	FETCH NEXT @pageSize ROWS ONLY;
	SELECT COUNT(*) FROM Usuarios
	WHERE Nombre LIKE '%' + @buscar + '%' OR Correo LIKE '%' + @buscar + '%';
END
