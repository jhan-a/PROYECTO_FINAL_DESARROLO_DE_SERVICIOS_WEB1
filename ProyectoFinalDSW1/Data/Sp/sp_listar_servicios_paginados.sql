CREATE OR ALTER PROCEDURE sp_listar_servicios_paginados
(
	@pageSize INT = 8,
	@page INT = 1,
	@buscar NVARCHAR(100) = ''
)
AS
BEGIN
	SET NOCOUNT ON
	DECLARE @inicio INT = (@page - 1) * @pageSize
	SELECT Id, Nombre, Descripcion, Precio, Activo
	FROM Servicios
	WHERE Nombre LIKE '%' + @buscar + '%'
	ORDER BY Nombre
	OFFSET @inicio ROWS
	FETCH NEXT @pageSize ROWS ONLY;
	SELECT COUNT(*) FROM Servicios WHERE Nombre LIKE '%' + @buscar + '%';
END
