CREATE FUNCTION [dal].[fn_EntryType__Concept] (
	@Id INT
)
RETURNS NVARCHAR (255)
AS
BEGIN
	RETURN (SELECT [Concept] FROM dbo.EntryTypes WHERE [Id] = @Id)
END