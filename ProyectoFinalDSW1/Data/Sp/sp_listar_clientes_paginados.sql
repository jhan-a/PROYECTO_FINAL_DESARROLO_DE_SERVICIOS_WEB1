CREATE OR ALTER PROCEDURE sp_listar_clientes_paginados
(
	@pageSize INT = 8,
	@page INT = 1,
	@buscar NVARCHAR(100) = ''
)
AS
BEGIN
	SET NOCOUNT ON
	DECLARE @inicio INT = (@page - 1) * @pageSize
	SELECT Id, Nombre, Dni, Telefono, Correo, Activo
	FROM Clientes
	WHERE Nombre LIKE '%' + @buscar + '%' OR Dni LIKE '%' + @buscar + '%' OR Correo LIKE '%' + @buscar + '%'
	ORDER BY Nombre
	OFFSET @inicio ROWS
	FETCH NEXT @pageSize ROWS ONLY;
	SELECT COUNT(*) FROM Clientes
	WHERE Nombre LIKE '%' + @buscar + '%' OR Dni LIKE '%' + @buscar + '%' OR Correo LIKE '%' + @buscar + '%';
END
