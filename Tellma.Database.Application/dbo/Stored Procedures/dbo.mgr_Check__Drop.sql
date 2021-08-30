CREATE PROCEDURE [dbo].[mgr_Check__Drop]
	@schema_name nvarchar(256) = N'dbo',
	@table_name nvarchar(256),
	@col_name nvarchar(256)
AS
	DECLARE @Command  nvarchar(1000);

	SELECT @Command = 'ALTER TABLE ' + @schema_name + '.[' + @table_name + '] DROP CONSTRAINT ' + d.name
	FROM sys.tables t
	JOIN sys.default_constraints d on d.parent_object_id = t.object_id
	JOIN sys.columns c on c.object_id = t.object_id and c.column_id = d.parent_column_id
	WHERE t.name = @table_name
	AND t.schema_id = schema_id(@schema_name)
	AND c.name = @col_name;
	--PRINT @Command
	EXECUTE (@Command);
GO