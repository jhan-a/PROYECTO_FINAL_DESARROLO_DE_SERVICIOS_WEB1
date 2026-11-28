CREATE OR ALTER PROCEDURE sp_listar_repuestos_paginados
(
	@pageSize INT = 8,
	@page INT = 1,
	@buscar NVARCHAR(100) = '',
	@soloAlerta BIT = 0 /* 1 = solo repuestos que llegaron al stock mínimo */
)
AS
BEGIN
	SET NOCOUNT ON
	DECLARE @inicio INT = (@page - 1) * @pageSize
	SELECT Id, Nombre, Precio, Stock, StockMinimo, Activo
	FROM Repuestos
	WHERE Nombre LIKE '%' + @buscar + '%'
		AND (@soloAlerta = 0 OR Stock <= StockMinimo)
	ORDER BY Nombre
	OFFSET @inicio ROWS
	FETCH NEXT @pageSize ROWS ONLY;
	SELECT COUNT(*) FROM Repuestos
	WHERE Nombre LIKE '%' + @buscar + '%'
		AND (@soloAlerta = 0 OR Stock <= StockMinimo);
END
